using System;
using System.Collections.Generic;

namespace Application.DTOs.Recruiter
{
    public class DashboardStatsDto
    {
        public int ActiveJobOffers { get; set; }
        public int TotalApplications { get; set; }
        public int AiAnalysesCount { get; set; }
        public int PlannedInterviews { get; set; }

        public double OffersTrend { get; set; }
        public double ApplicationsTrend { get; set; }
        public double InterviewsTrend { get; set; }
        public int AvgApplicationsPerOffer { get; set; }
        public int ProcessedApplications { get; set; }

        public List<RecentOfferDto> RecentOffers { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
        public List<int> MonthlyApplications { get; set; } = new();
        public List<int> MonthlyProcessed { get; set; } = new();
        public List<int> WeeklyFlow { get; set; } = new();
        public List<int> WeeklyProcessed { get; set; } = new();
        public List<TeamMemberStatDto> TeamStats { get; set; } = new();
        public List<StatusBreakdownItemDto> StatusBreakdown { get; set; } = new();
        public List<TalentQualityDto> TalentDistribution { get; set; } = new();
        public List<SkillStatDto> TopSkills { get; set; } = new();
    }

    public class SkillStatDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class TalentQualityDto
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Percentage { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class StatusBreakdownItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class TeamMemberStatDto
    {
        public string Name { get; set; }
        public int OffersCount { get; set; }
        public string Initials { get; set; }
    }

    public class RecentOfferDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int ApplicationsCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class RecentActivityDto
    {
        public string Title { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public int Score { get; set; }
    }

    public class RecruiterMemberDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class InviteRecruiterDto
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public string Role { get; set; } = "Recruiter";
        public Guid? DepartmentId { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
