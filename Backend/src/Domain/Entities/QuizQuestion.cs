using System;

namespace Domain.Entities
{
    public class QuizQuestion
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid QuizId { get; set; }
        public virtual Quiz Quiz { get; set; }

        public string Text { get; set; }
        public string Type { get; set; } // "Technical" or "SoftSkill"
        
        // Stockage des options au format JSON pour plus de flexibilité
        public string OptionsJson { get; set; } 
        
        public int CorrectAnswerIndex { get; set; }
        public string Explanation { get; set; }
    }
}
