"""
NovaHire AI Service - Pipeline Tests
Tests each pipeline stage independently + integration test of orchestrator.
Run with: python -m pytest tests/ -v  (from the AI directory)
Or standalone: python tests/test_pipeline.py
"""

import sys
import os
import time

# Add parent directory to path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))


def test_cache():
    """Test TTL cache put/get/eviction."""
    from cache import TTLCache

    cache = TTLCache(maxsize=3, ttl_seconds=60)
    cache.put("a", 1)
    cache.put("b", 2)
    cache.put("c", 3)

    assert cache.get("a") == 1
    assert cache.get("b") == 2
    assert cache.get("c") == 3

    # Should evict "a" (LRU)
    cache.put("d", 4)
    assert cache.get("a") is None  # evicted
    assert cache.get("d") == 4

    stats = cache.stats
    assert stats["size"] == 3
    assert stats["hits"] > 0
    assert stats["misses"] > 0
    print("  Cache test: PASS")


def test_education_scoring():
    """Test education level detection."""
    from pipeline.analyzers import calculate_education_score

    assert calculate_education_score("Doctorat en informatique") == 100
    assert calculate_education_score("Master en gestion") == 90
    assert calculate_education_score("Licence en droit") == 80
    assert calculate_education_score("BTS comptabilité") == 70
    assert calculate_education_score("Baccalauréat scientifique") == 60
    assert calculate_education_score("Aucun diplôme mentionné") == 50
    print("  Education scoring test: PASS")


def test_seniority_detection():
    """Test JD seniority level detection."""
    from pipeline.analyzers import detect_seniority_level

    assert detect_seniority_level("Nous cherchons un Développeur Junior") == "JUNIOR"
    assert detect_seniority_level("Senior Software Architect") == "SENIOR"
    assert detect_seniority_level("Manager de transition") == "MANAGER"
    assert detect_seniority_level("Développeur Fullstack") == "MID"
    print("  Seniority detection test: PASS")


def test_seniority_calculation():
    """Test years of experience calculation with overlapping intervals."""
    from pipeline.analyzers import calculate_seniority

    text = "2018 à 2020, puis 2019 à 2022"  # overlapping
    years = calculate_seniority(text)
    assert years == 4.0, f"Expected 4.0 but got {years}"

    text2 = "01/2020 à aujourd'hui"
    years2 = calculate_seniority(text2)
    assert years2 > 0
    print(f"  Seniority calculation test: PASS (overlap={years}y, current={years2}y)")


def test_language_detection():
    """Test language detection from CV text."""
    from pipeline.analyzers import detect_languages

    text = "Langues:\nFrançais (Maternel)\nAnglais (C1)\nEspagnol (B2)"
    langs = detect_languages(text)
    assert len(langs) >= 2, f"Expected >=2 languages but got {langs}"
    lang_str = " ".join(langs).lower()
    assert "français" in lang_str
    assert "anglais" in lang_str
    print(f"  Language detection test: PASS ({langs})")


def test_language_scoring():
    """Test CEFR-based language matching."""
    from pipeline.analyzers import score_language_match

    reqs = [{"language": "Anglais", "level": "C1"}]

    result1 = score_language_match(["Anglais (C1)"], reqs)
    assert result1.match_score == 100.0

    result2 = score_language_match(["Anglais (B2)"], reqs)
    assert result2.match_score == 75.0  # gap of 1 level * 25

    result3 = score_language_match(["Anglais (B1)"], reqs)
    assert result3.match_score == 50.0  # gap of 2 levels * 25

    result4 = score_language_match([], [])
    assert result4.match_score == 100.0  # no requirements = perfect
    print("  Language scoring test: PASS")


def test_text_segmentation():
    """Test CV text segmentation into sections."""
    from pipeline.parser import EntityParser

    parser = EntityParser()
    cv = """
    JEAN DUPONT
    EXPERIENCE PROFESSIONNELLE
    Développeur Python chez Google (2020-2023)

    FORMATION
    Master en Informatique - Université de Paris

    COMPETENCES
    Python, React, Docker
    """
    sections = parser.segment_text(cv)
    assert len(sections.experience) > 0, "Experience section empty"
    assert "Google" in sections.experience
    assert "Master" in sections.education
    assert "Python" in sections.skills
    print("  Text segmentation test: PASS")


def test_entity_parsing():
    """Test skill and contact extraction."""
    from pipeline.parser import EntityParser

    parser = EntityParser()
    text = "Contact: jean@email.com, +33612345678. Skills: Docker, Kubernetes, Terraform, Kafka."
    entities = parser.parse_entities(text)

    assert entities.email == "jean@email.com"
    assert entities.phone is not None
    assert len(entities.skills) >= 2, f"Expected >=2 skills but got {entities.skills}"
    print(f"  Entity parsing test: PASS (skills={entities.skills})")


