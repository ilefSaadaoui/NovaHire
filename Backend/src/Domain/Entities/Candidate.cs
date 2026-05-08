using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Candidate
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public required string Email { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public string? LinkedInUrl { get; set; }
        public string? PortfolioUrl { get; set; }

        // Final CV URL (points to the most recently uploaded CV)
        public string? MainCVUrl { get; set; }

        // Tenant Isolation
        [Required]
        public Guid CompanyId { get; set; }
        public virtual Company? Company { get; set; }

        // Navigation
        public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
