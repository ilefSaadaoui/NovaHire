import json
import logging
import ollama
from typing import List, Optional

from config import settings
from models import ParsedEntities, TextSections

logger = logging.getLogger("NovaHire-AI.parser")

class EntityParser:
    """
    Ollama-backed entity parser. 
    Replaces old spaCy and keyword-based logic for higher accuracy and zero maintenance.
    """

    def __init__(self):
        logger.info("EntityParser initialized (Ollama Mode)")

    def parse_entities(self, text: str) -> ParsedEntities:
        """Extract core contact info and skills using LLM reasoning."""
        logger.info("Parsing CV entities using Ollama (Llama 3)")
        
        prompt = f"""
Analysez le texte de CV suivant et extrayez toutes les informations pertinentes de manière structurée.
TEXTE DU CV :
---
{text[:6000]} 
---

Vous DEVEZ répondre UNIQUEMENT avec un objet JSON valide respectant cette structure :
{{
    "email": "exemple@mail.com",
    "phone": "0600000000",
    "skills": ["Python", "React", ...],
    "total_experience_years": 5.5,
    "education_score": 90, 
    "languages": ["Français (Maternel)", "Anglais (Courant)"],
    "summary": "Bref résumé professionnel",
    "work_experiences": [
        {{"title": "Intitulé du poste", "company": "Nom Entreprise", "duration": "Dates", "description": "Missions"}}
    ],
    "educations": [
        {{"degree": "Nom du diplôme", "school": "École/Université", "year": "Année"}}
    ]
}}

Note pour education_score : 100 (Doctorat), 90 (Master/Ingénieur), 80 (Licence/Bachelor), 70 (Bac+2), 60 (Bac), 50 (Autre).
Si une information est absente, mettez null ou un tableau vide [].
"""
        try:
            response = ollama.chat(
                model='llama3',
                messages=[{'role': 'user', 'content': prompt}],
                format='json'
            )
            
            data = json.loads(response['message']['content'])
            
            return ParsedEntities(
                email=data.get("email"),
                phone=data.get("phone"),
                skills=data.get("skills", []),
                total_experience_years=data.get("total_experience_years", 0.0),
                education_score=data.get("education_score", 50.0),
                languages=data.get("languages", []),
                summary=data.get("summary", ""),
                work_experiences=data.get("work_experiences", []),
                educations=data.get("educations", [])
            )
        except Exception as e:
            logger.error(f"Error parsing entities with Ollama: {e}")
            return ParsedEntities()

    def anonymize(self, text: str) -> str:
        """Anonymize text for unbiased scoring using LLM."""
        prompt = f"Anonymise ce texte de CV en remplaçant les noms propres, adresses exactes et photos par [NOM], [ADRESSE], etc. Garde tout le reste intact :\n{text[:2000]}"
        try:
            response = ollama.chat(model='llama3', messages=[{'role': 'user', 'content': prompt}])
            return response['message']['content']
        except:
            return text

    def segment_text(self, text: str) -> TextSections:
        """Divide CV text into logical semantic sections using LLM."""
        logger.info("Segmenting CV text using Ollama")
        
        prompt = f"""
Répartissez le texte du CV suivant dans les trois catégories : 'experience', 'education', 'skills'.
TEXTE :
{text[:4000]}

Répondez UNIQUEMENT en JSON :
{{
    "experience": "contenu de la section expérience",
    "education": "contenu de la section formation",
    "skills": "contenu de la section compétences"
}}
"""
        try:
            response = ollama.chat(
                model='llama3',
                messages=[{'role': 'user', 'content': prompt}],
                format='json'
            )
            data = json.loads(response['message']['content'])
            
            return TextSections(
                experience=data.get("experience", ""),
                education=data.get("education", ""),
                skills=data.get("skills", "")
            )
        except Exception as e:
            logger.error(f"Error segmenting text with Ollama: {e}")
            return TextSections()

    def segment_jd(self, text: str) -> dict:
        """Segment Job Description into Requirements and Responsibilities using LLM."""
        prompt = f"""
Analysez cette offre d'emploi et séparez les 'exigences' (compétences requises) des 'responsabilités' (missions).
OFFRE :
{text[:4000]}

Répondez UNIQUEMENT en JSON :
{{
    "requirements": "liste des exigences",
    "responsibilities": "liste des missions"
}}
"""
        try:
            response = ollama.chat(
                model='llama3',
                messages=[{'role': 'user', 'content': prompt}],
                format='json'
            )
            return json.loads(response['message']['content'])
        except Exception as e:
            logger.error(f"Error segmenting JD with Ollama: {e}")
            return {"requirements": text, "responsibilities": "", "other": ""}
