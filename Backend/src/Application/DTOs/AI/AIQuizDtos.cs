using System.Collections.Generic;

namespace Application.DTOs.AI
{
    public class AIQuizResponseDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<AIQuizQuestionDto> Questions { get; set; }
    }

    public class AIQuizQuestionDto
    {
        public string Text { get; set; }
        public string Type { get; set; } // Technical | SoftSkill
        public List<string> Options { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public string Explanation { get; set; }
    }
}
