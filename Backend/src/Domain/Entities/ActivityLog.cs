using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    /// <summary>
    /// Entité ActivityLog - Enregistre les actions d'audit et les activités des utilisateurs sur la plateforme.
    /// Utilisé pour la sécurité et la traçabilité des opérations sensibles.
    /// </summary>
    public class ActivityLog
    {
        /// <summary>
        /// Identifiant unique du log d'activité.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Description textuelle succincte de l'action effectuée (ex: "Connexion", "Création offre").
        /// </summary>
        [Required]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Détails additionnels ou payload JSON de l'opération réalisée.
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Identifiant de l'utilisateur à l'origine de l'action (optionnel si anonyme).
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Adresse e-mail de l'utilisateur à l'origine de l'action.
        /// </summary>
        public string? UserEmail { get; set; }

        /// <summary>
        /// Identifiant de l'entreprise (tenant) associée à l'activité.
        /// </summary>
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Type d'entité sur laquelle porte l'action (ex: "JobOffer", "JobApplication").
        /// </summary>
        public string? EntityType { get; set; }

        /// <summary>
        /// Identifiant de l'entité concernée par l'action.
        /// </summary>
        public string? EntityId { get; set; }

        /// <summary>
        /// Date et heure précises (UTC) de réalisation de l'action.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Adresse IP depuis laquelle la requête a été émise.
        /// </summary>
        public string? IPAddress { get; set; }
    }
}
