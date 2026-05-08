using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Entité Utilisateur - Représente un utilisateur de la plateforme
    /// </summary>
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; } // Use required

        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; } // Use required

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public required string Email { get; set; } // Use required

        [Required]
        public required string PasswordHash { get; set; } // Use required

        [Phone]
        public string? PhoneNumber { get; set; } // Made nullable

        public Guid? DepartmentId { get; set; }
        public virtual Department? Department { get; set; }

        // Multi-tenant: Lien avec la société
        public Guid? CompanyId { get; set; }
        public virtual Company? Company { get; set; } // Made nullable

        // Rôle de l'utilisateur
        [Required]
        public UserRole Role { get; set; }

        // État du compte
        public bool IsActive { get; set; } = true;
        public bool EmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; } // Made nullable

        // Réinitialisation de mot de passe
        public string? PasswordResetToken { get; set; } // Made nullable
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public bool MustChangePassword { get; set; } = false;

        // Refresh Token pour JWT
        public string? RefreshToken { get; set; } // Made nullable
        public DateTime? RefreshTokenExpiry { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Moteur Optionnel: Poste exact du membre
        [MaxLength(100)]
        public string? JobTitle { get; set; }

        // Avatar
        public string? AvatarUrl { get; set; }

        // Navigation properties
        public virtual ICollection<JobOffer> CreatedJobOffers { get; set; } = new List<JobOffer>(); // Initialize
        public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>(); // Initialize

        // Méthodes utilitaires
        public string FullName => $"{FirstName} {LastName}";

        public bool IsPasswordResetTokenValid()
        {
            return !string.IsNullOrEmpty(PasswordResetToken)
                && PasswordResetTokenExpiry.HasValue
                && PasswordResetTokenExpiry.Value > DateTime.UtcNow;
        }

        public bool IsRefreshTokenValid()
        {
            return !string.IsNullOrEmpty(RefreshToken)
                && RefreshTokenExpiry.HasValue
                && RefreshTokenExpiry.Value > DateTime.UtcNow;
        }
    }
}