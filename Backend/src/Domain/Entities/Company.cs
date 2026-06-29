using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Entité Company - Représente une société (tenant) dans le système multi-sociétés (Multi-Tenant).
    /// Gère les informations administratives, de contact, de marque (branding) et d'activation.
    /// </summary>
    public class Company
    {
        /// <summary>
        /// Identifiant unique de l'entreprise.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Nom légal ou commercial de l'entreprise.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        /// <summary>
        /// Secteur d'activité ou industrie de l'entreprise.
        /// </summary>
        [MaxLength(100)]
        public string? Industry { get; set; }

        /// <summary>
        /// Adresse URL du site Web institutionnel.
        /// </summary>
        [MaxLength(255)]
        public string? Website { get; set; }

        /// <summary>
        /// Taille ou effectif de l'entreprise (ex: "50-200").
        /// </summary>
        [MaxLength(50)]
        public string? Size { get; set; }

        /// <summary>
        /// Description textuelle de la société et de ses activités.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Adresse e-mail générale de contact de l'entreprise.
        /// </summary>
        [EmailAddress]
        [MaxLength(255)]
        public string? ContactEmail { get; set; }

        /// <summary>
        /// Numéro de téléphone de contact principal de l'entreprise.
        /// </summary>
        [Phone]
        public string? ContactPhone { get; set; }

        /// <summary>
        /// Adresse postale physique du siège ou bureau principal.
        /// </summary>
        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// Ville d'implantation.
        /// </summary>
        [MaxLength(100)]
        public string? City { get; set; }

        /// <summary>
        /// Code postal associé à l'adresse.
        /// </summary>
        [MaxLength(20)]
        public string? PostalCode { get; set; }

        /// <summary>
        /// Pays d'implantation.
        /// </summary>
        [MaxLength(100)]
        public string? Country { get; set; }

        /// <summary>
        /// URL pointant vers le fichier logo officiel de la société.
        /// </summary>
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Code hexadécimal de la couleur primaire de l'entreprise pour la personnalisation graphique (branding).
        /// </summary>
        [MaxLength(7)]
        public string PrimaryColor { get; set; } = "#FFD700"; // Jaune par défaut

        /// <summary>
        /// Code hexadécimal de la couleur secondaire de l'entreprise pour la personnalisation graphique (branding).
        /// </summary>
        [MaxLength(7)]
        public string SecondaryColor { get; set; } = "#000000"; // Noir par défaut

        /// <summary>
        /// Statut actuel de validation/approbation de l'entreprise (Approved, Pending, etc.).
        /// </summary>
        public CompanyStatus Status { get; set; } = CompanyStatus.Approved;

        /// <summary>
        /// Indique si l'entreprise est active au sein de la plateforme.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date et heure d'enregistrement de l'entreprise.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date et heure de la dernière mise à jour des informations de l'entreprise.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Collection d'utilisateurs rattachés à cette entreprise.
        /// </summary>
        public virtual ICollection<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// Collection de départements créés dans l'entreprise.
        /// </summary>
        public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

        /// <summary>
        /// Collection d'offres d'emploi publiées par l'entreprise.
        /// </summary>
        public virtual ICollection<JobOffer> JobOffers { get; set; } = new List<JobOffer>();
    }
}