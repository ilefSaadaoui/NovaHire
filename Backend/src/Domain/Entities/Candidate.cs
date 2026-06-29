using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    /// <summary>
    /// Entité Candidat - Représente un profil candidat enregistré dans le système.
    /// Contient les informations personnelles, liens professionnels, et la collection de ses candidatures.
    /// </summary>
    public class Candidate
    {
        /// <summary>
        /// Identifiant unique du candidat.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Prénom du candidat.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; }

        /// <summary>
        /// Nom de famille du candidat.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; }

        /// <summary>
        /// Adresse e-mail de contact du candidat.
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public required string Email { get; set; }

        /// <summary>
        /// Numéro de téléphone du candidat.
        /// </summary>
        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// URL vers le profil LinkedIn professionnel du candidat.
        /// </summary>
        public string? LinkedInUrl { get; set; }

        /// <summary>
        /// URL vers le portfolio en ligne du candidat.
        /// </summary>
        public string? PortfolioUrl { get; set; }

        /// <summary>
        /// URL pointant vers le dernier curriculum vitae (CV) principal téléversé.
        /// </summary>
        public string? MainCVUrl { get; set; }

        /// <summary>
        /// Identifiant unique de l'entreprise (tenant) auquel le profil candidat est rattaché pour l'isolation multi-tenant.
        /// </summary>
        [Required]
        public Guid CompanyId { get; set; }

        /// <summary>
        /// Propriété de navigation vers l'entreprise associée.
        /// </summary>
        public virtual Company? Company { get; set; }

        /// <summary>
        /// Liste de l'ensemble des candidatures soumises par ce candidat.
        /// </summary>
        public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();

        /// <summary>
        /// Date et heure de création de la fiche candidat.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date et heure de la dernière mise à jour de la fiche candidat.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
