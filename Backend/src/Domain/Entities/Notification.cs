using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    /// <summary>
    /// Entité Notification - Représente une notification générée pour alerter les utilisateurs à propos des événements liés à une candidature.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Identifiant unique de la notification.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Identifiant unique de la candidature concernée.
        /// </summary>
        [Required]
        public Guid JobApplicationId { get; set; }

        /// <summary>
        /// Propriété de navigation vers la candidature associée.
        /// </summary>
        public virtual JobApplication? JobApplication { get; set; }

        /// <summary>
        /// Titre ou en-tête de l'alerte/notification.
        /// </summary>
        [Required]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Message descriptif de l'événement.
        /// </summary>
        [Required]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Indique si la notification a été lue par le destinataire.
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Date et heure de création de la notification.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Métadonnées optionnelles décrivant le type d'alerte (ex: "interview_scheduled", "new_application").
        /// </summary>
        public string? Type { get; set; }
    }
}
