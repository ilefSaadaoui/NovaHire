using System;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities
{
    public class ContactMessage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string FullName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public required string Email { get; set; }

        [MaxLength(200)]
        public string? Company { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        [MaxLength(2000)]
        public required string Message { get; set; }

        public ContactMessageStatus Status { get; set; } = ContactMessageStatus.New;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
    }
}
