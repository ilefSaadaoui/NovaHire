using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    /// <summary>
    /// Entité JobOffer - Représente une offre d'emploi ou de stage
    /// </summary>
    public class JobOffer
    {
        [Key]
        public Guid Id { get; set; }

        // Informations de base
        [Required]
        [MaxLength(200)]
        public required string Title { get; set; } // Use required

        [Required]
        public required string Description { get; set; } // Use required

        public JobType Type { get; set; } = JobType.FullTime;

        [MaxLength(100)]
        public string? Location { get; set; } // Made nullable

        public string? Department { get; set; }

        public string? SalaryRange { get; set; } // Made nullable

        public RemotePolicy? RemotePolicy { get; set; }

        public ExperienceLevel? ExperienceLevel { get; set; }

        public List<string>? Skills { get; set; } = new();

        public DateTime? Deadline { get; set; }

        // Tenant isolation
        [Required]
        public Guid CompanyId { get; set; }
        public virtual Company? Company { get; set; } // Made nullable

        // Créateur de l'offre
        public Guid CreatedById { get; set; }
        public virtual User? CreatedBy { get; set; } // Made nullable

        // Configuration du formulaire de candidature
        public ApplicationFormConfig FormConfig { get; set; } = new ApplicationFormConfig(); // Initialize

        // Configuration de l'IA (Pondération)
        public int WeightExperience { get; set; } = 40; // Default 40%
        public int WeightEducation { get; set; } = 30;  // Default 30%
        public int WeightSkills { get; set; } = 30;     // Default 30%

        // Auto-rejection: candidates below this AI score are automatically rejected (0 = disabled)
        public int AutoRejectThreshold { get; set; } = 0;

        // Configuration de l'affichage public
        public PublicDisplayConfig DisplayConfig { get; set; } = new PublicDisplayConfig(); // Initialize

        // Lien public généré
        [MaxLength(500)]
        public string? PublicUrl { get; set; } // Made nullable

        [MaxLength(100)]
        public string? ShareToken { get; set; }

        // Statut de l'offre
        public JobOfferStatus Status { get; set; } = JobOfferStatus.Draft;

        // Dates
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        // Navigation properties
        public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>(); // Initialize

        // Méthodes utilitaires
        public bool IsActive()
        {
            return Status == JobOfferStatus.Published
                && (!ExpiresAt.HasValue || ExpiresAt.Value > DateTime.UtcNow);
        }

        public void Publish()
        {
            Status = JobOfferStatus.Published;
            PublishedAt = DateTime.UtcNow;
        }

        public void Close()
        {
            Status = JobOfferStatus.Closed;
        }
    }

    /// <summary>
    /// Types de postes
    /// </summary>
    public enum JobType
    {
        FullTime = 0,
        PartTime = 1,
        Contract = 2,
        Internship = 3,
        Freelance = 4
    }

    /// <summary>
    /// Statuts d'une offre
    /// </summary>
    public enum JobOfferStatus
    {
        Draft = 0,
        Published = 1,
        Closed = 2,
        Archived = 3
    }

    /// <summary>
    /// Politiques de télétravail
    /// </summary>
    public enum RemotePolicy
    {
        OnSite = 0,
        Remote = 1,
        Hybrid = 2
    }

    /// <summary>
    /// Niveaux d'expérience
    /// </summary>
    public enum ExperienceLevel
    {
        Junior = 0,
        Intermediate = 1,
        Senior = 2,
        Expert = 3,
        Graduate = 4,
        All = 5,
        Beginner = 6
    }

    /// <summary>
    /// Configuration du formulaire de candidature
    /// </summary>
    public class ApplicationFormConfig
    {
        public bool RequireFullName { get; set; } = true;
        public bool RequireEmail { get; set; } = true;
        public bool RequirePhone { get; set; } = false;
        public bool RequireCV { get; set; } = true;
        public bool RequireCoverLetter { get; set; } = false;
        public bool RequirePortfolio { get; set; } = false;
        public bool RequireLinkedIn { get; set; } = false;

        public List<string> CustomFields { get; set; } = new List<string>();
        public List<string> RequiredDocuments { get; set; } = new List<string>();
    }

    /// <summary>
    /// Configuration de l'affichage public
    /// </summary>
    public class PublicDisplayConfig
    {
        public bool ShowCompanyName { get; set; } = true;
        public bool ShowCompanyLogo { get; set; } = true;
        public bool ShowSalary { get; set; } = false;
        public bool ShowLocation { get; set; } = true;

        public string? CustomCSS { get; set; } // Made nullable
        public string? HeaderImageUrl { get; set; } // Made nullable
    }
}