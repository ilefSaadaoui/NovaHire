import logging
import json
import ollama
from typing import List, Dict

logger = logging.getLogger("NovaHire-AI.scorer")

class SemanticScorer:
    """
    Ollama-backed semantic scorer.
    Uses LLM reasoning to evaluate the match between a candidate and a job.
    """

    def __init__(self):
        logger.info("SemanticScorer initialized (Ollama Mode)")

    def calculate_match(self, cv_summary: str, jd_text: str) -> Dict:
        """
        Calculates a nuanced matching score using Ollama.
        Returns detailed scores and professional rationale.
        """
        logger.info("Calculating match score using Ollama")
        
        prompt = f"""
Comparez ce profil de candidat avec l'offre d'emploi suivante.
RÉSUMÉ DU CANDIDAT :
{cv_summary}

OFFRE D'EMPLOI :
{jd_text[:2000]}

Évaluez l'adéquation du profil. 
Vous DEVEZ répondre UNIQUEMENT en JSON avec cette structure :
{{
    "overall": 85,
    "experience_score": 80,
    "education_score": 90,
    "skills_score": 85,
    "strengths": ["point fort 1", "point fort 2"],
    "weaknesses": ["point faible 1", "point faible 2"],
    "recommendation": "Avis d'expert court"
}}
Note : Les scores sont sur 100.
"""
        try:
            response = ollama.chat(
                model='llama3',
                messages=[{'role': 'user', 'content': prompt}],
                format='json'
            )
            return json.loads(response['message']['content'])
        except Exception as e:
            logger.error(f"Error calculating match with Ollama: {e}")
            return {
                "score": 50,
                "strengths": [],
                "weaknesses": ["Erreur technique lors de l'analyse"],
                "recommendation": "Analyse indisponible."
            }

    # Les anciennes méthodes sont gardées vides pour éviter de casser l'orchestrateur immédiatement
    def calculate_similarity(self, cv_text: str, jd_text: str) -> float:
        return 0.0 

    def match_skills_semantically(self, text: str, threshold: float = 0.7) -> List[str]:
        return []
