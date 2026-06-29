using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Entité Quiz - Représente un questionnaire technique ou d'évaluation comportementale associé à une offre d'emploi.
    /// Contient des paramètres de limite de temps, de statut d'activité et la collection des questions associées.
    /// </summary>
    public class Quiz
    {
        /// <summary>
        /// Identifiant unique du quiz.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Identifiant unique de l'offre d'emploi associée.
        /// </summary>
        public Guid JobOfferId { get; set; }

        /// <summary>
        /// Propriété de navigation vers l'offre d'emploi associée.
        /// </summary>
        public virtual JobOffer JobOffer { get; set; }

        /// <summary>
        /// Titre général du quiz (ex: "Test Technique C# Junior").
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description succincte des objectifs du quiz et des règles.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Temps limite alloué au candidat pour terminer le test (exprimé en minutes).
        /// </summary>
        public int TimeLimitMinutes { get; set; } = 15;

        /// <summary>
        /// Date et heure de génération du quiz.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indique si le quiz est actif et peut être envoyé aux candidats.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Liste des questions associées à ce quiz.
        /// </summary>
        public virtual ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    }
}
