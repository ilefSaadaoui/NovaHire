"""
NovaHire AI Service - Async Job Queue
In-memory job queue with background worker processing.

Flow:
  1. POST /jobs  → submit job → returns job_id immediately
  2. GET  /jobs/{id} → poll status (queued → processing → completed/failed)
  3. Background worker picks jobs from queue, runs pipeline, stores result

Features:
  - Automatic retries with exponential backoff (max 3)
  - Dead-letter tracking for permanently failed jobs
  - TTL-based cleanup of completed jobs
  - Idempotent resubmission via file content hash
"""

import asyncio
import hashlib
import logging
import os
import json
import time
import uuid
import threading
from collections import OrderedDict
from datetime import datetime, timedelta
from typing import Dict, Optional

from config import settings
from models import (
    AnalysisMode, AnalysisResponse, JobRecord, JobStatus, JobStatusResponse,
)
from metrics import metrics

logger = logging.getLogger("NovaHire-AI.queue")


class JobQueue:
    """
    Thread-safe in-memory job queue with background worker.
    For production scale, swap the internal dict for Redis/RabbitMQ.
    """

    def __init__(self):
        self._jobs: OrderedDict[str, JobRecord] = OrderedDict()
        self._job_data: Dict[str, dict] = {}  # raw inputs keyed by job_id
        self._queue: asyncio.Queue = asyncio.Queue()
        self._lock = threading.Lock()
        self._dead_letter: list[str] = []
        self._idempotency_map: Dict[str, str] = {}  # content_hash → job_id
        self._orchestrator = None  # set by main.py at startup
        self._running = False

    def set_orchestrator(self, orchestrator) -> None:
        self._orchestrator = orchestrator

    # ── Submit ────────────────────────────────

    def submit(
        self,
        file_content: bytes,
        job_title: str,
        job_description: str,
        weight_exp: int = 33,
        weight_edu: int = 33,
        weight_skills: int = 34,
        mode: AnalysisMode = AnalysisMode.FULL,
    ) -> str:
        """
        Submit a job. Returns job_id immediately.
        Idempotent: same file + JD returns existing job_id if still alive.
        """
        # Idempotency check
        content_hash = hashlib.sha256(
            file_content + job_description.encode()
        ).hexdigest()

        with self._lock:
            if content_hash in self._idempotency_map:
                existing_id = self._idempotency_map[content_hash]
                if existing_id in self._jobs:
                    existing = self._jobs[existing_id]
                    if existing.status in (JobStatus.QUEUED, JobStatus.PROCESSING):
                        logger.info(f"Idempotent hit: reusing job {existing_id}")
                        return existing_id

        job_id = str(uuid.uuid4())
        record = JobRecord(
            job_id=job_id,
            status=JobStatus.QUEUED,
            mode=mode,
            created_at=datetime.utcnow(),
        )

        with self._lock:
            self._jobs[job_id] = record
            self._job_data[job_id] = {
                "file_content": file_content,
                "job_title": job_title,
                "job_description": job_description,
                "weight_exp": weight_exp,
                "weight_edu": weight_edu,
                "weight_skills": weight_skills,
                "mode": mode,
            }
            self._idempotency_map[content_hash] = job_id

        self._queue.put_nowait(job_id)
        metrics.record_job_submitted()
        logger.info(f"Job {job_id} queued (mode={mode.value})")
        return job_id

    # ── Status ────────────────────────────────

    def get_status(self, job_id: str) -> Optional[JobStatusResponse]:
        with self._lock:
            record = self._jobs.get(job_id)
            if not record:
                return None
            return JobStatusResponse(
                job_id=record.job_id,
                status=record.status,
                created_at=record.created_at,
                started_at=record.started_at,
                completed_at=record.completed_at,
                retries=record.retries,
                error=record.error,
                result=record.result,
                stage_timings=record.stage_timings,
            )

    @property
    def queue_depth(self) -> int:
        return self._queue.qsize()

    @property
    def dead_letter_count(self) -> int:
        return len(self._dead_letter)

    # ── Worker ────────────────────────────────

    async def start_worker(self) -> None:
        """Start the background worker loop. Called once at FastAPI startup."""
        if self._running:
            return
        self._running = True
        logger.info("Job queue worker started.")
        asyncio.create_task(self._worker_loop())
        asyncio.create_task(self._cleanup_loop())

    async def _worker_loop(self) -> None:
        while self._running:
            try:
                job_id = await asyncio.wait_for(
                    self._queue.get(), timeout=5.0
                )
            except asyncio.TimeoutError:
                continue

            await self._process_job(job_id)

    async def _process_job(self, job_id: str) -> None:
        with self._lock:
            record = self._jobs.get(job_id)
            data = self._job_data.get(job_id)
        if not record or not data:
            return

        record.status = JobStatus.PROCESSING
        record.started_at = datetime.utcnow()

        try:
            # Run pipeline in executor to avoid blocking event loop
            loop = asyncio.get_event_loop()
            result, timings = await loop.run_in_executor(
                None,
                lambda: self._orchestrator.run(
                    file_content=data["file_content"],
                    job_title=data["job_title"],
                    job_description=data["job_description"],
                    weight_exp=data["weight_exp"],
                    weight_edu=data["weight_edu"],
                    weight_skills=data["weight_skills"],
                    mode=data["mode"],
                ),
            )

            record.status = JobStatus.COMPLETED
            record.completed_at = datetime.utcnow()
            record.result = result
            record.stage_timings = timings
            metrics.record_job_completed()

            # Record per-stage metrics
            for stage_name, ms in timings.items():
                metrics.record_stage(stage_name, ms)

            logger.info(
                f"Job {job_id} completed "
                f"(score={result.overallScore}, "
                f"total={sum(timings.values()):.0f}ms)"
            )

            # Feedback log
            self._log_feedback(job_id, result)

        except Exception as e:
            record.retries += 1
            if record.retries < record.max_retries:
                record.status = JobStatus.RETRYING
                record.error = str(e)
                metrics.record_job_retried()
                logger.warning(
                    f"Job {job_id} failed (attempt {record.retries}), retrying: {e}"
                )
                # Exponential backoff
                await asyncio.sleep(2 ** record.retries)
                self._queue.put_nowait(job_id)
            else:
                record.status = JobStatus.FAILED
                record.error = str(e)
                record.completed_at = datetime.utcnow()
                self._dead_letter.append(job_id)
                metrics.record_job_failed()
                logger.error(f"Job {job_id} permanently failed after {record.retries} retries: {e}")

    def _log_feedback(self, job_id: str, result: AnalysisResponse) -> None:
        try:
            os.makedirs(settings.feedback_log_dir, exist_ok=True)
            log_file = os.path.join(
                settings.feedback_log_dir, f"analysis_{job_id[:10]}.json"
            )
            with open(log_file, "w", encoding="utf-8") as f:
                json.dump(result.model_dump(), f, ensure_ascii=False, indent=2, default=str)
        except Exception as e:
            logger.warning(f"Failed to log feedback: {e}")

    async def _cleanup_loop(self) -> None:
        """Remove expired jobs to prevent memory leak."""
        while self._running:
            await asyncio.sleep(300)  # every 5 minutes
            cutoff = datetime.utcnow() - timedelta(seconds=settings.job_ttl_seconds)
            with self._lock:
                expired = [
                    jid for jid, rec in self._jobs.items()
                    if rec.status in (JobStatus.COMPLETED, JobStatus.FAILED)
                    and rec.completed_at
                    and rec.completed_at < cutoff
                ]
                for jid in expired:
                    del self._jobs[jid]
                    self._job_data.pop(jid, None)
                if expired:
                    logger.info(f"Cleaned up {len(expired)} expired jobs.")

    def stop(self) -> None:
        self._running = False


# Singleton
job_queue = JobQueue()
