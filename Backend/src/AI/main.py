"""
NovaHire AI Service - FastAPI Entry Point (Refactored)

Endpoints:
  GET  /health          → service health check
  POST /analyze         → synchronous analysis (backward compatible)
  POST /jobs            → submit async job (returns job_id immediately)
  GET  /jobs/{job_id}   → poll job status / retrieve result
  GET  /metrics         → observability dashboard
  GET  /cache/stats     → cache hit rates
  POST /cache/clear     → flush all caches

Architecture:
  - Pipeline stages are independent modules under pipeline/
  - Async jobs processed by background worker via queue/
  - All scoring cached by content hash for idempotency
"""

import os
import sys
import logging
from contextlib import asynccontextmanager

from fastapi import FastAPI, File, UploadFile, Form, HTTPException, Query
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel

# Add AI directory to Python path for module resolution
sys.path.insert(0, os.path.dirname(__file__))

from config import settings
from models import AnalysisMode, AnalysisResponse, JobStatusResponse, QuizRequest, QuizResponse, JDGenerationRequest, JDGenerationResponse, FeedbackRequest, FeedbackResponse, OfferLetterRequest, OfferLetterResponse
from pipeline.parser import EntityParser
from pipeline.scorer import SemanticScorer
from pipeline.orchestrator import PipelineOrchestrator
from jobqueue.manager import job_queue
from metrics import metrics
from cache import extraction_cache, jd_embedding_cache
from pipeline.quiz_generator import QuizGenerator
from pipeline.jd_generator import JDGenerator

# ── Logging ───────────────────────────────────

logging.basicConfig(
    level=getattr(logging, settings.log_level),
    format="%(asctime)s [%(name)s] %(levelname)s: %(message)s",
)
logger = logging.getLogger("NovaHire-AI")


# ── Lifecycle ─────────────────────────────────

@asynccontextmanager
async def lifespan(app: FastAPI):
    """Initialize pipeline components at startup, cleanup at shutdown."""
    logger.info("=== NovaHire AI Service Starting ===")

    # Initialize pipeline components (loaded once, reused)
    parser = EntityParser()
    scorer = SemanticScorer()

    orchestrator = PipelineOrchestrator(parser, scorer)

    # Wire up job queue
    job_queue.set_orchestrator(orchestrator)
    await job_queue.start_worker()

    # Store in app state for endpoint access
    app.state.orchestrator = orchestrator
    app.state.parser = parser
    app.state.scorer = scorer
    app.state.quiz_generator = QuizGenerator(parser)
    app.state.jd_generator = JDGenerator()

    logger.info("=== NovaHire AI Service Ready ===")
    yield

    # Shutdown
    job_queue.stop()
    logger.info("=== NovaHire AI Service Stopped ===")


app = FastAPI(
    title="NovaHire AI Analysis Service",
    version="2.0.0",
    lifespan=lifespan,
)


class ScoreFromTextRequest(BaseModel):
    cvText: str
    jobTitle: str
    jobDescription: str
    weightExperience: int = 33
    weightEducation: int = 33
    weightSkills: int = 34
    language: str = "fr"

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


# ══════════════════════════════════════════════
# Health Check
# ══════════════════════════════════════════════

@app.get("/health")
async def health_check():
    import requests
    
    # Check Groq status (based on config)
    groq_status = "configured" if settings.groq_api_key else "missing_key"
    
    # Check Ollama status (ping)
    ollama_status = "offline"
    try:
        # Ping the tags endpoint which is light
        resp = requests.get(f"{settings.ollama_url}/api/tags", timeout=1)
        if resp.status_code == 200:
            ollama_status = "online"
    except:
        pass

    return {
        "status": "healthy",
        "service": "novahire-ai-local",
        "version": "2.0.0",
        "providers": {
            "groq": groq_status,
            "ollama": ollama_status
        },
        "queue_depth": job_queue.queue_depth,
        "dead_letter_count": job_queue.dead_letter_count,
    }


# ══════════════════════════════════════════════
# Synchronous Analysis (backward compatible)
# ══════════════════════════════════════════════

