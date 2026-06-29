using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    /// <summary>
    /// Représente une évaluation chiffrée et commentée d'une candidature par un recruteur spécifique.
    /// Utilisé pour l'évaluation collaborative interne.
    /// </summary>
    public class ApplicationRating
    {
        /// <summary>
        /// Identifiant unique de l'évaluation de candidature.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifiant unique de la candidature évaluée.
        /// </summary>
        [Required]
        public Guid JobApplicationId { get; set; }

        /// <summary>
        /// Propriété de navigation vers la candidature associée.
        /// </summary>
        public virtual JobApplication? JobApplication { get; set; }

        /// <summary>
        /// Identifiant unique du recruteur ayant réalisé l'évaluation.
        /// </summary>
        [Required]
        public Guid RecruiterId { get; set; }

        /// <summary>
        /// Propriété de navigation vers le recruteur (utilisateur) associé.
        /// </summary>
        public virtual User? Recruiter { get; set; }

        /// <summary>
        /// Note attribuée par le recruteur (comprise entre 1 et 5).
        /// </summary>
        [Range(1, 5)]
        public int Score { get; set; }

        /// <summary>
        /// Commentaire ou justification textuelle associée à la note.
        /// </summary>
        [MaxLength(1000)]
        public string? Comment { get; set; }

        /// <summary>
        /// Date et heure de soumission de l'évaluation.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date et heure de la dernière mise à jour de l'évaluation.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
