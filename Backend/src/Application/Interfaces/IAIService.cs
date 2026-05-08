using System;
using System.Threading.Tasks;
using Application.DTOs.AI;

namespace Application.Interfaces
{
    public interface IAIService
    {
        Task<AIAnalysisResponseDto> AnalyzeCVAsync(string filePath, string jobTitle, string jobDescription, int weightExp, int weightEdu, int weightSkills, string language = "fr");
        Task<ExtractedDataDto> ExtractCVAsync(string filePath);
        Task<AIQuizResponseDto> GenerateQuizAsync(string jobTitle, string jobDescription, int numQuestions = 10, string language = "fr", string difficulty = "Mid", List<string>? topics = null);
        Task<string> GenerateJobDescriptionAsync(string jobTitle, string? keywords = null, string language = "fr");
        Task<string> GenerateRejectionFeedbackAsync(string candidateName, string jobTitle, string recruiterNotes, string language = "fr");
        Task<string> GenerateOfferLetterAsync(string candidateName, string jobTitle, decimal salary, string currency, string startDate, string benefits, string recruiterName, string companyName, string language = "fr");
    }
}
