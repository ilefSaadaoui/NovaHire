using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    /// <summary>
    /// Représente une évaluation d'une candidature par un recruteur spécifique.
    /// Utilisé pour l'évaluation collaborative.
    /// </summary>
    public class ApplicationRating
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid JobApplicationId { get; set; }
        public virtual JobApplication? JobApplication { get; set; }

        [Required]
        public Guid RecruiterId { get; set; }
        public virtual User? Recruiter { get; set; }

        [Range(1, 5)]
        public int Score { get; set; } // Note de 1 à 5

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
