using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Interview
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid JobApplicationId { get; set; }
        public virtual JobApplication? JobApplication { get; set; }

        [Required]
        public Guid RecruiterId { get; set; }
        public virtual User? Recruiter { get; set; }

        [Required]
        public DateTime ScheduledAt { get; set; }

        [Required]
        [MaxLength(20)]
        public required string Type { get; set; } // "visio", "phone", "onsite"

        [MaxLength(500)]
        public string? LocationOrLink { get; set; }

        public string? Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public InterviewStatus Status { get; set; } = InterviewStatus.Planned;
    }

    public enum InterviewStatus
    {
        Planned = 0,
        Completed = 1,
        Cancelled = 2,
        Rescheduled = 3,
        NoShow = 4
    }
}
