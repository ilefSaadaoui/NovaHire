using System;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Entité ContactMessage - Représente un message soumis via le formulaire de contact public par un visiteur ou client potentiel.
    /// </summary>
    public class ContactMessage
    {
        /// <summary>
        /// Identifiant unique du message de contact.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Nom complet de l'émetteur du message.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public required string FullName { get; set; }

        /// <summary>
        /// Adresse e-mail de l'émetteur pour la réponse.
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public required string Email { get; set; }

        /// <summary>
        /// Nom de l'entreprise associée à l'émetteur (facultatif).
        /// </summary>
        [MaxLength(200)]
        public string? Company { get; set; }

        /// <summary>
        /// Numéro de téléphone de l'émetteur (facultatif).
        /// </summary>
        [Phone]
        [MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// Contenu texte du message de contact.
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public required string Message { get; set; }

        /// <summary>
        /// Statut actuel de traitement du message de contact (New, Read, Responded).
        /// </summary>
        public ContactMessageStatus Status { get; set; } = ContactMessageStatus.New;

        /// <summary>
        /// Date et heure de réception du message.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date et heure à laquelle une réponse a été apportée au message.
        /// </summary>
        public DateTime? RespondedAt { get; set; }
    }
}
