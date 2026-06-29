using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    /// <summary>
    /// DTO pour l'inscription complète d'une entreprise et de son administrateur
    /// </summary>
    public class CompanyRegisterDto
    {
        // Informations Société
        [Required]
        [MaxLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Industry { get; set; } = string.Empty;



        [Required]
        public string EmployeesRange { get; set; } = string.Empty;

        // Informations Administrateur
        [Required]
        [MaxLength(100)]
        public string AdminFirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string AdminLastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string AdminEmail { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères, une majuscule et un chiffre.")]
        public string AdminPassword { get; set; } = string.Empty;

        // Informations Recruteur optionnel
        public string? RecruiterEmail { get; set; }
    }
}
