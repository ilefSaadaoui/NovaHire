using System;

namespace Domain.Entities
{
    /// <summary>
    /// Entité CandidateQuizResult - Stocke le score et les réponses détaillées d'un candidat après avoir passé un quiz d'évaluation.
    /// </summary>
    public class CandidateQuizResult
    {
        /// <summary>
        /// Identifiant unique du résultat du quiz.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Identifiant unique de la candidature associée.
        /// </summary>
        public Guid JobApplicationId { get; set; }

        /// <summary>
        /// Propriété de navigation vers la candidature associée.
        /// </summary>
        public virtual JobApplication JobApplication { get; set; }

        /// <summary>
        /// Identifiant unique du quiz passé.
        /// </summary>
        public Guid QuizId { get; set; }

        /// <summary>
        /// Propriété de navigation vers le quiz associé.
        /// </summary>
        public virtual Quiz Quiz { get; set; }

        /// <summary>
        /// Score final obtenu (exprimé généralement en pourcentage ou sur un barème).
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Date et heure de complétion du test par le candidat.
        /// </summary>
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Contenu au format JSON stockant le détail des réponses fournies par le candidat pour archivage et affichage.
        /// </summary>
        public string CandidateAnswersJson { get; set; }
    }
}
