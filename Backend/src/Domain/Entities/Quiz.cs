using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Quiz
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid JobOfferId { get; set; }
        public virtual JobOffer JobOffer { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public int TimeLimitMinutes { get; set; } = 15;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public virtual ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    }
}
