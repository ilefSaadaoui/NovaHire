"""
NovaHire AI Service - Caching Layer
LRU caches for PDF extraction results and JD embeddings.
Keyed by content hash to guarantee idempotency.
"""

import hashlib
import time
import threading
import logging
from typing import Optional, Any
from collections import OrderedDict

from config import settings

logger = logging.getLogger("NovaHire-AI.cache")


class TTLCache:
    """Thread-safe LRU cache with TTL expiration."""

    def __init__(self, maxsize: int, ttl_seconds: int):
        self._maxsize = maxsize
        self._ttl = ttl_seconds
        self._store: OrderedDict[str, tuple[float, Any]] = OrderedDict()
        self._lock = threading.Lock()
        self._hits = 0
        self._misses = 0

    def get(self, key: str) -> Optional[Any]:
        with self._lock:
            if key in self._store:
                ts, value = self._store[key]
                if time.time() - ts < self._ttl:
                    self._store.move_to_end(key)
                    self._hits += 1
                    return value
                else:
                    del self._store[key]
            self._misses += 1
            return None

    def put(self, key: str, value: Any) -> None:
        with self._lock:
            if key in self._store:
                self._store.move_to_end(key)
            self._store[key] = (time.time(), value)
            while len(self._store) > self._maxsize:
                self._store.popitem(last=False)

    def clear(self) -> None:
        with self._lock:
            self._store.clear()
            self._hits = 0
            self._misses = 0

    @property
    def stats(self) -> dict:
        total = self._hits + self._misses
        return {
            "size": len(self._store),
            "maxsize": self._maxsize,
            "hits": self._hits,
            "misses": self._misses,
            "hit_rate": round(self._hits / total, 3) if total > 0 else 0.0,
        }


def content_hash(data: bytes) -> str:
    """SHA-256 hash of raw bytes for cache keying."""
    return hashlib.sha256(data).hexdigest()


def text_hash(text: str) -> str:
    """SHA-256 hash of text for cache keying."""
    return hashlib.sha256(text.encode("utf-8")).hexdigest()


# ── Singleton caches ──────────────────────────

extraction_cache = TTLCache(
    maxsize=settings.extraction_cache_maxsize,
    ttl_seconds=settings.cache_ttl_seconds,
)

jd_embedding_cache = TTLCache(
    maxsize=settings.jd_embedding_cache_maxsize,
    ttl_seconds=settings.cache_ttl_seconds,
)
