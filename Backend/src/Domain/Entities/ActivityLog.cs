using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ActivityLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Action { get; set; } = string.Empty;

        public string? Details { get; set; }

        public Guid? UserId { get; set; }

        public string? UserEmail { get; set; }

        public Guid? CompanyId { get; set; }

        public string? EntityType { get; set; }

        public string? EntityId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? IPAddress { get; set; }
    }
}
