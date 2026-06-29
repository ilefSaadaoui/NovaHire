using System;

namespace Application.DTOs.Recruiter
{
    public class InterviewCreateDto
    {
        /// <summary>Date au format ISO yyyy-MM-dd envoyée par le front (input type="date").</summary>
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Type { get; set; } = "visio";
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class InterviewDto
    {
        public Guid Id { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? LocationOrLink { get; set; }
        public string? Message { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class InterviewUpdateDto
    {
        public string? LocationOrLink { get; set; }
        public string? Message { get; set; }
        /// <summary>Planned, Completed, Cancelled, Rescheduled, NoShow</summary>
        public string? Status { get; set; }
        /// <summary>Date du rendez-vous (reprogrammation).</summary>
        public DateTime? Date { get; set; }
        /// <summary>Heure au format HH:mm (reprogrammation).</summary>
        public string? Time { get; set; }
    }
}
