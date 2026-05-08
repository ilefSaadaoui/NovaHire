import logging
from typing import List, Dict

from models import LanguageResult

logger = logging.getLogger("NovaHire-AI.analyzers")

# Les analyseurs sont maintenant simplifiés car Ollama fait le travail lourd dans l'étape de parsing.

def calculate_education_score(entities_edu_score: float) -> float:
    """Retourne simplement le score calculé par l'LLM."""
    return entities_edu_score

def calculate_seniority(entities_xp_years: float) -> float:
    """Retourne simplement les années d'expérience calculées par l'LLM."""
    return entities_xp_years

def detect_languages(entities_languages: List[str]) -> List[str]:
    """Retourne la liste des langues détectées par l'LLM."""
    return entities_languages

def score_language_match(
    cv_languages: List[str], jd_requirements: List[Dict[str, str]]
) -> LanguageResult:
    """
    Compare les langues du CV avec les exigences du poste.
    On garde cette logique car elle compare deux sources d'informations.
    """
    if not jd_requirements:
        return LanguageResult(
            detected=cv_languages,
            requirements=jd_requirements,
            match_score=100.0,
        )

    # Logique simplifiée de matching
    total_score = 0
    gaps = []
    
    cv_lang_lower = [l.lower() for l in cv_languages]
    
    for req in jd_requirements:
        req_lang = req["language"].lower()
        found = False
        for cv_l in cv_lang_lower:
            if req_lang in cv_l:
                found = True
                total_score += 100
                break
        
        if not found:
            gaps.append(f"Langue requise [{req['language']}] non détectée clairement.")

    score = (total_score / len(jd_requirements)) if jd_requirements else 100.0
    return LanguageResult(
        detected=cv_languages,
        requirements=jd_requirements,
        match_score=score,
        gaps=gaps,
    )

def calculate_confidence(text: str, entities: dict) -> float:
    """Score de confiance basé sur la richesse des données extraites."""
    confidence = 1.0
    if len(text) < 500:
        confidence *= 0.5
    if not entities.get("skills"):
        confidence *= 0.7
    return round(max(min(confidence, 1.0), 0.1), 2)
