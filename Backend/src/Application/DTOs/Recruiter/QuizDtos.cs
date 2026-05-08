using System;
using System.Collections.Generic;

namespace Application.DTOs.Recruiter
{
    public class QuizDto
    {
        public Guid Id { get; set; }
        public Guid JobOfferId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TimeLimitMinutes { get; set; }
        public List<QuizQuestionDto> Questions { get; set; }
    }

    public class QuizQuestionDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public List<string> Options { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public string Explanation { get; set; }
    }

    public class CreateQuizRequest
    {
        public Guid JobOfferId { get; set; }
        public int NumQuestions { get; set; } = 10;
        public string Language { get; set; } = "fr";
        public string Difficulty { get; set; } = "Mid"; // Junior, Mid, Senior, Expert
        public List<string> Topics { get; set; } = new List<string>();
    }

    public class SubmitQuizRequest
    {
        public Guid JobApplicationId { get; set; }
        public Guid QuizId { get; set; }
        public List<int> Answers { get; set; }
    }

    public class QuizResultDto
    {
        public double Score { get; set; }
        public DateTime CompletedAt { get; set; }
        public List<QuestionReviewDto> Reviews { get; set; } = new List<QuestionReviewDto>();
    }

    public class QuestionReviewDto
    {
        public int QuestionIndex { get; set; }
        public string Text { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int CorrectAnswerIndex { get; set; }
        public int CandidateAnswerIndex { get; set; }
        public bool IsCorrect { get; set; }
        public string? Explanation { get; set; }
    }
}
