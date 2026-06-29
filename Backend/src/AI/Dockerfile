# ============================================================
# NovaHire AI Service — Dockerfile
# Base: Python 3.11 slim + Tesseract OCR (via apt-get)
# Platform: Render (Linux container)
# ============================================================

FROM python:3.11-slim

# ── System dependencies (Tesseract + langue FR/EN) ─────────
RUN apt-get update && apt-get install -y --no-install-recommends \
    tesseract-ocr \
    tesseract-ocr-fra \
    tesseract-ocr-eng \
    libglib2.0-0 \
    libsm6 \
    libxext6 \
    libxrender-dev \
    libgomp1 \
    libgl1 \
    && rm -rf /var/lib/apt/lists/*

# ── Workdir ─────────────────────────────────────────────────
WORKDIR /app

# ── Python dependencies ──────────────────────────────────────
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# ── Download spaCy models ────────────────────────────────────
RUN python -m spacy download fr_core_news_lg && \
    python -m spacy download en_core_web_sm

# ── Copy source code ─────────────────────────────────────────
COPY . .

# ── Tesseract path (Linux) ───────────────────────────────────
ENV TESSERACT_CMD=/usr/bin/tesseract
ENV NOVAHIRE_TESSERACT_CMD=/usr/bin/tesseract

# ── Expose port ──────────────────────────────────────────────
EXPOSE 8000

# ── Launch FastAPI via uvicorn ───────────────────────────────
CMD ["python", "main.py"]
