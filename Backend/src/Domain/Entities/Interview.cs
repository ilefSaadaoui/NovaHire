using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    /// <summary>
    /// Entité Interview - Représente un entretien de recrutement planifié entre un candidat et un recruteur.
    /// Gère les informations de planification, le type d'entretien, le statut et les coordonnées (lien/lieu).
    /// </summary>
    public class Interview
    {
        /// <summary>
        /// Identifiant unique de l'entretien.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifiant unique de la candidature associée.
        /// </summary>
        [Required]
        public Guid JobApplicationId { get; set; }

        /// <summary>
        /// Propriété de navigation vers la candidature associée.
        /// </summary>
        public virtual JobApplication? JobApplication { get; set; }

        /// <summary>
        /// Identifiant unique du recruteur en charge de l'entretien.
        /// </summary>
        [Required]
        public Guid RecruiterId { get; set; }

        /// <summary>
        /// Propriété de navigation vers l'utilisateur (recruteur) associé.
        /// </summary>
        public virtual User? Recruiter { get; set; }

        /// <summary>
        /// Date et heure de tenue de l'entretien.
        /// </summary>
        [Required]
        public DateTime ScheduledAt { get; set; }

        /// <summary>
        /// Type d'entretien : "visio", "phone", "onsite", etc.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public required string Type { get; set; }

        /// <summary>
        /// Lieu physique ou lien de réunion virtuelle pour l'entretien.
        /// </summary>
        [MaxLength(500)]
        public string? LocationOrLink { get; set; }

        /// <summary>
        /// Message ou consignes additionnelles destinées au candidat.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Date et heure de création de la fiche d'entretien.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date et heure de la dernière mise à jour de la fiche d'entretien.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Statut actuel de l'entretien (Planned, Completed, Rescheduled).
        /// </summary>
        public InterviewStatus Status { get; set; } = InterviewStatus.Planned;
    }

    /// <summary>
    /// Statuts possibles pour un entretien de recrutement.
    /// </summary>
    public enum InterviewStatus
    {
        /// <summary>
        /// L'entretien est planifié.
        /// </summary>
        Planned = 0,

        /// <summary>
        /// L'entretien a été réalisé.
        /// </summary>
        Completed = 1,

        /// <summary>
        /// L'entretien a été reporté à une autre date.
        /// </summary>
        Rescheduled = 2,

        /// <summary>
        /// L'entretien a été annulé.
        /// </summary>
        Cancelled = 3
    }
}
