using System;
using System.Collections.Generic;
using Domain.Entities;

namespace Application.DTOs.CompanyAdmin
{
    public class CompanyStatsDto
    {
        public int ActiveJobOffers { get; set; }
        public int TotalApplications { get; set; }
        public int ApplicationsToday { get; set; }
        public int ApplicationsThisWeek { get; set; }
        public int AiAnalysesCount { get; set; }
        public double AvgAiScore { get; set; }
        public int PlannedInterviews { get; set; }

        public List<RecentOfferDto> RecentOffers { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
        public List<int> MonthlyApplications { get; set; } = new();
        public List<TeamMemberStatsDto> TeamStats { get; set; } = new();
        public List<DepartmentStatDto> DepartmentStats { get; set; } = new();
        public GlobalFunnelDto FunnelData { get; set; } = new();
        public double AverageTimeToHire { get; set; }
        public double AiEfficiencyRate { get; set; }
    }



    public class RecentOfferDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int ApplicationsCount { get; set; }
    }

    public class RecentActivityDto
    {
        public string Title { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Offer, Application, Interview
    }

    public class TeamMemberStatsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public int OffersCount { get; set; }
        public int InterviewsCount { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class DepartmentStatDto
    {
        public string Name { get; set; } = string.Empty;
        public int OffersCount { get; set; }
        public int ApplicationsCount { get; set; }
    }

    public class GlobalFunnelDto
    {
        public int Sourcing { get; set; }
        public int Screening { get; set; }
        public int Interview { get; set; }
        public int Offer { get; set; }
    }

    public class CompanyBrandingDto
    {
        public string? LogoUrl { get; set; }
        public string PrimaryColor { get; set; } = "#FFD700";
        public string SecondaryColor { get; set; } = "#000000";
        public string CompanyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Industry { get; set; }
        public string? Website { get; set; }
        public string? Country { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class UpdateBrandingRequest
    {
        public string? LogoUrl { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? CompanyName { get; set; }
        public string? Description { get; set; }
        public string? Industry { get; set; }
        public string? Website { get; set; }
        public string? Country { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class TeamMemberDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? JobTitle { get; set; }
        public string? AvatarUrl { get; set; }
        public int OffersCount { get; set; }
        public int InterviewsCount { get; set; }
    }

    public class InviteMemberRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public string Role { get; set; } = "Recruiter";
        public Guid? DepartmentId { get; set; }
    }

    public class UpdateRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }

    public class UpdateMemberDetailsRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
        public Guid? DepartmentId { get; set; }
    }
}
