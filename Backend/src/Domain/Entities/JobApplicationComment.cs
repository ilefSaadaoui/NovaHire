using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class JobApplicationComment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid JobApplicationId { get; set; }

        [ForeignKey("JobApplicationId")]
        public virtual JobApplication? JobApplication { get; set; }

        [Required]
        public Guid AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public virtual User? Author { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Future: support simple threading
        public Guid? ParentCommentId { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