@app.post("/analyze", response_model=AnalysisResponse)
async def analyze_cv(
    file: UploadFile = File(...),
    jobTitle: str = Form(...),
    jobDescription: str = Form(...),
    weightExperience: int = Form(33),
    weightEducation: int = Form(33),
    weightSkills: int = Form(34),
    mode: str = Form("full_analysis"),
    language: str = Form("fr"),
):
    """
    Synchronous CV analysis. Backward compatible with v1 API.
    Blocks until analysis completes. For high-throughput use /jobs instead.
    """
    # Formats acceptés : PDF et DOCX
    allowed_ext = tuple(f".{fmt}" for fmt in ["pdf", "docx", "doc"])
    if not file.filename.lower().endswith(allowed_ext):
        raise HTTPException(
            status_code=400,
            detail="Seuls les fichiers PDF et DOCX sont acceptés.",
        )

    try:
        analysis_mode = AnalysisMode(mode)
    except ValueError:
        analysis_mode = AnalysisMode.FULL

    try:
        content = await file.read()
        orchestrator: PipelineOrchestrator = app.state.orchestrator

        result, timings = orchestrator.run(
            file_content=content,
            filename=file.filename,
            job_title=jobTitle,
            job_description=jobDescription,
            weight_exp=weightExperience,
            weight_edu=weightEducation,
            weight_skills=weightSkills,
            mode=analysis_mode,
        )

        # Record metrics
        for stage_name, ms in timings.items():
            metrics.record_stage(stage_name, ms)
        metrics.record_job_submitted()
        metrics.record_job_completed()

        logger.info(
            f"Sync analysis complete: score={result.overallScore}, "
            f"total={sum(timings.values()):.0f}ms"
        )
        return result

    except ValueError as ve:
        raise HTTPException(status_code=400, detail=str(ve))
    except Exception as e:
        logger.error(f"Analysis error: {e}")
        metrics.record_job_failed()
        raise HTTPException(status_code=500, detail=str(e))


# ══════════════════════════════════════════════
# Async Job Submission
# ══════════════════════════════════════════════

@app.post("/jobs")
async def submit_job(
    file: UploadFile = File(...),
    jobTitle: str = Form(...),
    jobDescription: str = Form(...),
    weightExperience: int = Form(33),
    weightEducation: int = Form(33),
    weightSkills: int = Form(34),
    mode: str = Form("full_analysis"),
    language: str = Form("fr"),
):
    """
    Submit CV for async analysis. Returns job_id immediately.
    Poll GET /jobs/{job_id} for status and results.
    """
    # Formats acceptés : PDF et DOCX
    allowed_ext = tuple(f".{fmt}" for fmt in ["pdf", "docx", "doc"])
    if not file.filename.lower().endswith(allowed_ext):
        raise HTTPException(
            status_code=400,
            detail="Seuls les fichiers PDF et DOCX sont acceptés.",
        )

    try:
        analysis_mode = AnalysisMode(mode)
    except ValueError:
        analysis_mode = AnalysisMode.FULL

    content = await file.read()

    job_id = job_queue.submit(
        file_content=content,
        job_title=jobTitle,
        job_description=jobDescription,
        weight_exp=weightExperience,
        weight_edu=weightEducation,
        weight_skills=weightSkills,
        mode=analysis_mode,
        language=language,
    )

    return {
        "job_id": job_id,
        "status": "queued",
        "poll_url": f"/jobs/{job_id}",
    }


@app.get("/jobs/{job_id}", response_model=JobStatusResponse)
async def get_job_status(job_id: str):
    """Poll job status. Returns result when completed."""
    status = job_queue.get_status(job_id)
    if not status:
        raise HTTPException(status_code=404, detail="Job not found.")
    return status


