using System;
using System.Collections.Generic;
using Domain.Entities;

namespace Application.DTOs.Recruiter
{
    public class JobOfferCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Type { get; set; } // Map to enum
        public string? Location { get; set; }
        public string? Department { get; set; }
        public int? SalaryMin { get; set; }
        public int? SalaryMax { get; set; }
        public bool SalaryConfidential { get; set; }
        public string? RemotePolicy { get; set; }
        public string? ExperienceLevel { get; set; }
        public List<string> Skills { get; set; } = new();
        public string? Visibility { get; set; } // Map to status
        public DateTime? Deadline { get; set; }

        // Configuration du formulaire
        public JobOfferFormConfigDto? FormConfig { get; set; }

        // Pondération IA
        public int WeightExperience { get; set; } = 40;
        public int WeightEducation { get; set; } = 30;
        public int WeightSkills { get; set; } = 30;

        /// <summary>Score minimum (0 = désactivé). Candidats sous ce seuil sont refusés après analyse IA.</summary>
        public int AutoRejectThreshold { get; set; } = 0;
    }

    public class JobOfferDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Department { get; set; }
        public string? RemotePolicy { get; set; }
        public string? ExperienceLevel { get; set; }
        public List<string> Skills { get; set; } = new();
        public DateTime? Deadline { get; set; }
        public string? SalaryRange { get; set; }
        public bool SalaryConfidential { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ShareToken { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ApplicationsCount { get; set; }

        public JobOfferFormConfigDto? FormConfig { get; set; }

        // Pondération IA
        public int WeightExperience { get; set; }
        public int WeightEducation { get; set; }
        public int WeightSkills { get; set; }
        public int AutoRejectThreshold { get; set; }
    }

    public class JobOfferFormConfigDto
    {
        public bool RequireFullName { get; set; }
        public bool RequireEmail { get; set; }
        public bool RequirePhone { get; set; }
        public bool RequireCV { get; set; }
        public bool RequireCoverLetter { get; set; }
        public bool RequirePortfolio { get; set; }
        public bool RequireLinkedIn { get; set; }
    }
}