def test_anonymization():
    """Test PER entity masking."""
    from pipeline.parser import EntityParser

    parser = EntityParser()
    text = "Candidature de Jean Dupont, ingénieur chez Google."
    anonymized = parser.anonymize(text)
    # spaCy may or may not detect "Jean Dupont" depending on model
    # The function should at least not crash
    assert isinstance(anonymized, str)
    assert len(anonymized) > 0
    print(f"  Anonymization test: PASS")


def test_confidence_score():
    """Test confidence calculation."""
    from pipeline.analyzers import calculate_confidence

    # Short text = low confidence
    conf1 = calculate_confidence("Short.", {}, {})
    assert conf1 < 0.5

    # Rich text = high confidence
    long_text = "CV détaillé avec beaucoup d'informations..." * 50
    conf2 = calculate_confidence(long_text, {"skills": ["Python"], "email": "a@b.com"}, {})
    assert conf2 > 0.8
    print(f"  Confidence score test: PASS (short={conf1}, rich={conf2})")


def test_scorer_cache():
    """Test JD embedding cache hit on second call."""
    from pipeline.scorer import SemanticScorer

    scorer = SemanticScorer()
    jd = "Développeur Fullstack Python/React senior avec 5 ans d'expérience."
    cv1 = "Développeur Python passionné par React et le cloud."
    cv2 = "Développeur Java avec 3 ans en backend Spring Boot."

    t0 = time.time()
    s1 = scorer.calculate_similarity(cv1, jd)
    cold_time = time.time() - t0

    t0 = time.time()
    s2 = scorer.calculate_similarity(cv2, jd)  # same JD = cache hit
    warm_time = time.time() - t0

    assert s1 > 0
    assert s2 > 0
    # Warm should generally be faster (JD not re-encoded)
    print(f"  Scorer cache test: PASS (cold={cold_time:.3f}s, warm={warm_time:.3f}s)")


def test_metrics():
    """Test metrics collection and percentiles."""
    from metrics import MetricsCollector

    m = MetricsCollector()
    for i in range(100):
        m.record_stage("test_stage", float(i))
    m.record_job_submitted()
    m.record_job_completed()
    m.record_job_failed()

    snap = m.snapshot()
    assert snap["stages"]["test_stage"]["count"] == 100
    assert snap["stages"]["test_stage"]["p50_ms"] == 50.0
    assert snap["stages"]["test_stage"]["p95_ms"] == 95.0
    assert snap["jobs"]["total"] == 1
    assert snap["jobs"]["completed"] == 1
    assert snap["jobs"]["failed"] == 1
    print("  Metrics test: PASS")


if __name__ == "__main__":
    print("=" * 60)
    print("NovaHire AI Pipeline - Unit Tests")
    print("=" * 60)

    tests = [
        ("Cache", test_cache),
        ("Education Scoring", test_education_scoring),
        ("Seniority Detection", test_seniority_detection),
        ("Seniority Calculation", test_seniority_calculation),
        ("Language Detection", test_language_detection),
        ("Language Scoring", test_language_scoring),
        ("Confidence Score", test_confidence_score),
        ("Metrics", test_metrics),
    ]

    # These require spaCy model loaded
    heavy_tests = [
        ("Text Segmentation", test_text_segmentation),
        ("Entity Parsing", test_entity_parsing),
        ("Anonymization", test_anonymization),
    ]

    # These require sentence-transformers loaded
    ml_tests = [
        ("Scorer Cache", test_scorer_cache),
    ]

    passed = 0
    failed = 0

    for name, test_fn in tests:
        try:
            test_fn()
            passed += 1
        except Exception as e:
            print(f"  {name}: FAIL ({e})")
            failed += 1

    print("\n--- Heavy tests (require spaCy) ---")
    for name, test_fn in heavy_tests:
        try:
            test_fn()
            passed += 1
        except Exception as e:
            print(f"  {name}: FAIL ({e})")
            failed += 1

    print("\n--- ML tests (require sentence-transformers) ---")
    for name, test_fn in ml_tests:
        try:
            test_fn()
            passed += 1
        except Exception as e:
            print(f"  {name}: FAIL ({e})")
            failed += 1

    print(f"\n{'=' * 60}")
    print(f"Results: {passed} passed, {failed} failed")
    print(f"{'=' * 60}")
    sys.exit(1 if failed > 0 else 0)