@app.post("/score-from-text", response_model=AnalysisResponse)
async def score_from_text(payload: ScoreFromTextRequest):
    """
    Score/summarize from pre-extracted CV text (no PDF extraction stage).
    """
    if not payload.cvText or not payload.cvText.strip():
        raise HTTPException(status_code=400, detail="cvText est requis.")

    try:
        orchestrator: PipelineOrchestrator = app.state.orchestrator

        result, timings = orchestrator.run_from_text(
            cv_text=payload.cvText,
            job_description=payload.jobDescription,
            weight_exp=payload.weightExperience,
            weight_edu=payload.weightEducation,
            weight_skills=payload.weightSkills,
            language=payload.language,
        )

        for stage_name, ms in timings.items():
            metrics.record_stage(stage_name, ms)
        metrics.record_job_submitted()
        metrics.record_job_completed()
        return result
    except ValueError as ve:
        raise HTTPException(status_code=400, detail=str(ve))
    except Exception as e:
        logger.error(f"Score-from-text error: {e}")
        metrics.record_job_failed()
        raise HTTPException(status_code=500, detail=str(e))


# ══════════════════════════════════════════════
# Quiz Generation
# ══════════════════════════════════════════════

@app.post("/generate-quiz", response_model=QuizResponse)
async def generate_quiz(request: QuizRequest):
    """
    Generate a technical and soft-skill quiz based on Job Description.
    """
    try:
        generator: QuizGenerator = app.state.quiz_generator
        result = generator.generate(request)
        return result
    except Exception as e:
        logger.error(f"Quiz generation error: {e}")
        raise HTTPException(status_code=500, detail=str(e))


# ══════════════════════════════════════════════
# Observability
# ══════════════════════════════════════════════

@app.get("/metrics")
async def get_metrics():
    """Operational metrics: throughput, latency percentiles, error rates."""
    data = metrics.snapshot()
    data["queue"] = {
        "depth": job_queue.queue_depth,
        "dead_letter": job_queue.dead_letter_count,
    }
    return data


@app.get("/cache/stats")
async def cache_stats():
    """Cache hit rates for extraction and JD embeddings."""
    return {
        "extraction_cache": extraction_cache.stats,
        "jd_embedding_cache": jd_embedding_cache.stats,
    }


@app.post("/cache/clear")
async def clear_caches():
    """Flush all caches. Use after model updates or taxonomy changes."""
    extraction_cache.clear()
    jd_embedding_cache.clear()
    return {"status": "cleared"}


@app.post("/generate-jd", response_model=JDGenerationResponse)
async def generate_jd(request: JDGenerationRequest):
    try:
        generator: JDGenerator = app.state.jd_generator
        response = generator.generate(request)
        return response
    except Exception as e:
        logger.error(f"JD Generation Error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/generate-feedback", response_model=FeedbackResponse)
async def generate_feedback(request: FeedbackRequest):
    """
    Generate a constructive rejection email using LLM.
    """
    try:
        orchestrator: PipelineOrchestrator = app.state.orchestrator
        feedback = orchestrator.generate_feedback(
            candidate_name=request.candidateName,
            job_title=request.jobTitle,
            recruiter_notes=request.recruiterNotes,
            language=request.language
        )
        return FeedbackResponse(feedback=feedback)
    except Exception as e:
        logger.error(f"Feedback generation error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/generate-offer-letter", response_model=OfferLetterResponse)
async def generate_offer_letter(request: OfferLetterRequest):
    """
    Generate a professional offer letter using LLM.
    """
    try:
        orchestrator: PipelineOrchestrator = app.state.orchestrator
        offer_letter = orchestrator.generate_offer_letter(
            candidate_name=request.candidateName,
            job_title=request.jobTitle,
            salary=request.salary,
            currency=request.currency,
            start_date=request.startDate,
            benefits=request.benefits,
            recruiter_name=request.recruiterName,
            company_name=request.companyName,
            language=request.language
        )
        return OfferLetterResponse(offerLetter=offer_letter)
    except Exception as e:
        logger.error(f"Offer letter generation error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

# ══════════════════════════════════════════════
# Entry Point
# ══════════════════════════════════════════════

if __name__ == "__main__":
    import uvicorn

    uvicorn.run(
        "main:app",
        host=settings.host,
        port=settings.port,
        workers=1,  # single worker — models are not fork-safe
        log_level=settings.log_level.lower(),
    )
