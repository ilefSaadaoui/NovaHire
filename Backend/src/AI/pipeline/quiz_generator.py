import logging
import json
from typing import List
from groq import Groq
from models import QuizResponse, QuizQuestionModel, QuizRequest
from config import settings

logger = logging.getLogger("NovaHire-AI.quiz")

class QuizGenerator:
    def __init__(self, parser=None):
        self.parser = parser
        self.groq_client = Groq(api_key=settings.groq_api_key) if settings.groq_api_key else None

    def generate(self, request: QuizRequest) -> QuizResponse:
        if not self.groq_client:
            raise ValueError("GROQ_API_KEY non configurée. Impossible d'utiliser l'IA.")

        logger.info(f"Generating quiz for: {request.jobTitle} using Groq (llama3-8b-8192)")
        
        prompt = f"""You are a seasoned HR technology specialist tasked with designing a targeted assessment quiz for a specific job role.

Generate a quiz of {request.numQuestions} questions in {request.language}, calibrated to '{request.difficulty}' proficiency level for the position of {request.jobTitle}.

PARAMETERS:
- Content Scope: Use the job description below as context to ensure relevance.
- Topic Emphasis: {f'At least 50% of questions MUST focus on: {", ".join(request.topics)}' if request.topics else 'Standard mix of Technical and SoftSkill questions.'}
- Question Complexity: Adjust difficulty and depth to match '{request.difficulty}' level precisely.
- Question Types: Each question "type" MUST be exactly "Technical" or "SoftSkill".

JOB DESCRIPTION:
{request.jobDescription[:1000]}

OUTPUT — JSON ONLY, no extra text:
{{
    "title": "Quiz: {request.jobTitle}",
    "description": "Assessment for {request.jobTitle} at {request.difficulty} level",
    "questions": [
        {{
            "text": "The question prompt",
            "options": ["A", "B", "C", "D"],
            "correctAnswerIndex": 0,
            "explanation": "Detailed rationale for the correct answer",
            "type": "Technical"
        }}
    ]
}}"""
        try:
            response = self.groq_client.chat.completions.create(
                model="llama-3.1-8b-instant",
                messages=[{"role": "user", "content": prompt}],
                temperature=0.7,
                response_format={"type": "json_object"}
            )
            
            result_json = response.choices[0].message.content.strip()
            data = json.loads(result_json)
            
            # Validation via Pydantic
            quiz = QuizResponse(**data)
            return quiz
            
        except Exception as e:
            logger.error(f"Groq generation error: {e}")
            raise Exception("Erreur lors de la génération avec Groq API. Vérifiez votre clé API ou votre connexion internet.")
