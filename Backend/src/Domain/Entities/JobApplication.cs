using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    /// <summary>
    /// Entité JobApplication - Représente une candidature soumise par un candidat pour une offre d'emploi.
    /// Contient les documents, le statut de progression, les résultats d'analyses IA (scores de matching), les notes de recruteurs et les résultats de quiz.
    /// </summary>
    public class JobApplication
    {
        /// <summary>
        /// Identifiant unique de la candidature.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifiant unique de l'offre d'emploi postulée.
        /// </summary>
        [Required]
        public Guid JobOfferId { get; set; }

        /// <summary>
        /// Propriété de navigation vers l'offre d'emploi associée.
        /// </summary>
        public virtual JobOffer? JobOffer { get; set; }

        /// <summary>
        /// Identifiant unique du candidat associé.
        /// </summary>
        [Required]
        public Guid CandidateId { get; set; }

        /// <summary>
        /// Propriété de navigation vers la fiche candidat.
        /// </summary>
        public virtual Candidate? Candidate { get; set; }

        /// <summary>
        /// URL pointant vers le curriculum vitae (CV) téléversé spécifiquement pour cette candidature.
        /// </summary>
        [Required]
        public required string CVUrl { get; set; }

        /// <summary>
        /// URL pointant vers la lettre de motivation (facultatif).
        /// </summary>
        public string? CoverLetterUrl { get; set; }

        /// <summary>
        /// Dictionnaire contenant les réponses fournies par le candidat aux champs de formulaire personnalisés de l'offre.
        /// </summary>
        public Dictionary<string, string> CustomFieldsData { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Statut actuel de traitement de la candidature (Submitted, UnderReview, Shortlisted, etc.).
        /// </summary>
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;

        /// <summary>
        /// Canal d'origine de la candidature (ex: "LinkedIn", "Direct", "Indeed").
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Détail structuré des analyses qualitatives et quantitatives générées par l'IA.
        /// </summary>
        public AIAnalysisResult? AIAnalysis { get; set; }

        /// <summary>
        /// Cache du score de matching global de l'IA (0-100) pour optimiser les performances de tri.
        /// </summary>
        public int? AiScore { get; set; }

        /// <summary>
        /// Commentaires ou remarques générales du recruteur sur cette candidature.
        /// </summary>
        public string? RecruiterNotes { get; set; }

        /// <summary>
        /// Indique si un quiz d'évaluation de présélection a été envoyé à ce candidat.
        /// </summary>
        public bool QuizSent { get; set; } = false;

        /// <summary>
        /// Note finale obtenue par le candidat au quiz de présélection (exprimée en pourcentage).
        /// </summary>
        public int? QuizScore { get; set; }

        /// <summary>
        /// Date et heure d'expiration au-delà de laquelle le candidat ne peut plus passer le quiz.
        /// </summary>
        public DateTime? QuizExpiresAt { get; set; }

        /// <summary>
        /// Contenu texte ou HTML de la lettre de proposition d'embauche (Offer Letter) envoyée au candidat.
        /// </summary>
        public string? OfferLetterContent { get; set; }

        /// <summary>
        /// Liste des commentaires laissés par les différents membres de l'équipe RH sur cette candidature.
        /// </summary>
        public virtual ICollection<JobApplicationComment> Comments { get; set; } = new List<JobApplicationComment>();

        /// <summary>
        /// Liste de toutes les évaluations (étoiles) données à cette candidature par les recruteurs.
        /// </summary>
        public virtual ICollection<ApplicationRating> Ratings { get; set; } = new List<ApplicationRating>();

        /// <summary>
        /// Collection de notifications générées en rapport avec cette candidature.
        /// </summary>
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        /// <summary>
        /// Date et heure de soumission de la candidature.
        /// </summary>
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date et heure de la dernière mise à jour de la candidature.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Date et heure de la dernière évaluation du statut (revue) de la candidature.
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// Identifiant du recruteur ou administrateur ayant procédé à la dernière évaluation.
        /// </summary>
        public Guid? ReviewedById { get; set; }

        /// <summary>
        /// Propriété de navigation vers l'utilisateur (évaluateur) associé.
        /// </summary>
        public virtual User? ReviewedBy { get; set; }

        /// <summary>
        /// Met à jour le statut de la candidature et enregistre les informations de l'évaluateur.
        /// </summary>
        /// <param name="newStatus">Le nouveau statut à appliquer.</param>
        /// <param name="reviewerId">L'identifiant du recruteur réalisant l'action.</param>
        public void UpdateStatus(ApplicationStatus newStatus, Guid reviewerId)
        {
            Status = newStatus;
            ReviewedById = reviewerId;
            ReviewedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Statuts du cycle de traitement d'une candidature.
    /// </summary>
    public enum ApplicationStatus
    {
        /// <summary>
        /// Candidature soumise, en attente d'examen initial.
        /// </summary>
        Submitted = 0,

        /// <summary>
        /// Candidature en cours d'analyse qualitative ou technique.
        /// </summary>
        UnderReview = 1,

        /// <summary>
        /// Candidature sélectionnée pour figurer dans la shortlist.
        /// </summary>
        Shortlisted = 2,

        /// <summary>
        /// Entretien de recrutement planifié.
        /// </summary>
        Interview = 3,

        /// <summary>
        /// Entretien réalisé avec succès.
        /// </summary>
        Interviewed = 6,

        /// <summary>
        /// Candidature rejetée.
        /// </summary>
        Rejected = 4,

        /// <summary>
        /// Candidature acceptée / Recrutement validé.
        /// </summary>
        Accepted = 5,

        /// <summary>
        /// Proposition d'embauche envoyée au candidat.
        /// </summary>
        OfferSent = 7
    }

    /// <summary>
    /// Modèle représentant les résultats d'analyses qualitatives et quantitatives de l'IA sur le profil d'un candidat.
    /// </summary>
    public class AIAnalysisResult
    {
        /// <summary>
        /// Score global de matching (0-100) calculé par l'IA.
        /// </summary>
        public int OverallScore { get; set; }

        /// <summary>
        /// Score partiel basé sur la pertinence de l'expérience professionnelle (0-100).
        /// </summary>
        public int ExperienceScore { get; set; }

        /// <summary>
        /// Score partiel basé sur le niveau d'études et formations (0-100).
        /// </summary>
        public int EducationScore { get; set; }

        /// <summary>
        /// Score partiel basé sur les compétences mentionnées dans le CV (0-100).
        /// </summary>
        public int SkillsScore { get; set; }

        /// <summary>
        /// Total d'années d'expérience cumulées extraites du CV par l'IA.
        /// </summary>
        public double TotalYearsExperience { get; set; }

        /// <summary>
        /// Données brutes de parcours professionnel et académique extraites du CV.
        /// </summary>
        public ExtractedCVData? ExtractedData { get; set; }

        /// <summary>
        /// Résumé exécutif du profil rédigé automatiquement par le LLM.
        /// </summary>
        public string? AutoGeneratedSummary { get; set; }

        /// <summary>
        /// Compétences techniques et humaines identifiées dans le CV.
        /// </summary>
        public List<string> IdentifiedSkills { get; set; } = new List<string>();

        /// <summary>
        /// Points forts identifiés par l'IA dans le profil du candidat.
        /// </summary>
        public List<string> Strengths { get; set; } = new List<string>();

        /// <summary>
        /// Points de vigilance ou lacunes potentielles relevées par l'IA.
        /// </summary>
        public List<string> Weaknesses { get; set; } = new List<string>();

        /// <summary>
        /// Questions d'entretien suggérées par l'IA pour valider les doutes ou approfondir l'évaluation.
        /// </summary>
        public List<InterviewQuestionRecord> InterviewQuestions { get; set; } = new List<InterviewQuestionRecord>();

        /// <summary>
        /// Recommandation globale formulée par l'IA (ex: "Fortement Recommandé", "A rejeter", etc.).
        /// </summary>
        public string? AIRecommendation { get; set; }

        /// <summary>
        /// Date et heure auxquelles l'analyse IA a été complétée.
        /// </summary>
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Regroupe l'ensemble des données extraites du CV lors de l'analyse OCR et NLP.
    /// </summary>
    public class ExtractedCVData
    {
        /// <summary>
        /// Liste des expériences de travail détectées.
        /// </summary>
        public List<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();

        /// <summary>
        /// Liste des diplômes et cursus de formation.
        /// </summary>
        public List<Education> Educations { get; set; } = new List<Education>();

        /// <summary>
        /// Liste globale des compétences extraites du CV.
        /// </summary>
        public List<string> Skills { get; set; } = new List<string>();

        /// <summary>
        /// Liste des langues maîtrisées par le candidat.
        /// </summary>
        public List<string> Languages { get; set; } = new List<string>();

        /// <summary>
        /// Certifications professionnelles et diplômes d'académies tierces.
        /// </summary>
        public List<string> Certifications { get; set; } = new List<string>();
    }

    /// <summary>
    /// Représente une expérience professionnelle extraite du CV.
    /// </summary>
    public class WorkExperience
    {
        /// <summary>
        /// Titre du poste occupé (ex: "Lead Developer").
        /// </summary>
        public required string JobTitle { get; set; }

        /// <summary>
        /// Nom de l'entreprise d'accueil.
        /// </summary>
        public required string Company { get; set; }

        /// <summary>
        /// Localisation géographique (ville, pays).
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Date de début de l'expérience.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Date de fin de l'expérience (nulle si poste actuel).
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Indique s'il s'agit du poste actuel du candidat.
        /// </summary>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// Description des missions accomplies et des technologies utilisées.
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// Représente une formation ou un diplôme extrait du CV.
    /// </summary>
    public class Education
    {
        /// <summary>
        /// Diplôme obtenu ou visé (ex: "Master en Ingénierie").
        /// </summary>
        public required string Degree { get; set; }

        /// <summary>
        /// Nom de l'école ou de l'université.
        /// </summary>
        public required string Institution { get; set; }

        /// <summary>
        /// Domaine d'études spécifique (ex: "Informatique").
        /// </summary>
        public string? FieldOfStudy { get; set; }

        /// <summary>
        /// Date de début du cursus.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Date d'obtention ou de fin du cursus.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Moyenne académique / Mention obtenue (GPA) si disponible.
        /// </summary>
        public string? GPA { get; set; }
    }

    /// <summary>
    /// Représente une question d'entretien suggérée ou notée dans le cadre de l'évaluation du candidat.
    /// </summary>
    public class InterviewQuestionRecord
    {
        /// <summary>
        /// Catégorie de la question (ex: "Technique", "Comportemental", "Culturel").
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Contenu de la question recommandée.
        /// </summary>
        public string Question { get; set; } = string.Empty;

        /// <summary>
        /// Objectif visé par cette question ou élément à valider.
        /// </summary>
        public string Purpose { get; set; } = string.Empty;

        /// <summary>
        /// Score optionnel attribué à la réponse du candidat en cours d'entretien (de 1 à 5).
        /// </summary>
        public int? Score { get; set; }

        /// <summary>
        /// Notes prises en direct par le recruteur lors de l'entretien.
        /// </summary>
        public string? Notes { get; set; }
    }
}