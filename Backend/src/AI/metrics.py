"""
NovaHire AI Service - Observability
Collects per-stage latency, throughput, error rates, and queue depth.
Exposes metrics via /metrics endpoint.
"""

import time
import threading
import logging
from collections import defaultdict
from typing import Dict, List

logger = logging.getLogger("NovaHire-AI.metrics")


class MetricsCollector:
    """Thread-safe metrics collector with percentile calculation."""

    def __init__(self):
        self._lock = threading.Lock()
        self._stage_latencies: Dict[str, List[float]] = defaultdict(list)
        self._stage_errors: Dict[str, int] = defaultdict(int)
        self._total_jobs = 0
        self._completed_jobs = 0
        self._failed_jobs = 0
        self._retried_jobs = 0
        self._start_time = time.time()

    def record_stage(self, stage_name: str, latency_ms: float) -> None:
        with self._lock:
            self._stage_latencies[stage_name].append(latency_ms)

    def record_stage_error(self, stage_name: str) -> None:
        with self._lock:
            self._stage_errors[stage_name] += 1

    def record_job_submitted(self) -> None:
        with self._lock:
            self._total_jobs += 1

    def record_job_completed(self) -> None:
        with self._lock:
            self._completed_jobs += 1

    def record_job_failed(self) -> None:
        with self._lock:
            self._failed_jobs += 1

    def record_job_retried(self) -> None:
        with self._lock:
            self._retried_jobs += 1

    def _percentile(self, data: List[float], p: float) -> float:
        if not data:
            return 0.0
        sorted_data = sorted(data)
        idx = int(len(sorted_data) * p / 100)
        idx = min(idx, len(sorted_data) - 1)
        return round(sorted_data[idx], 2)

    def snapshot(self) -> dict:
        """Return a snapshot of all metrics."""
        with self._lock:
            stages = {}
            for name, latencies in self._stage_latencies.items():
                stages[name] = {
                    "count": len(latencies),
                    "total_ms": round(sum(latencies), 2),
                    "avg_ms": round(sum(latencies) / len(latencies), 2) if latencies else 0,
                    "p50_ms": self._percentile(latencies, 50),
                    "p95_ms": self._percentile(latencies, 95),
                    "p99_ms": self._percentile(latencies, 99),
                    "errors": self._stage_errors.get(name, 0),
                }

            return {
                "uptime_seconds": round(time.time() - self._start_time, 1),
                "jobs": {
                    "total": self._total_jobs,
                    "completed": self._completed_jobs,
                    "failed": self._failed_jobs,
                    "retried": self._retried_jobs,
                    "throughput_per_min": round(
                        self._completed_jobs / max((time.time() - self._start_time) / 60, 0.01), 2
                    ),
                },
                "stages": stages,
            }

    def reset(self) -> None:
        with self._lock:
            self._stage_latencies.clear()
            self._stage_errors.clear()
            self._total_jobs = 0
            self._completed_jobs = 0
            self._failed_jobs = 0
            self._retried_jobs = 0
            self._start_time = time.time()


# Singleton
metrics = MetricsCollector()
