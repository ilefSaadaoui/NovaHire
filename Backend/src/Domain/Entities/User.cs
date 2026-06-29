using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Entité Utilisateur - Représente un compte utilisateur de la plateforme (administrateur, recruteur, etc.).
    /// Contient les informations d'authentification, les informations de profil et l'isolation multi-tenant.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Identifiant unique de l'utilisateur.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Prénom de l'utilisateur.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; }

        /// <summary>
        /// Nom de famille de l'utilisateur.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; }

        /// <summary>
        /// Adresse e-mail de l'utilisateur, servant également d'identifiant de connexion.
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public required string Email { get; set; }

        /// <summary>
        /// Hash sécurisé du mot de passe de l'utilisateur.
        /// </summary>
        [Required]
        public required string PasswordHash { get; set; }

        /// <summary>
        /// Numéro de téléphone de l'utilisateur (facultatif).
        /// </summary>
        [Phone]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Identifiant unique du département auquel l'utilisateur est rattaché (facultatif).
        /// </summary>
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// Propriété de navigation vers le département associé.
        /// </summary>
        public virtual Department? Department { get; set; }

        /// <summary>
        /// Identifiant unique de l'entreprise (tenant) à laquelle l'utilisateur appartient (facultatif pour les SuperAdmins).
        /// </summary>
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Propriété de navigation vers l'entreprise (tenant) associée.
        /// </summary>
        public virtual Company? Company { get; set; }

        /// <summary>
        /// Rôle de l'utilisateur au sein de la plateforme (SuperAdmin, CompanyAdmin, Recruteur, etc.).
        /// </summary>
        [Required]
        public UserRole Role { get; set; }

        /// <summary>
        /// Indique si le compte de l'utilisateur est actif.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Indique si l'adresse e-mail a été confirmée.
        /// </summary>
        public bool EmailConfirmed { get; set; } = false;

        /// <summary>
        /// Jeton de sécurité pour la confirmation de l'e-mail.
        /// </summary>
        public string? EmailConfirmationToken { get; set; }

        /// <summary>
        /// Jeton de sécurité pour la réinitialisation du mot de passe.
        /// </summary>
        public string? PasswordResetToken { get; set; }

        /// <summary>
        /// Date et heure d'expiration du jeton de réinitialisation de mot de passe.
        /// </summary>
        public DateTime? PasswordResetTokenExpiry { get; set; }

        /// <summary>
        /// Indique si l'utilisateur doit obligatoirement changer de mot de passe lors de sa prochaine connexion.
        /// </summary>
        public bool MustChangePassword { get; set; } = false;

        /// <summary>
        /// Jeton de rafraîchissement (Refresh Token) JWT pour le maintien de session.
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Date et heure d'expiration du Refresh Token.
        /// </summary>
        public DateTime? RefreshTokenExpiry { get; set; }

        /// <summary>
        /// Date et heure de création de l'enregistrement de l'utilisateur.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date et heure de la dernière mise à jour des informations de l'utilisateur.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Date et heure de la dernière connexion de l'utilisateur.
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Intitulé du poste de travail ou fonction exacte de l'utilisateur.
        /// </summary>
        [MaxLength(100)]
        public string? JobTitle { get; set; }

        /// <summary>
        /// URL de la photo de profil/avatar de l'utilisateur.
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Liste des offres d'emploi créées par cet utilisateur.
        /// </summary>
        public virtual ICollection<JobOffer> CreatedJobOffers { get; set; } = new List<JobOffer>();

        /// <summary>
        /// Liste des candidatures examinées ou évaluées par cet utilisateur.
        /// </summary>
        public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();

        /// <summary>
        /// Obtient le nom complet de l'utilisateur (Prénom suivi du Nom).
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Vérifie si le jeton de réinitialisation de mot de passe actuel est encore valide.
        /// </summary>
        /// <returns>True si le jeton existe et n'a pas expiré, sinon False.</returns>
        public bool IsPasswordResetTokenValid()
        {
            return !string.IsNullOrEmpty(PasswordResetToken)
                && PasswordResetTokenExpiry.HasValue
                && PasswordResetTokenExpiry.Value > DateTime.UtcNow;
        }

        /// <summary>
        /// Vérifie si le jeton de rafraîchissement actuel est encore valide.
        /// </summary>
        /// <returns>True si le jeton existe et n'a pas expiré, sinon False.</returns>
        public bool IsRefreshTokenValid()
        {
            return !string.IsNullOrEmpty(RefreshToken)
                && RefreshTokenExpiry.HasValue
                && RefreshTokenExpiry.Value > DateTime.UtcNow;
        }
    }
}