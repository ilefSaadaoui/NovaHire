using System;
using System.Collections.Generic;

namespace Application.DTOs.Public
{
    public class PublicApplicationDto
    {
        public string ShareToken { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? LinkedIn { get; set; }
        public string? Portfolio { get; set; }
        public string? CoverLetter { get; set; }

        // Custom fields can be expanded later if needed
    }

    public class PublicOfferDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }
        public string? CompanyDescription { get; set; }
        public string? CompanyIndustry { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? CompanyCountry { get; set; }
        public string? CompanyPhone { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? SalaryRange { get; set; }
        public bool ShowSalary { get; set; }
        public string? RemotePolicy { get; set; }
        public string? ExperienceLevel { get; set; }
        public string? PageColor { get; set; }
        public string? PageSecondaryColor { get; set; }
        public string? WelcomeMsg { get; set; }
        public List<string> Skills { get; set; } = new();

        // Pondération IA (pour information frontend)
        public int WeightExperience { get; set; }
        public int WeightEducation { get; set; }
        public int WeightSkills { get; set; }

        // Form Configuration for Dynamic Rendering
        public bool RequireFullName { get; set; } = true;
        public bool RequireEmail { get; set; } = true;
        public bool RequirePhone { get; set; }
        public bool RequireCV { get; set; } = true;
        public bool RequireCoverLetter { get; set; }
        public bool RequirePortfolio { get; set; }
        public bool RequireLinkedIn { get; set; }

        // Quiz Information
        public bool HasQuiz { get; set; }
        public int QuizTimeLimit { get; set; }
    }
}
