using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid JobApplicationId { get; set; }
        public virtual JobApplication? JobApplication { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Metadonnées optionnelles (ex: "interview_scheduled", "new_application")
        public string? Type { get; set; }
    }
}
