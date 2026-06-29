using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    /// <summary>
    /// DTO pour l'inscription
    /// </summary>
    public class RegisterDto
    {
        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères, une majuscule et un chiffre.")]
        public required string Password { get; set; }

        [Required]
        [Compare("Password")]
        public required string ConfirmPassword { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Role { get; set; }

        public Guid? CompanyId { get; set; }

        public string? CompanyName { get; set; }
    }

    /// <summary>
    /// DTO pour la connexion
    /// </summary>
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// DTO pour la réponse d'authentification
    /// </summary>
    public class AuthResponseDto
    {
        public Guid UserId { get; set; }

        [EmailAddress]
        public required string Email { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Role { get; set; }

        public Guid? CompanyId { get; set; }

        public string? CompanyName { get; set; }

        public required string AccessToken { get; set; }

        public required string RefreshToken { get; set; }

        public DateTime TokenExpiration { get; set; }

        public bool EmailConfirmed { get; set; }
        public bool MustChangePassword { get; set; }
        public bool RequiresApproval { get; set; }
    }

    /// <summary>
    /// DTO pour le rafraîchissement du token
    /// </summary>
    public class RefreshTokenDto
    {
        [Required]
        public required string RefreshToken { get; set; }

        // This property was causing the error - either remove it or keep it optional
        public string? AccessToken { get; set; } // Made optional with ?
    }

    /// <summary>
    /// DTO pour la demande de réinitialisation de mot de passe
    /// </summary>
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }

    /// <summary>
    /// DTO pour la réinitialisation de mot de passe
    /// </summary>
    public class ResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Token { get; set; }

        [Required]
        [MinLength(6)]
        public required string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public required string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// DTO pour le changement de mot de passe
    /// </summary>
    public class ChangePasswordDto
    {
        public string? CurrentPassword { get; set; }

        [Required]
        [MinLength(6)]
        public required string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public required string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// DTO pour la confirmation d'email
    /// </summary>
    public class ConfirmEmailDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Token { get; set; }
    }
}