"""
NovaHire AI Service - Configuration
Centralized settings for all pipeline stages, models, caching, and concurrency.
"""

import os
from pydantic_settings import BaseSettings
from typing import Optional


class Settings(BaseSettings):
    # --- Server ---
    host: str = "0.0.0.0"
    port: int = int(os.getenv("PORT", "8000"))

    # --- spaCy ---
    spacy_models: list[str] = [
        "fr_core_news_lg",
        "fr_core_news_md",
        "fr_core_news_sm",
        "en_core_web_sm",
    ]

    # --- Sentence Transformer ---
    # paraphrase-multilingual-mpnet-base-v2 : meilleur pour CV/offre multilingue FR/EN
    embedding_model_primary: str = "paraphrase-multilingual-mpnet-base-v2"
    embedding_model_fallback: str = "paraphrase-multilingual-MiniLM-L12-v2"

    # --- OCR ---
    # Tesseract est l'OCR principal (rapide, précis, support FR natif)
    ocr_languages: list[str] = ["fr", "en"]
    tesseract_lang: str = "fra+eng"          # format Tesseract (ISO 639-3)
    ocr_min_text_threshold: int = 100        # chars en dessous duquel l'OCR se déclenche
    ocr_dpi_scale: int = 2                   # échelle de rendu pour la qualité OCR
    # Chemin explicite vers le binaire Tesseract (Windows)
    # Mis automatiquement si vide, sinon utilise ce chemin
    tesseract_cmd: str = r"C:\Program Files\Tesseract-OCR\tesseract.exe"
    # Formats acceptés
    supported_formats: list[str] = ["pdf", "docx", "doc", "png", "jpg", "jpeg"]

    # --- Scoring ---
    semantic_skill_threshold: float = 0.65
    default_weight_experience: int = 33
    default_weight_education: int = 33
    default_weight_skills: int = 34

    # --- Concurrency ---
    max_cpu_workers: int = int(os.getenv("MAX_CPU_WORKERS", "4"))
    max_gpu_workers: int = int(os.getenv("MAX_GPU_WORKERS", "2"))
    job_timeout_seconds: int = 300
    job_ttl_seconds: int = 3600  # results kept for 1 hour

    # --- Cache ---
    extraction_cache_maxsize: int = 200
    jd_embedding_cache_maxsize: int = 500
    cache_ttl_seconds: int = 1800  # 30 minutes

    # --- Groq API ---
    groq_api_key: Optional[str] = os.getenv("GROQ_API_KEY", "")

    # --- Ollama ---
    ollama_url: str = os.getenv("OLLAMA_URL", "http://localhost:11434")

    # --- Logging ---
    log_level: str = os.getenv("LOG_LEVEL", "INFO")
    feedback_log_dir: str = "logs/analysis_feedback"

    # --- Taxonomy ---
    taxonomy_path: str = os.path.join(os.path.dirname(__file__), "skills_taxonomy.json")

    model_config = {"env_prefix": "NOVAHIRE_", "env_file": ".env", "extra": "ignore"}


settings = Settings()
