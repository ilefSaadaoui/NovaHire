using System;

namespace Domain.Entities
{
    public class CandidateQuizResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid JobApplicationId { get; set; }
        public virtual JobApplication JobApplication { get; set; }

        public Guid QuizId { get; set; }
        public virtual Quiz Quiz { get; set; }

        public double Score { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
        
        // Stockage des réponses du candidat pour historique
        public string CandidateAnswersJson { get; set; }
    }
}
