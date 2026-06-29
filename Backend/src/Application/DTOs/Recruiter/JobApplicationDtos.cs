using System;
using System.Collections.Generic;

namespace Application.DTOs.Recruiter
{
    public class JobApplicationDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string? Role { get; set; } // Current job title if known
        public int? Score { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Stage { get; set; } = string.Empty; // Mapping from ApplicationStatus
        public string? AiSummary { get; set; }
        public string? ResumeUrl { get; set; }
        public List<string> Skills { get; set; } = new();
        public int CommentsCount { get; set; }
        public double? AverageRating { get; set; }
        public int? QuizScore { get; set; }
        public bool QuizSent { get; set; }
        public DateTime? QuizExpiresAt { get; set; }
    }

    public class CandidateProfileDto
    {
        public Guid Id { get; set; }
        public Guid JobOfferId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public int OverallScore { get; set; }
        public string AiSummary { get; set; } = string.Empty;
        public List<CandidateCriteriaDto> Criteria { get; set; } = new();
        public List<WorkExperienceDto> Experiences { get; set; } = new();
        public List<EducationDto> Education { get; set; } = new();
        public List<SkillGroupDto> SkillGroups { get; set; } = new();
        public string CoverLetter { get; set; } = string.Empty;
        public string? ResumeUrl { get; set; }
        public List<string> Strengths { get; set; } = new();
        public List<string> Weaknesses { get; set; } = new();
        public string Stage { get; set; } = string.Empty;
        public List<ApplicationCommentDto> Comments { get; set; } = new();
        public List<TimelineEventDto> Timeline { get; set; } = new();
        public List<string> RequiredSkills { get; set; } = new();
        public List<InterviewQuestionDto> InterviewQuestions { get; set; } = new();
        public double? AverageRecruiterRating { get; set; }
        public int? MyRating { get; set; }
        public int? QuizScore { get; set; }
        public bool QuizSent { get; set; }
        public DateTime? QuizExpiresAt { get; set; }
        public bool HasQuizGenerated { get; set; }
    }

    public class InterviewQuestionDto
    {
        public string Category { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public int? Score { get; set; }
        public string? Notes { get; set; }
    }

    public class ApplicationCommentDto
    {
        public Guid Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
    }

    public class CandidateCriteriaDto
    {
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }
    }

    public class WorkExperienceDto
    {
        public Guid Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string CompanyInitial { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    public class EducationDto
    {
        public Guid Id { get; set; }
        public string Degree { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
    }

    public class SkillGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
    }

    public class TimelineEventDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string Color { get; set; } = "#94a3b8";
        public string Icon { get; set; } = "default";
        public string? Actor { get; set; }
    }

    public class UpdateExtractedDataDto
    {
        public List<UpdateWorkExperienceDto> Experiences { get; set; } = new();
        public List<UpdateEducationDto> Education { get; set; } = new();
        public List<string> Skills { get; set; } = new();
    }

    public class UpdateWorkExperienceDto
    {
        public string Role { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateEducationDto
    {
        public string Degree { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public string? Year { get; set; }
    }
    public class ApplicationRatingDto
    {
        public Guid Id { get; set; }
        public string RecruiterName { get; set; } = string.Empty;
        public int Score { get; set; }
        public string? Comment { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class ApplicationRatingRequest
    {
        public int Score { get; set; }
        public string? Comment { get; set; }
    }

    public class ComparisonDto
    {
        public List<CandidateComparisonItemDto> Candidates { get; set; } = new();
        public List<string> CommonSkills { get; set; } = new();
    }

    public class CandidateComparisonItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OverallScore { get; set; }
        public int ExperienceScore { get; set; }
        public int EducationScore { get; set; }
        public int SkillsScore { get; set; }
        public List<string> TopSkills { get; set; } = new();
        public string? Initials { get; set; }
    }
}
