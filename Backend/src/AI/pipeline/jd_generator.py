import logging
import json
from groq import Groq
from models import JDGenerationRequest, JDGenerationResponse
from config import settings

logger = logging.getLogger("NovaHire-AI.jd")

class JDGenerator:
    def __init__(self):
        self.groq_client = Groq(api_key=settings.groq_api_key) if settings.groq_api_key else None

    def generate(self, request: JDGenerationRequest) -> JDGenerationResponse:
        if not self.groq_client:
            raise ValueError("GROQ_API_KEY non configurée. Impossible d'utiliser l'IA.")

        logger.info(f"Generating JD for: {request.jobTitle} using Groq")
        
        prompt = f"""You are an expert HR recruiter. Your task is to write a professional and modern Job Description for the following role:
Job Title: {request.jobTitle}
Keywords/Context: {request.keywords if request.keywords else "None provided"}

The description must be in {request.language}.
It should include these sections:
1. 'Description du poste' (A professional introduction)
2. 'Missions & Responsabilités' (Bulleted list)
3. 'Profil recherché & Compétences' (Bulleted list)
4. 'Avantages & Environnement' (Bulleted list)

IMPORTANT: Return ONLY a JSON object with a single key "description" containing the full text formatted as clean HTML.
CRITICAL: Do NOT use Markdown symbols like #, ##, *, or **.
Use <h3> for section titles.
Use <p> for paragraphs.
Use <ul> and <li> for bulleted lists, or <ol> and <li> for numbered lists.
The AI should choose the most appropriate list type for each section.

OUTPUT FORMAT:
{{
    "description": "<h3>Introduction</h3><p>...</p><h3>Missions</h3><ul><li>...</li></ul>"
}}"""
        try:
            response = self.groq_client.chat.completions.create(
                model="llama-3.3-70b-versatile",
                messages=[
                    {"role": "system", "content": "You are a specialized HR assistant that returns ONLY pure HTML within a JSON object. You NEVER use Markdown symbols like #, ##, or *. You only use <h3>, <p>, <ul>, <li> tags."},
                    {"role": "user", "content": prompt}
                ],
                temperature=0.7,
                response_format={"type": "json_object"}
            )
            
            result_json = response.choices[0].message.content.strip()
            data = json.loads(result_json)
            
            return JDGenerationResponse(description=data.get("description", ""))
            
        except Exception as e:
            logger.error(f"Groq generation error: {e}")
            raise Exception("Erreur lors de la génération avec Groq API.")
