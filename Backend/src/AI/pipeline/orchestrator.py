import time
import json
import logging
import ollama
from typing import Dict, List

from models import AnalysisMode, AnalysisResponse
from pipeline.extractor import extract_text
from pipeline.parser import EntityParser
from pipeline.scorer import SemanticScorer
from pipeline.analyzers import calculate_confidence
from groq import Groq
from config import settings

logger = logging.getLogger("NovaHire-AI.orchestrator")

class PipelineOrchestrator:
    """
    Unified Orchestrator using Groq Cloud API for ultra-fast Llama-3 inference.
    """

    def __init__(self, parser: EntityParser, scorer: SemanticScorer):
        self.parser = parser
        self.scorer = scorer
        # Initialize Groq client only if key is available
        self.groq_client = Groq(api_key=settings.groq_api_key) if settings.groq_api_key else None
        # Configure Ollama host
        if settings.ollama_url:
            import os
            os.environ["OLLAMA_HOST"] = settings.ollama_url

    def run(
        self,
        file_content: bytes,
        job_title: str,
        job_description: str,
        weight_exp: int = 33,
        weight_edu: int = 33,
        weight_skills: int = 34,
        mode: AnalysisMode = AnalysisMode.FULL,
        filename: str = "resume.pdf",
        language: str = "fr",
        cv_text_override: str = None
    ) -> tuple[AnalysisResponse, Dict[str, float]]:
        
        if not self.groq_client:
            raise ValueError("GROQ_API_KEY non configurée. Impossible d'utiliser l'IA.")

        timings: Dict[str, float] = {}

        # 1. Extraction (PDF -> Text)
        if cv_text_override:
            cv_text = cv_text_override
            timings["extraction_ms"] = 0
        else:
            t0 = time.perf_counter()
            extraction = extract_text(file_content, filename)
            timings["extraction_ms"] = (time.perf_counter() - t0) * 1000
            cv_text = extraction.raw_text

        if not cv_text or not cv_text.strip():
            raise ValueError("Impossible d'extraire le texte du CV.")

        # 2. Grand Unified Analysis via Groq (Llama-3)
        t0 = time.perf_counter()
        logger.info("Starting Full Unified Analysis using Groq (llama3-8b-8192)")
        
        # ── Build optimized prompt ──
        lang_instruction = "Respond in French." if language == "fr" else "Respond in English."
        
        prompt = f"""You are a seasoned Human Resources AI specialist with extensive expertise in parsing and analyzing resumes against job descriptions.

TASK: Analyze the CV below against the JOB DESCRIPTION. Follow these steps precisely:

1. EXTRACT all work experiences (job title, company, startDate, endDate, description). Do NOT omit or summarize any entry.
2. EXTRACT all educational qualifications (degree, institution, endDate, honors if any).
3. EXTRACT key entities: email, phone, skills, total years of experience, languages, and a professional summary.
4. SCORE the candidate's fit against the job on 3 axes (0-100 each):
   - experience_score: relevance and depth of work experience
   - education_score: relevance and level of education
   - skills_score: match between candidate skills and job requirements
5. Generate 3-5 interview questions tailored to this candidate's profile gaps.

SCORING WEIGHTS (set by recruiter): Experience={weight_exp}%, Education={weight_edu}%, Skills={weight_skills}%.
{lang_instruction}

CV:
{cv_text[:3000]}

JOB DESCRIPTION:
{job_description[:1000]}

OUTPUT FORMAT — JSON ONLY, no extra text:
{{
    "entities": {{
        "email": "...", "phone": "...", "skills": ["..."],
        "total_experience_years": 0.0, "education_score": 50,
        "languages": ["..."], "summary": "2-3 sentence professional summary",
        "work_experiences": [{{"jobTitle": "...", "company": "...", "startDate": "YYYY", "endDate": "YYYY", "description": "..."}}],
        "educations": [{{"degree": "...", "institution": "...", "endDate": "YYYY"}}]
    }},
    "matching": {{
        "overall_score": 0, "experience_score": 0, "education_score": 0, "skills_score": 0,
        "strengths": ["..."], "weaknesses": ["..."], "recommendation": "hire/consider/reject with brief justification"
    }},
    "interview_guide": [{{"question": "...", "expected_answer": "...", "intent": "what this question evaluates"}}]
}}"""
        try:
            try:
                response = self.groq_client.chat.completions.create(
                    model="llama-3.1-8b-instant",
                    messages=[{"role": "user", "content": prompt}],
                    temperature=0.1,
                    response_format={"type": "json_object"}
                )
                raw_content = response.choices[0].message.content.strip()
            except Exception as groq_err:
                # Détection du Rate Limit (429) ou autre erreur Groq
                is_rate_limit = "429" in str(groq_err) or "rate_limit" in str(groq_err).lower()
                
                if is_rate_limit:
                    logger.warning(f"Groq Rate Limit reached (429). Falling back to local Ollama. Error: {groq_err}")
                else:
                    logger.error(f"Groq API error: {groq_err}. Trying fallback to Ollama.")
                
                # Fallback vers Ollama (Llama3 local)
                t_fallback = time.perf_counter()
                ollama_response = ollama.chat(
                    model='llama3',
                    messages=[{'role': 'user', 'content': prompt}],
                    format='json'
                )
                raw_content = ollama_response['message']['content'].strip()
                logger.info(f"Ollama fallback successful in {(time.perf_counter() - t_fallback):.2f}s")

            try:
                data = json.loads(raw_content)
            except json.JSONDecodeError:
                # Fallback repair just in case
                start = raw_content.find('{')
                end = raw_content.rfind('}')
                if start != -1 and end != -1:
                    data = json.loads(raw_content[start:end+1])
                else:
                    raise ValueError("L'IA n'a pas renvoyé de JSON valide.")

            timings["llm_total_ms"] = (time.perf_counter() - t0) * 1000

            entities = data.get("entities", {})
            match = data.get("matching", {})

            
            # Helper to safely convert to int, handling dicts and strings
            def safe_int(val, default=0):
                try:
                    if val is None: return default
                    if isinstance(val, dict):
                        return safe_int(val.get("value") or val.get("score") or next(iter(val.values())), default)
                    return int(float(str(val)))
                except:
                    return default

            exp_score = safe_int(match.get("experience_score"))
            edu_score = safe_int(match.get("education_score"))
            skill_score = safe_int(match.get("skills_score"))
            
            # Calcul mathématique strict avec les poids du recruteur
            w_exp = weight_exp / 100.0
            w_edu = weight_edu / 100.0
            w_skills = weight_skills / 100.0
            calculated_overall = int(round((exp_score * w_exp) + (edu_score * w_edu) + (skill_score * w_skills)))

            # Mapping adjustments for NovaHire .NET/Vue naming conventions
            result = AnalysisResponse(
                overallScore=calculated_overall,
                experienceScore=exp_score,
                educationScore=edu_score,
                skillsScore=skill_score,
                totalYearsExperience=entities.get("total_experience_years", 0),
                autoGeneratedSummary=entities.get("summary", ""),
                identifiedSkills=entities.get("skills", []),
                strengths=match.get("strengths", []),
                weaknesses=match.get("weaknesses", []),
                aiRecommendation=match.get("recommendation", ""),
                confidenceScore=calculate_confidence(cv_text, entities),
                extractedData={
                    "workExperiences": entities.get("work_experiences", []),
                    "educations": entities.get("educations", []),
                    "skills": entities.get("skills", []),
                    "languages": entities.get("languages", []),
                    "email": entities.get("email"),
                    "phone": entities.get("phone"),
                    "totalYearsExperience": entities.get("total_experience_years", 0)
                },
                rawCvText=cv_text,
                interviewQuestions=data.get("interview_guide", [])
            )
            return result, timings

        except Exception as e:
            logger.error(f"Unified analysis error: {e}")
            raise Exception(f"Erreur d'analyse IA locale : {e}")

    def run_from_text(
        self,
        cv_text: str,
        job_description: str,
        weight_exp: int = 33,
        weight_edu: int = 33,
        weight_skills: int = 34,
        language: str = "fr"
    ) -> tuple[AnalysisResponse, Dict[str, float]]:
        """Analyze already extracted text."""
        # On simule un file_content vide car on a déjà le texte
        return self.run(b"", "", job_description, weight_exp, weight_edu, weight_skills, AnalysisMode.FULL, "resume.txt", language, cv_text_override=cv_text)

    def generate_feedback(self, candidate_name: str, job_title: str, recruiter_notes: str, language: str = "fr") -> str:
        """Generate a benevolent and constructive rejection email content."""
        if not self.groq_client:
            raise ValueError("GROQ_API_KEY non configurée. Impossible d'utiliser l'IA.")

        lang_instruction = "Respond in French." if language == "fr" else "Respond in English."
        
        prompt = f"""You are a professional Recruitment Coach. Write a benevolent, constructive, and personalized rejection email for a candidate.
        
        CONTEXT:
        - Candidate: {candidate_name}
        - Job: {job_title}
        - Recruiter Notes (Constructive Feedback): {recruiter_notes}
        
        REQUIREMENTS:
        - Professional yet warm tone.
        - Include the specific constructive feedback from the recruiter notes to help the candidate improve.
        - Encourage the candidate for their future career.
        - {lang_instruction}
        
        OUTPUT: Only the content of the email, starting with 'Bonjour {candidate_name},' or equivalent. NO intro or outro text outside the email.
        """
        
        try:
            response = self.groq_client.chat.completions.create(
                model="llama-3.1-8b-instant",
                messages=[{"role": "user", "content": prompt}],
                temperature=0.7
            )
            return response.choices[0].message.content.strip()
        except Exception as e:
            logger.error(f"Feedback generation error: {e}")
            return f"Bonjour {candidate_name}, merci pour votre intérêt. Malheureusement, nous n'avons pas pu retenir votre candidature pour le poste de {job_title}. Bonne chance pour la suite."

    def generate_offer_letter(self, candidate_name: str, job_title: str, salary: float, currency: str, start_date: str, benefits: str, recruiter_name: str, company_name: str, language: str = "fr") -> str:
        """
        Generate a professional offer letter using LLM.
        """
        prompt = f"""
        Rédige une lettre d'offre d'emploi officielle et enthousiaste.
        
        CANDIDAT: {candidate_name}
        POSTE: {job_title}
        SALAIRE: {salary} {currency}
        DATE DE DÉBUT: {start_date}
        AVANTAGES: {benefits}
        RECRUTEUR: {recruiter_name}
        ENTREPRISE: {company_name}
        
        RÈGLES CRITIQUES DE FORMATAGE :
        1. Utilise UNIQUEMENT des balises HTML standards (<b>, <br>, <ul>, <li>, <p>) pour structurer la lettre.
        2. N'utilise JAMAIS de Markdown (interdiction stricte d'utiliser ** ou * ou #).
        3. Ne mets pas de variables à remplir comme [Votre nom], utilise directement les informations fournies.
        4. Évite les doubles sauts de ligne inutiles entre les paragraphes pour que l'email reste compact et professionnel.
        
        La lettre doit être structurée, professionnelle et chaleureuse. 
        Inclus des sections sur le poste, la rémunération, les avantages et les prochaines étapes, en signant avec le nom du recruteur et de l'entreprise.
        Langue: {language}
        """
        
        try:
            response = self.groq_client.chat.completions.create(
                messages=[
                    {"role": "system", "content": "Tu es un DRH expert spécialisé dans la rédaction de propositions d'embauche premium."},
                    {"role": "user", "content": prompt}
                ],
                model="llama-3.3-70b-versatile",
                temperature=0.7
            )
            return response.choices[0].message.content
        except Exception as e:
            logger.error(f"Groq offer letter error: {e}")
            return f"Offre officielle pour {candidate_name} au poste de {job_title}. Salaire: {salary} {currency}. Date de début: {start_date}."

    def _build_extract_only_response(self, cv_text, entities):
        # ...
        # ... keep as is if needed, but run() is primary
        return AnalysisResponse(extractedData={}) # Simplifié pour test

