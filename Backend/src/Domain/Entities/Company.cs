using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Entité Company - Représente une société (tenant) dans le système multi-sociétés
    /// </summary>
    public class Company
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Name { get; set; } // Use required

        [MaxLength(100)]
        public string? Industry { get; set; }

        [MaxLength(255)]
        public string? Website { get; set; }

        [MaxLength(50)]
        public string? Size { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; } // Made nullable

        // Informations de contact
        [EmailAddress]
        [MaxLength(255)]
        public string? ContactEmail { get; set; } // Made nullable

        [Phone]
        public string? ContactPhone { get; set; } // Made nullable

        [MaxLength(500)]
        public string? Address { get; set; } // Made nullable

        [MaxLength(100)]
        public string? City { get; set; } // Made nullable

        [MaxLength(20)]
        public string? PostalCode { get; set; } // Made nullable

        [MaxLength(100)]
        public string? Country { get; set; } // Made nullable

        // Branding
        public string? LogoUrl { get; set; } // Made nullable

        [MaxLength(7)]
        public string PrimaryColor { get; set; } = "#FFD700"; // Jaune NeoLedge par défaut

        [MaxLength(7)]
        public string SecondaryColor { get; set; } = "#000000"; // Noir par défaut

        // Configuration
        public CompanyStatus Status { get; set; } = CompanyStatus.Approved;
        public bool IsActive { get; set; } = true;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>(); // Initialize
        public virtual ICollection<Department> Departments { get; set; } = new List<Department>(); // Initialize
        public virtual ICollection<JobOffer> JobOffers { get; set; } = new List<JobOffer>(); // Initialize
    }
}