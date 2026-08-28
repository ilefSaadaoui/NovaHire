#pragma warning disable CS8601, CS8602, CS8604
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Application.DTOs.Recruiter;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    /// <summary>
    /// Contrôleur principal pour l'espace Recruteur.
    /// Fournit les services indispensables pour la gestion quotidienne : statistiques,
    /// invitations, dossiers candidats, examens IA, guide d'entretien interactif, notations,
    /// envoi de quiz et de courriels personnalisés de proposition ou de refus.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RecruiterOrAbove")]
    public class RecruiterController : ControllerBase
    {
        private readonly IJobOfferRepository _jobOfferRepository;
        private readonly IJobApplicationRepository _jobApplicationRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IEmailService _emailService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAIService _aiService;
        private readonly IExportService _exportService;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RecruiterController> _logger;

        public RecruiterController(
            IJobOfferRepository jobOfferRepository,
            IJobApplicationRepository jobApplicationRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IEmailService emailService,
            ICurrentUserService currentUserService,
            IAIService aiService,
            IExportService exportService,
            IWebHostEnvironment environment,
            ApplicationDbContext db,
            IConfiguration configuration,
            ILogger<RecruiterController> logger)
        {
            _jobOfferRepository = jobOfferRepository;
            _jobApplicationRepository = jobApplicationRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _emailService = emailService;
            _currentUserService = currentUserService;
            _aiService = aiService;
            _exportService = exportService;
            _environment = environment;
            _db = db;
            _configuration = configuration;
            _logger = logger;
        }

        private Guid GetCompanyId()
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                throw new UnauthorizedAccessException("CompanyId claim is missing or invalid.");
            }
            return companyId;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("UserId claim is missing or invalid.");
            }
            return userId;
        }

        /// <summary>
        /// Récupère les données statistiques du tableau de bord RH pour le recruteur connecté.
        /// Propose des filtres de personnalisation (mes offres uniquement) ou de période.
        /// </summary>
        /// <param name="personal">Si vrai, filtre uniquement sur les offres créées par le recruteur de session.</param>
        /// <param name="period">Fenêtre de temps (day, week, month).</param>
        /// <param name="section">Section spécifique du tableau de bord (optionnelle).</param>
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats(
            [FromQuery] bool personal = false,
            [FromQuery] string? period = null,
            [FromQuery] string? section = null)
        {
            try
            {
                var companyId = GetCompanyId();
                var currentUserId = _currentUserService.UserId ?? Guid.Empty;
                var userRole = _currentUserService.Role;

                bool isRecruiterOnly = userRole == "Recruiter";
                bool filterByMe = personal || isRecruiterOnly;

                // Compute date cutoff from period param
                DateTime? cutoff = period switch
                {
                    "day" => DateTime.UtcNow.AddDays(-1),
                    "week" => DateTime.UtcNow.AddDays(-7),
                    "month" => DateTime.UtcNow.AddDays(-30),
                    _ => null
                };

                var offersQuery = _db.JobOffers.AsNoTracking();
                if (filterByMe && currentUserId != Guid.Empty)
                    offersQuery = offersQuery.Where(o => o.CreatedById == currentUserId);
                else
                    offersQuery = offersQuery.Where(o => o.CompanyId == companyId);
                
                var appsQuery = _db.JobApplications.AsNoTracking();
                if (filterByMe && currentUserId != Guid.Empty)
                    appsQuery = appsQuery.Where(a => a.JobOffer!.CreatedById == currentUserId);
                else
                    appsQuery = appsQuery.Where(a => a.JobOffer!.CompanyId == companyId);

                // Variables pour la période courante
                var currentOffersQuery = offersQuery;
                var currentAppsQuery = appsQuery;

                if (cutoff.HasValue)
                {
                    currentOffersQuery = currentOffersQuery.Where(o => o.CreatedAt >= cutoff.Value);
                    currentAppsQuery = currentAppsQuery.Where(a => a.AppliedAt >= cutoff.Value);
                }

                int activeOffersCount = await currentOffersQuery.CountAsync(o => o.Status == JobOfferStatus.Published);
                int totalAppsCount = await currentAppsQuery.CountAsync();
                int analyzedCount = await currentAppsQuery.CountAsync(a => a.AiScore.HasValue);
                int processedCount = await currentAppsQuery.CountAsync(a => a.Status != ApplicationStatus.Submitted);

                // Previous period buckets for trend computation
                int prevOffersCount = 0;
                int prevAppsCount = 0;

                if (cutoff.HasValue)
                {
                    var windowSpan = DateTime.UtcNow - cutoff.Value;
                    var prevCutoffEnd = cutoff.Value;
                    var prevCutoffStart = cutoff.Value - windowSpan;

                    prevOffersCount = await offersQuery.CountAsync(o => o.CreatedAt >= prevCutoffStart && o.CreatedAt < prevCutoffEnd && o.Status == JobOfferStatus.Published);
                    prevAppsCount = await appsQuery.CountAsync(a => a.AppliedAt >= prevCutoffStart && a.AppliedAt < prevCutoffEnd);
                }

                double ComputeTrend(int current, int previous)
                {
                    if (previous == 0) return current > 0 ? 100.0 : 0.0;
                    return Math.Round(((current - previous) / (double)previous) * 100.0, 1);
                }

                var plannedInterviews = await _db.Interviews!
                    .CountAsync(i => i.JobApplication != null && i.JobApplication.JobOffer != null && i.JobApplication.JobOffer.CompanyId == companyId && i.ScheduledAt >= DateTime.UtcNow);

                var pastInterviews = await _db.Interviews!
                    .CountAsync(i => i.JobApplication != null && i.JobApplication.JobOffer != null && i.JobApplication.JobOffer.CompanyId == companyId && i.ScheduledAt < DateTime.UtcNow && i.ScheduledAt >= DateTime.UtcNow.AddDays(-7));

                var stats = new DashboardStatsDto
                {
                    ActiveJobOffers = activeOffersCount,
                    TotalApplications = totalAppsCount,
                    AiAnalysesCount = analyzedCount,
                    ProcessedApplications = processedCount,
                    PlannedInterviews = plannedInterviews,
                    OffersTrend = ComputeTrend(activeOffersCount, prevOffersCount),
                    ApplicationsTrend = ComputeTrend(totalAppsCount, prevAppsCount),
                    InterviewsTrend = ComputeTrend(plannedInterviews, pastInterviews),
                    AvgApplicationsPerOffer = activeOffersCount > 0 ? (int)Math.Round(totalAppsCount / (double)activeOffersCount) : 0
                };

                // RecentOffers: load raw data first, then project in memory (avoid ToString() translation issues)
                var rawRecentOffers = await currentOffersQuery
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(4)
                    .Select(o => new
                    {
                        o.Id,
                        o.Title,
                        o.Location,
                        TypeInt = (int)o.Type,
                        AppCount = o.Applications != null ? o.Applications.Count : 0,
                        StatusInt = (int)o.Status
                    }).ToListAsync();

                stats.RecentOffers = rawRecentOffers.Select(o => new RecentOfferDto
                {
                    Id = o.Id,
                    Title = o.Title,
                    Location = o.Location ?? "N/A",
                    Type = ((Domain.Entities.JobType)o.TypeInt).ToString(),
                    ApplicationsCount = o.AppCount,
                    Status = ((JobOfferStatus)o.StatusInt).ToString().ToLower()
                }).ToList();

                // RecentActivities: GetTimeAgo() is a C# method — cannot be translated to SQL
                var rawRecentActivities = await currentAppsQuery
                    .OrderByDescending(a => a.AppliedAt)
                    .Take(5)
                    .Select(a => new
                    {
                        FirstName = a.Candidate!.FirstName,
                        LastName = a.Candidate.LastName,
                        a.AppliedAt,
                        a.AiScore
                    }).ToListAsync();

                stats.RecentActivities = rawRecentActivities.Select(a => new RecentActivityDto
                {
                    Title = $"Candidature reçue – {a.FirstName} {a.LastName}",
                    TimeAgo = GetTimeAgo(a.AppliedAt),
                    Score = (int)(a.AiScore ?? 0)
                }).ToList();

                // Lightweight projection for time-series charts to avoid memory leaks
                var appsForCharts = await appsQuery
                    .Select(a => new { a.AppliedAt, a.UpdatedAt, a.Status })
                    .ToListAsync();

                var now = DateTime.UtcNow;

                bool IsProcessedStat(ApplicationStatus status, DateTime? updatedAt) => status != ApplicationStatus.Submitted && updatedAt.HasValue;

                if (cutoff.HasValue && period == "day")
                {
                    stats.MonthlyApplications = Enumerable.Range(0, 6)
                        .Select(i => appsForCharts.Count(a => a.AppliedAt >= now.AddHours(-4 * (5 - i)) && a.AppliedAt < now.AddHours(-4 * (4 - i))))
                        .ToList();
                    stats.MonthlyProcessed = Enumerable.Range(0, 6)
                        .Select(i => appsForCharts.Count(a => IsProcessedStat(a.Status, a.UpdatedAt) && a.UpdatedAt >= now.AddHours(-4 * (5 - i)) && a.UpdatedAt < now.AddHours(-4 * (4 - i))))
                        .ToList();
                }
                else if (cutoff.HasValue && period == "week")
                {
                    stats.MonthlyApplications = Enumerable.Range(0, 7)
                        .Select(i => now.AddDays(-(6 - i)))
                        .Select(day => appsForCharts.Count(a => a.AppliedAt.Date == day.Date))
                        .ToList();
                    stats.MonthlyProcessed = Enumerable.Range(0, 7)
                        .Select(i => now.AddDays(-(6 - i)))
                        .Select(day => appsForCharts.Count(a => IsProcessedStat(a.Status, a.UpdatedAt) && a.UpdatedAt!.Value.Date == day.Date))
                        .ToList();
                }
                else
                {
                    stats.MonthlyApplications = Enumerable.Range(0, 6)
                        .Reverse()
                        .Select(i => now.AddMonths(-i))
                        .Select(month => appsForCharts.Count(a => a.AppliedAt.Month == month.Month && a.AppliedAt.Year == month.Year))
                        .ToList();
                    stats.MonthlyProcessed = Enumerable.Range(0, 6)
                        .Reverse()
                        .Select(i => now.AddMonths(-i))
                        .Select(month => appsForCharts.Count(a => IsProcessedStat(a.Status, a.UpdatedAt) && a.UpdatedAt!.Value.Month == month.Month && a.UpdatedAt!.Value.Year == month.Year))
                        .ToList();
                }

                // Weekly flow (always last 7 days, Mon-Sun) for the bottom line chart
                var weekStart = now.Date.AddDays(-6);
                stats.WeeklyFlow = Enumerable.Range(0, 7)
                    .Select(i => weekStart.AddDays(i))
                    .Select(day => appsForCharts.Count(a => a.AppliedAt.Date == day.Date))
                    .ToList();
                stats.WeeklyProcessed = Enumerable.Range(0, 7)
                    .Select(i => weekStart.AddDays(i))
                    .Select(day => appsForCharts.Count(a => IsProcessedStat(a.Status, a.UpdatedAt) && a.UpdatedAt!.Value.Date == day.Date))
                    .ToList();

                // Status Breakdown calculation
                if (totalAppsCount > 0)
                {
                    stats.StatusBreakdown = Enum.GetValues(typeof(ApplicationStatus))
                        .Cast<ApplicationStatus>()
                        .Select(s =>
                        {
                            int count = appsForCharts.Count(a => a.Status == s);
                            return new StatusBreakdownItemDto
                            {
                                Name = s.ToString(),
                                Count = count,
                                Percentage = Math.Round((double)count / totalAppsCount * 100, 1),
                                Color = s switch
                                {
                                    ApplicationStatus.Submitted => "#fbbf24",
                                    ApplicationStatus.Shortlisted => "#10b981",
                                    ApplicationStatus.Interview => "#818cf8",
                                    ApplicationStatus.Interviewed => "#a78bfa",
                                    ApplicationStatus.UnderReview => "#fbbf24",
                                    ApplicationStatus.Rejected => "#f472b6",
                                    ApplicationStatus.Accepted => "#10b981",
                                    _ => "#94a3b8"
                                }
                            };
                        })
                        .Where(x => x.Count > 0)
                        .OrderByDescending(x => x.Count)
                        .ToList();
                }

                // Talent Quality Distribution calculation
                if (totalAppsCount > 0)
                {
                    stats.TalentDistribution = new List<TalentQualityDto>
                    {
                        new TalentQualityDto {
                            Label = "Elite (85%+)",
                            Count = await currentAppsQuery.CountAsync(a => a.AiScore >= 85),
                            Percentage = (int)Math.Round((double)await currentAppsQuery.CountAsync(a => a.AiScore >= 85) / totalAppsCount * 100),
                            Color = "#10b981"
                        },
                        new TalentQualityDto {
                            Label = "Qualifiés (70-84%)",
                            Count = await currentAppsQuery.CountAsync(a => a.AiScore >= 70 && a.AiScore < 85),
                            Percentage = (int)Math.Round((double)await currentAppsQuery.CountAsync(a => a.AiScore >= 70 && a.AiScore < 85) / totalAppsCount * 100),
                            Color = "#fbbf24"
                        },
                        new TalentQualityDto {
                            Label = "À réévaluer (<70%)",
                            Count = await currentAppsQuery.CountAsync(a => a.AiScore < 70),
                            Percentage = (int)Math.Round((double)await currentAppsQuery.CountAsync(a => a.AiScore < 70) / totalAppsCount * 100),
                            Color = "#f472b6"
                        }
                    };
                }

                // Top Skills calculation
                // EF Core cannot translate SelectMany on a JSON List<string> column to SQL.
                // We load the skill lists into memory first, then group in C#.
                var rawSkillLists = await currentAppsQuery
                    .Where(a => a.AIAnalysis != null && a.AIAnalysis.IdentifiedSkills != null)
                    .Select(a => a.AIAnalysis!.IdentifiedSkills!)
                    .ToListAsync();

                var allSkills = rawSkillLists
                    .SelectMany(skills => skills)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .GroupBy(s => s.Trim(), StringComparer.OrdinalIgnoreCase)
                    .Select(g => new SkillStatDto { Name = g.Key, Count = g.Count() })
                    .OrderByDescending(s => s.Count)
                    .Take(5)
                    .ToList();

                stats.TopSkills = allSkills;

                if (!filterByMe)
                {
                    var teamUsers = await _userRepository.GetByCompanyIdAsync(companyId);
                    var allOffersForTeam = await _jobOfferRepository.GetByCompanyIdAsync(companyId);

                    stats.TeamStats = teamUsers
                        .Where(u => u.Role == UserRole.Recruiter || u.Role == UserRole.CompanyAdmin)
                        .Select(u => new TeamMemberStatDto
                        {
                            Name = $"{(u.FirstName ?? "")} {(u.LastName ?? "")}".Trim(),
                            OffersCount = allOffersForTeam.Count(o => o.CreatedById == u.Id),
                            Initials = $"{(u.FirstName != null && u.FirstName.Length > 0 ? u.FirstName[0] : ' ')}{(u.LastName != null && u.LastName.Length > 0 ? u.LastName[0] : ' ')}".Trim().ToUpper()
                        })
                        .OrderByDescending(s => s.OffersCount)
                        .ToList();
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        /// <summary>
        /// Récupère la liste chronologique de tous les entretiens planifiés pour l'entreprise.
        /// </summary>
        [HttpGet("interviews")]
        public async Task<ActionResult<IEnumerable<InterviewDto>>> GetInterviews()
        {
            try
            {
                var companyId = GetCompanyId();
                
                var interviews = await _db.Interviews!
                    .Include(i => i.JobApplication)
                        .ThenInclude(a => a.Candidate)
                    .Include(i => i.JobApplication)
                        .ThenInclude(a => a.JobOffer)
                    .Where(i => i.JobApplication != null && i.JobApplication.JobOffer != null && i.JobApplication.JobOffer.CompanyId == companyId)
                    .OrderBy(i => i.ScheduledAt)
                    .ToListAsync();

                var dtos = interviews.Select(i => new InterviewDto
                {
                    Id = i.Id,
                    CandidateName = i.JobApplication?.Candidate != null 
                        ? $"{i.JobApplication.Candidate.FirstName} {i.JobApplication.Candidate.LastName}" 
                        : "Inconnu",
                    JobTitle = i.JobApplication?.JobOffer?.Title ?? "N/A",
                    ScheduledAt = i.ScheduledAt,
                    Type = i.Type ?? "visio",
                    LocationOrLink = i.LocationOrLink,
                    Message = i.Message,
                    Status = i.Status.ToString(),
                    Color = (i.Type ?? "visio").ToLower() switch
                    {
                        "visio" => "#0ea5e9",
                        "phone" => "#8b5cf6",
                        "onsite" => "#ec4899",
                        _ => "#64748b"
                    }
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetInterviews: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Récupère les collègues membres de l'équipe RH de l'entreprise.
        /// </summary>
        [HttpGet("team")]
        public async Task<IActionResult> GetTeamMembers()
        {
            try
            {
                var companyId = GetCompanyId();
                var users = await _userRepository.GetByCompanyIdAsync(companyId);

                var members = users.Select(u => new RecruiterMemberDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    Department = u.Department?.Name ?? "RH / Talent Acquisition",
                    IsActive = u.IsActive,
                    LastLoginAt = u.LastLoginAt
                }).ToList();

                return Ok(members);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RecruiterController] Error in GetTeamMembers: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Invite un nouveau collaborateur recruteur en lui générant des identifiants d'accès.
        /// Uniquement accessible aux administrateurs de l'entreprise ou rôles supérieurs.
        /// </summary>
        /// <param name="dto">Les coordonnées et assignation départementale du nouveau recruteur.</param>
        [HttpPost("invite")]
        [Authorize(Policy = "CompanyAdminOrAbove")]
        public async Task<IActionResult> InviteRecruiter([FromBody] InviteRecruiterDto dto)
        {
            try
            {
                var isSuperAdmin = User.IsInRole("SuperAdmin") || User.FindFirst(ClaimTypes.Role)?.Value == "SuperAdmin" || User.FindFirst("role")?.Value == "SuperAdmin";
                Guid companyId = Guid.Empty;

                if (isSuperAdmin)
                {
                    if (dto.CompanyId.HasValue && dto.CompanyId.Value != Guid.Empty)
                    {
                        companyId = dto.CompanyId.Value;
                    }
                }
                else
                {
                    companyId = GetCompanyId();
                }

                var adminId = GetUserId();
                var admin = await _userRepository.GetByIdAsync(adminId);

                // 1. Vérifier si l'email est déjà pris
                var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    // Si l'utilisateur appartient à la même société (ou si SuperAdmin), on autorise le renvoi
                    if (isSuperAdmin || (existingUser.CompanyId == companyId && existingUser.LastLoginAt == null))
                    {
                        var targetCompanyId = existingUser.CompanyId ?? companyId;
                        var invitationCompany = targetCompanyId != Guid.Empty ? await _companyRepository.GetActiveCompanyByIdAsync(targetCompanyId) : null;
                        var companyName = invitationCompany?.Name ?? "NovaHire";
                        var senderName = admin?.FullName ?? "L'Administrateur Plateforme";
                        string newPassword = GenerateSimpleTempPassword(14);
                        
                        existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                        if (!string.IsNullOrWhiteSpace(dto.FirstName)) existingUser.FirstName = dto.FirstName;
                        if (!string.IsNullOrWhiteSpace(dto.LastName)) existingUser.LastName = dto.LastName;
                        if (!string.IsNullOrWhiteSpace(dto.JobTitle)) existingUser.JobTitle = dto.JobTitle;
                        if (dto.DepartmentId.HasValue) existingUser.DepartmentId = dto.DepartmentId;
                        existingUser.MustChangePassword = true;
                        existingUser.UpdatedAt = DateTime.UtcNow;

                        await _userRepository.UpdateAsync(existingUser);
                        await _userRepository.SaveChangesAsync();
                        var success = await _emailService.SendRecruiterInvitationAsync(dto.Email, newPassword, companyName, senderName);
                        if (!success)
                        {
                            return Ok(new { 
                                message = $"Invitation réinitialisée avec succès. (Note: l'e-mail automatique n'a pas pu être envoyé par SMTP. Mot de passe temporaire : {newPassword})",
                                tempPassword = newPassword,
                                emailSent = false
                            });
                        }

                        return Ok(new { message = "Invitation renvoyée avec succès par email.", emailSent = true });
                    }

                    return BadRequest(new { message = "Cet email est déjà rattaché à un compte NovaHire actif." });
                }

                // 2. Récupérer la société
                Company? company = null;
                if (companyId != Guid.Empty)
                {
                    company = await _companyRepository.GetActiveCompanyByIdAsync(companyId);
                }
                else if (isSuperAdmin)
                {
                    var allCompanies = await _companyRepository.GetAllActiveAsync();
                    company = allCompanies.FirstOrDefault();
                    if (company != null) companyId = company.Id;
                }

                if (company == null && !isSuperAdmin) return BadRequest(new { message = "Société introuvable." });
                var companyNameFinal = company?.Name ?? "NovaHire";

                // 3. Générer un mot de passe temporaire complexe
                string tempPassword = GenerateSimpleTempPassword(14);

                // 4. Créer le compte recruteur
                var recruiter = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email.ToLower(),
                    JobTitle = dto.JobTitle,
                    Role = UserRole.Recruiter, // Strict role for this endpoint
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
                    CompanyId = companyId != Guid.Empty ? companyId : null,
                    DepartmentId = dto.DepartmentId,
                    IsActive = true,
                    MustChangePassword = true,
                    EmailConfirmed = true, // On considère l'invitation comme une validation
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(recruiter);
                await _userRepository.SaveChangesAsync();

                // 5. Envoyer l'e-mail premium
                var adminName = admin?.FullName ?? "L'Administrateur Plateforme";
                var invitationSuccess = await _emailService.SendRecruiterInvitationAsync(dto.Email, tempPassword, companyNameFinal, adminName);
                
                if (!invitationSuccess)
                {
                    return Ok(new { 
                        message = $"Membre invité avec succès ! (Note: l'e-mail automatique n'a pas pu être envoyé par SMTP. Mot de passe temporaire généré : {tempPassword})",
                        tempPassword = tempPassword,
                        emailSent = false
                    });
                }

                return Ok(new { message = "Invitation envoyée avec succès par email.", emailSent = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RecruiterController] Error in InviteRecruiter: {ex.Message}");
                return BadRequest(new { message = "Une erreur est survenue lors de l'envoi de l'invitation: " + ex.Message });
            }
        }

        private string GenerateSimpleTempPassword(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Récupère les candidatures reçues par l'entreprise, éventuellement filtrées par offre d'emploi spécifique.
        /// </summary>
        /// <param name="jobOfferId">Identifiant unique de l'offre d'emploi (optionnel).</param>
        [HttpGet("applications")]
        public async Task<IActionResult> GetApplications([FromQuery] Guid? jobOfferId)
        {
            try
            {
                var companyId = GetCompanyId();
                IEnumerable<JobApplication> apps;

                if (jobOfferId.HasValue)
                {
                    apps = await _jobApplicationRepository.GetByJobOfferIdAsync(jobOfferId.Value);
                }
                else
                {
                    apps = await _jobApplicationRepository.GetByCompanyIdAsync(companyId);
                }

                var dtos = apps
                    .OrderByDescending(a => a.AIAnalysis != null ? a.AIAnalysis.OverallScore : (a.AiScore ?? 0))
                    .Select(a => new JobApplicationDto
                    {
                        Id = a.Id,
                        FirstName = a.Candidate?.FirstName ?? "Sans",
                        LastName = a.Candidate?.LastName ?? "Prénom",
                        Email = a.Candidate?.Email ?? string.Empty,
                        Role = a.JobOffer?.Title ?? "Candidat",
                        Score = a.AIAnalysis?.OverallScore ?? a.AiScore,
                        Date = a.AppliedAt.ToString("dd/MM/yyyy"),
                        Stage = a.Status.ToString().ToLower(),
                        AiSummary = a.AIAnalysis?.AutoGeneratedSummary,
                        ResumeUrl = a.CVUrl,
                        Skills = a.AIAnalysis?.IdentifiedSkills ?? new List<string>(),
                        CommentsCount = a.Comments?.Count ?? 0,
                        AverageRating = a.Ratings != null && a.Ratings.Any() ? a.Ratings.Average(r => r.Score) : (double?)null,
                        QuizScore = a.QuizScore,
                        QuizSent = a.QuizSent,
                        QuizExpiresAt = a.QuizExpiresAt
                    }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RecruiterController] Error in GetApplications: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Récupère le profil d'évaluation complet d'un candidat par l'ID de sa candidature.
        /// Compte les évaluations, compile son historique d'activités, et récupère ses réponses.
        /// </summary>
        /// <param name="id">Identifiant de la candidature.</param>
        [HttpGet("applications/{id}")]
        public async Task<IActionResult> GetCandidateProfile(Guid id)
        {
            try
            {
                var app = await _jobApplicationRepository.GetByIdWithDetailsAsync(id);
                if (app == null) return NotFound();

                var companyId = GetCompanyId();
                if (app.JobOffer?.CompanyId != companyId) return Forbid();

                // Check if a quiz has been generated for this offer
                var hasQuizGenerated = await _db.Quizzes!
                    .IgnoreQueryFilters()
                    .AnyAsync(q => q.JobOfferId == app.JobOfferId && q.IsActive);

                // Fetch interviews for this application
                var interviews = await _db.Interviews!
                    .Include(i => i.Recruiter)
                    .Where(i => i.JobApplicationId == id)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();

                // Build timeline events from all sources
                var timelineEvents = new List<TimelineEventDto>();

                // 1. Application submitted
                timelineEvents.Add(new TimelineEventDto
                {
                    Id = Guid.NewGuid(),
                    Title = "Candidature Reçue",
                    Description = $"Candidature soumise pour le poste de {app.JobOffer?.Title ?? "N/A"}.",
                    Time = app.AppliedAt.ToString("dd/MM/yyyy HH:mm"),
                    TimeAgo = GetTimeAgo(app.AppliedAt),
                    Color = "#10b981",
                    Icon = "submit"
                });

                // 2. AI Analysis (if done)
                if (app.AIAnalysis != null && (app.AIAnalysis.OverallScore > 0 || app.AIAnalysis.IdentifiedSkills?.Count > 0))
                {
                    timelineEvents.Add(new TimelineEventDto
                    {
                        Id = Guid.NewGuid(),
                        Title = "CV Analysé par l'IA",
                        Description = $"Score global : {app.AIAnalysis.OverallScore}% — Formation : {app.AIAnalysis.EducationScore}%, Expérience : {app.AIAnalysis.ExperienceScore}%, Compétences : {app.AIAnalysis.SkillsScore}%.",
                        Time = app.AIAnalysis.AnalyzedAt.ToString("dd/MM/yyyy HH:mm"),
                        TimeAgo = GetTimeAgo(app.AIAnalysis.AnalyzedAt),
                        Color = "#8b5cf6",
                        Icon = "ai"
                    });
                }

                // 3. Status change (if updated after submission)
                if (app.UpdatedAt.HasValue && app.Status != ApplicationStatus.Submitted)
                {
                    var statusLabels = new Dictionary<ApplicationStatus, string>
                    {
                        { ApplicationStatus.UnderReview, "En cours d'examen" },
                        { ApplicationStatus.Shortlisted, "Présélectionné" },
                        { ApplicationStatus.Interview, "Entretien prévu" },
                        { ApplicationStatus.Interviewed, "Entretien réalisé" },
                        { ApplicationStatus.Rejected, "Refusé" },
                        { ApplicationStatus.Accepted, "Accepté" }
                    };
                    var statusColors = new Dictionary<ApplicationStatus, string>
                    {
                        { ApplicationStatus.UnderReview, "#f59e0b" },
                        { ApplicationStatus.Shortlisted, "#0ea5e9" },
                        { ApplicationStatus.Interview, "#818cf8" },
                        { ApplicationStatus.Interviewed, "#a78bfa" },
                        { ApplicationStatus.Rejected, "#ef4444" },
                        { ApplicationStatus.Accepted, "#10b981" }
                    };

                    var label = statusLabels.ContainsKey(app.Status) ? statusLabels[app.Status] : app.Status.ToString();
                    var color = statusColors.ContainsKey(app.Status) ? statusColors[app.Status] : "#94a3b8";

                    timelineEvents.Add(new TimelineEventDto
                    {
                        Id = Guid.NewGuid(),
                        Title = "Statut mis à jour",
                        Description = $"Le statut est passé à « {label} ».",
                        Time = app.UpdatedAt.Value.ToString("dd/MM/yyyy HH:mm"),
                        TimeAgo = GetTimeAgo(app.UpdatedAt.Value),
                        Color = color,
                        Icon = "stage"
                    });
                }

                // 4. Interviews
                foreach (var interview in interviews)
                {
                    var typeLabels = new Dictionary<string, string>
                    {
                        { "visio", "en visioconférence" },
                        { "phone", "par téléphone" },
                        { "onsite", "en personne" }
                    };
                    var typeLabel = typeLabels.ContainsKey(interview.Type) ? typeLabels[interview.Type] : interview.Type;
                    var recruiterName = interview.Recruiter != null
                        ? $"{interview.Recruiter.FirstName} {interview.Recruiter.LastName}"
                        : null;

                    timelineEvents.Add(new TimelineEventDto
                    {
                        Id = interview.Id,
                        Title = "Entretien Planifié",
                        Description = $"Entretien {typeLabel} prévu le {interview.ScheduledAt:dd/MM/yyyy} à {interview.ScheduledAt:HH:mm}.",
                        Time = interview.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                        TimeAgo = GetTimeAgo(interview.CreatedAt),
                        Color = "#6366f1",
                        Icon = "interview",
                        Actor = recruiterName
                    });
                }

                // 5. Comments
                if (app.Comments != null)
                {
                    foreach (var comment in app.Comments)
                    {
                        var authorName = comment.Author != null
                            ? $"{comment.Author.FirstName} {comment.Author.LastName}"
                            : "Recruteur";

                        timelineEvents.Add(new TimelineEventDto
                        {
                            Id = comment.Id,
                            Title = "Commentaire ajouté",
                            Description = comment.Content.Length > 120 ? comment.Content.Substring(0, 120) + "…" : comment.Content,
                            Time = comment.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                            TimeAgo = GetTimeAgo(comment.CreatedAt),
                            Color = "#0ea5e9",
                            Icon = "comment",
                            Actor = authorName
                        });
                    }
                }

                // 6. Notifications (exclude types already covered above)
                if (app.Notifications != null)
                {
                    foreach (var notif in app.Notifications.Where(n => n.Type != "interview_scheduled"))
                    {
                        timelineEvents.Add(new TimelineEventDto
                        {
                            Id = notif.Id,
                            Title = notif.Title,
                            Description = notif.Message,
                            Time = notif.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                            TimeAgo = GetTimeAgo(notif.CreatedAt),
                            Color = "#f59e0b",
                            Icon = "notification"
                        });
                    }
                }

                // Sort timeline: newest first
                timelineEvents = timelineEvents.OrderByDescending(e => e.Time).ToList();

                var dto = new CandidateProfileDto
                {
                    Id = app.Id,
                    JobOfferId = app.JobOfferId,
                    FullName = app.Candidate != null ? $"{app.Candidate.FirstName} {app.Candidate.LastName}" : "Candidat Anonyme",
                    Email = app.Candidate?.Email ?? string.Empty,
                    Phone = app.Candidate?.PhoneNumber ?? "—",
                    JobTitle = app.JobOffer?.Title ?? "—",
                    Location = app.JobOffer?.Location ?? "Remote",
                    Initials = ((app.Candidate?.FirstName?.Length > 0 ? app.Candidate.FirstName[0].ToString() : "C") + (app.Candidate?.LastName?.Length > 0 ? app.Candidate.LastName[0].ToString() : "A")).ToUpper(),
                    OverallScore = app.AIAnalysis?.OverallScore ?? 0,
                    AiSummary = app.AIAnalysis?.AutoGeneratedSummary ?? "Analyse en attente...",
                    CoverLetter = app.CoverLetterUrl ?? "Aucune lettre de motivation n'a été transmise pour cette candidature.",
                    Criteria = new List<CandidateCriteriaDto>
                    {
                        new CandidateCriteriaDto { Name = "Formation", Score = app.AIAnalysis?.EducationScore ?? 0 },
                        new CandidateCriteriaDto { Name = "Expérience", Score = app.AIAnalysis?.ExperienceScore ?? 0 },
                        new CandidateCriteriaDto { Name = "Compétences", Score = app.AIAnalysis?.SkillsScore ?? 0 }
                    },
                    Experiences = app.AIAnalysis?.ExtractedData?.WorkExperiences?.Select(e => {
                        // Build period string
                        string period;
                        if (e.StartDate.HasValue)
                        {
                            var startStr = e.StartDate.Value.ToString("MM/yyyy");
                            var endStr = e.IsCurrent ? "Présent" : (e.EndDate?.ToString("MM/yyyy") ?? "N/A");
                            period = $"{startStr} - {endStr}";
                        }
                        else
                        {
                            period = "Non précisé";
                        }

                        // Compute duration
                        string duration;
                        if (e.StartDate.HasValue)
                        {
                            var endDate = e.IsCurrent ? DateTime.UtcNow : (e.EndDate ?? DateTime.UtcNow);
                            var totalMonths = ((endDate.Year - e.StartDate.Value.Year) * 12) + (endDate.Month - e.StartDate.Value.Month);
                            var years = totalMonths / 12;
                            var months = totalMonths % 12;
                            if (years > 0 && months > 0)
                                duration = $"{years} an{(years > 1 ? "s" : "")} {months} mois";
                            else if (years > 0)
                                duration = $"{years} an{(years > 1 ? "s" : "")}";
                            else
                                duration = $"{Math.Max(1, months)} mois";
                        }
                        else
                        {
                            duration = "Durée non précisée";
                        }

                        return new WorkExperienceDto
                        {
                            Id = Guid.NewGuid(),
                            Role = e.JobTitle ?? "Rôle inconnu",
                            Company = e.Company ?? "Entreprise inconnue",
                            CompanyInitial = (e.Company?.Length > 0 ? e.Company[0].ToString() : "E").ToUpper(),
                            Period = period,
                            Duration = duration,
                            Description = e.Description ?? string.Empty
                        };
                    }).ToList() ?? new List<WorkExperienceDto>(),
                    Education = app.AIAnalysis?.ExtractedData?.Educations?.Select(e => new EducationDto
                    {
                        Id = Guid.NewGuid(),
                        Degree = e.Degree ?? "Diplôme",
                        School = e.Institution ?? "Institution",
                        Year = e.EndDate?.Year.ToString() ?? "Non précisé"
                    }).ToList() ?? new List<EducationDto>(),
                    SkillGroups = new List<SkillGroupDto>
                    {
                        new SkillGroupDto { Name = "Compétences identifiées", Skills = app.AIAnalysis?.IdentifiedSkills ?? new List<string>() }
                    },
                    Strengths = app.AIAnalysis?.Strengths ?? new List<string>(),
                    Weaknesses = app.AIAnalysis?.Weaknesses ?? new List<string>(),
                    ResumeUrl = app.CVUrl,
                    Stage = app.Status.ToString().ToLower(),
                    Comments = app.Comments?.OrderByDescending(c => c.CreatedAt).Select(c => new ApplicationCommentDto
                    {
                        Id = c.Id,
                        AuthorName = $"{c.Author?.FirstName} {c.Author?.LastName}",
                        Content = c.Content,
                        CreatedAt = c.CreatedAt.ToString("g"),
                        TimeAgo = GetTimeAgo(c.CreatedAt)
                    }).ToList() ?? new List<ApplicationCommentDto>(),
                    Timeline = timelineEvents,
                    RequiredSkills = app.JobOffer?.Skills ?? new List<string>(),
                    InterviewQuestions = app.AIAnalysis?.InterviewQuestions?.Select(q => new InterviewQuestionDto
                    {
                        Category = q.Category,
                        Question = q.Question,
                        Purpose = q.Purpose
                    }).ToList() ?? new List<InterviewQuestionDto>(),
                    AverageRecruiterRating = app.Ratings != null && app.Ratings.Any() ? app.Ratings.Average(r => r.Score) : (double?)null,
                    MyRating = app.Ratings?.FirstOrDefault(r => r.RecruiterId == GetUserId())?.Score,
                    QuizScore = app.QuizScore,
                    QuizSent = app.QuizSent,
                    QuizExpiresAt = app.QuizExpiresAt,
                    HasQuizGenerated = hasQuizGenerated
                };
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour l'étape (statut) d'avancement d'une candidature.
        /// </summary>
        /// <param name="id">L'identifiant de la candidature.</param>
        /// <param name="dto">La nouvelle étape demandée.</param>
        [HttpPatch("applications/{id}/stage")]
        public async Task<IActionResult> UpdateApplicationStage(Guid id, [FromBody] UpdateStageDto dto)
        {
            try
            {
                var companyId = GetCompanyId();

                // Simple validation of enum first (before hitting DB)
                if (!Enum.TryParse<ApplicationStatus>(dto.Stage, true, out var newStatus))
                {
                    return BadRequest(new { message = "Stage invalide" });
                }

                // Load ONLY the application (no AIAnalysis, no owned entities) to avoid EF tracking conflicts
                var app = await _db.JobApplications
                    .Include(a => a.JobOffer)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (app == null) return NotFound();
                if (app.JobOffer?.CompanyId != companyId) return Forbid();

                app.Status = newStatus;
                app.UpdatedAt = DateTime.UtcNow;

                // Sync interview statuses if needed
                var interviews = await _db.Interviews!
                    .Where(i => i.JobApplicationId == id)
                    .ToListAsync();
                InterviewApplicationSync.SyncInterviewsFromApplicationStatus(newStatus, interviews);

                await _db.SaveChangesAsync();

                return Ok(new { message = "Statut mis à jour" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateApplicationStage] Error for app {id}: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Déclenche l'analyse intelligente IA du CV du candidat.
        /// Extrait la formation, l'expérience et calcule le score de matching par rapport aux exigences de l'offre.
        /// </summary>
        /// <param name="id">Identifiant de la candidature.</param>
        [HttpPost("applications/{id}/analyze")]
        public async Task<IActionResult> AnalyzeApplication(Guid id)
        {
            try
            {
                // Use a fresh context for the analysis to avoid tracking conflicts in a long-running request
                using var scope = HttpContext.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // 1. Load the application without tracking to prevent conflicts with owned entities
                var app = await db.JobApplications
                    .Include(ja => ja.AIAnalysis)
                        .ThenInclude(ai => ai!.ExtractedData)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ja => ja.Id == id);

                if (app == null) return NotFound();

                if (string.IsNullOrWhiteSpace(app.CVUrl))
                {
                    return BadRequest(new { message = "Aucun CV n'est associé à cette candidature." });
                }

                // 2. Load the Job Offer (non-tracking) to get weights and verify company
                var offer = await db.JobOffers.AsNoTracking().FirstOrDefaultAsync(o => o.Id == app.JobOfferId);
                if (offer == null) return BadRequest(new { message = "Offre associée introuvable" });

                var companyId = GetCompanyId();
                if (offer.CompanyId != companyId) return Forbid();

                try
                {
                    var lang = Request.Headers["Accept-Language"].ToString().Split(',').FirstOrDefault()?.Split('-').FirstOrDefault()?.ToLower() ?? "fr";
                    if (!new[] { "fr", "en", "ar" }.Contains(lang)) lang = "fr";

                    Console.WriteLine($"[RecruiterController] Analyzing application {id} with AI service using URL: {app.CVUrl}");

                    var analysisResult = await _aiService.AnalyzeCVAsync(
                        app.CVUrl,
                        offer.Title,
                        offer.Description,
                        offer.WeightExperience,
                        offer.WeightEducation,
                        offer.WeightSkills,
                        lang
                    );

                    // 3. Update the application data
                    app.AiScore = analysisResult.OverallScore;
                    app.UpdatedAt = DateTime.UtcNow;

                    // Ensure AIAnalysis is initialized
                    if (app.AIAnalysis == null) 
                    {
                        app.AIAnalysis = new AIAnalysisResult();
                    }
                    
                    var analysis = app.AIAnalysis;
                    analysis.AnalyzedAt = DateTime.UtcNow;
                    analysis.OverallScore = analysisResult.OverallScore;
                    analysis.ExperienceScore = analysisResult.ExperienceScore;
                    analysis.EducationScore = analysisResult.EducationScore;
                    analysis.SkillsScore = analysisResult.SkillsScore;
                    analysis.TotalYearsExperience = analysisResult.TotalYearsExperience;
                    analysis.AutoGeneratedSummary = analysisResult.AutoGeneratedSummary;
                    analysis.AIRecommendation = analysisResult.AIRecommendation;

                    // Update Lists (re-initialize to avoid potential null issues)
                    analysis.IdentifiedSkills = analysisResult.IdentifiedSkills ?? new List<string>();
                    analysis.Strengths = analysisResult.Strengths ?? new List<string>();
                    analysis.Weaknesses = analysisResult.Weaknesses ?? new List<string>();

                    // Update Interview Questions
                    analysis.InterviewQuestions = analysisResult.InterviewQuestions?.Select(q => new InterviewQuestionRecord
                    {
                        Category = q.Category ?? "Général",
                        Question = q.Question ?? "",
                        Purpose = q.Purpose ?? ""
                    }).ToList() ?? new List<InterviewQuestionRecord>();

                    // Update Extracted Data
                    if (analysis.ExtractedData == null) 
                    {
                        analysis.ExtractedData = new ExtractedCVData();
                    }
                    
                    var extData = analysis.ExtractedData;
                    
                    if (analysisResult.ExtractedData != null)
                    {
                        // Map Work Experiences
                        extData.WorkExperiences = analysisResult.ExtractedData.WorkExperiences?
                            .Where(we => we != null)
                            .Select(we => new WorkExperience
                            {
                                JobTitle = we.JobTitle ?? "Poste inconnu",
                                Company = we.Company ?? "Entreprise inconnue",
                                Location = we.Location,
                                Description = we.Description,
                                IsCurrent = we.IsCurrent,
                                StartDate = TryParseDate(we.StartDate),
                                EndDate = TryParseDate(we.EndDate)
                            }).ToList() ?? new List<WorkExperience>();

                        // Map Educations
                        extData.Educations = analysisResult.ExtractedData.Educations?
                            .Where(ed => ed != null)
                            .Select(ed => new Education
                            {
                                Degree = ed.Degree ?? "Diplôme",
                                Institution = ed.Institution ?? "Institution",
                                FieldOfStudy = ed.FieldOfStudy,
                                GPA = ed.GPA,
                                StartDate = TryParseDate(ed.StartDate),
                                EndDate = TryParseDate(ed.EndDate)
                            }).ToList() ?? new List<Education>();
                        
                        extData.Skills = analysisResult.ExtractedData.Skills?.Where(s => s != null).ToList() ?? new List<string>();
                        extData.Languages = analysisResult.ExtractedData.Languages?.Where(l => l != null).ToList() ?? new List<string>();
                        extData.Certifications = analysisResult.ExtractedData.Certifications?.Where(c => c != null).ToList() ?? new List<string>();
                    }

                    // Auto-reject: if threshold is set and score is below it,
                    // apply rejection regardless of current status (Submitted or UnderReview),
                    // unless the application is already in a terminal/advanced state.
                    var isTerminalStatus = app.Status == ApplicationStatus.Accepted
                        || app.Status == ApplicationStatus.OfferSent
                        || app.Status == ApplicationStatus.Interviewed;

                    if (!isTerminalStatus)
                    {
                        if (offer.AutoRejectThreshold > 0 && analysisResult.OverallScore < offer.AutoRejectThreshold)
                        {
                            app.Status = ApplicationStatus.Rejected;
                        }
                        else if (app.Status == ApplicationStatus.Submitted)
                        {
                            // Only advance to UnderReview if still at initial Submitted stage
                            app.Status = ApplicationStatus.UnderReview;
                        }
                        // If already UnderReview/Shortlisted/Interview and score >= threshold: keep current status
                    }

                    // 4. Perform a surgical update
                    db.JobApplications.Update(app);
                    await db.SaveChangesAsync();

                    return Ok(new 
                    {
                        message = "Analyse terminée avec succès",
                        analysis = app.AIAnalysis,
                        score = app.AiScore,
                        status = (int)app.Status,
                        autoRejected = app.Status == ApplicationStatus.Rejected && offer.AutoRejectThreshold > 0 && analysisResult.OverallScore < offer.AutoRejectThreshold
                    });
                }
                catch (Exception aiEx)
                {
                    Console.WriteLine($"[RecruiterController] AI Analysis mapping failed for application {id}: {aiEx.Message}");
                    return BadRequest(new { message = $"Erreur lors du traitement des résultats de l'IA: {aiEx.Message}" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private DateTime? TryParseDate(string? dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return null;
            if (DateTime.TryParse(dateStr, out var date)) return date;
            
            // Handle "YYYY-MM" strings like "2022-03"
            var trimmed = dateStr.Trim();
            var ymMatch = System.Text.RegularExpressions.Regex.Match(trimmed, @"^(\d{4})-(\d{2})$");
            if (ymMatch.Success)
            {
                var y = int.Parse(ymMatch.Groups[1].Value);
                var m = int.Parse(ymMatch.Groups[2].Value);
                if (y >= 1900 && y <= 2100 && m >= 1 && m <= 12)
                    return new DateTime(y, m, 1);
            }
            // Handle year-only strings like "2020"
            if (int.TryParse(trimmed, out var year) && year >= 1900 && year <= 2100)
                return new DateTime(year, 1, 1);
            return null;
        }

        /// <summary>
        /// Met à jour les notes d'évaluation privées du recruteur sur un dossier candidat.
        /// </summary>
        /// <param name="id">Identifiant de la candidature.</param>
        /// <param name="dto">Les nouvelles notes.</param>
        [HttpPatch("applications/{id}/notes")]
        public async Task<IActionResult> UpdateRecruiterNotes(Guid id, [FromBody] UpdateNotesDto dto)
        {
            try
            {
                var app = await _jobApplicationRepository.GetByIdWithJobOfferAsync(id);
                if (app == null) return NotFound();

                var companyId = GetCompanyId();
                if (app.JobOffer?.CompanyId != companyId) return Forbid();

                app.RecruiterNotes = dto.Notes;
                app.UpdatedAt = DateTime.UtcNow;

                // Use change tracking instead of full entity Update() to preserve AI analysis
                await _jobApplicationRepository.SaveChangesAsync();

                return Ok(new { message = "Note mise à jour" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class UpdateStageDto { public string Stage { get; set; } = string.Empty; }

        public class UpdateNotesDto { public string Notes { get; set; } = string.Empty; }

        /// <summary>
        /// Permet à un recruteur de modifier manuellement les données extraites du CV par l'IA.
        /// Recalcule ensuite le score global de matching en tenant compte des corrections manuelles.
        /// </summary>
        /// <param name="id">Identifiant de la candidature.</param>
        /// <param name="dto">Les données corrigées (expériences, formation, compétences).</param>
        [HttpPatch("applications/{id}/extracted-data")]
        public async Task<IActionResult> UpdateExtractedData(Guid id, [FromBody] UpdateExtractedDataDto dto)
        {
            try
            {
                var app = await _jobApplicationRepository.GetByIdWithJobOfferAsync(id);
                if (app == null) return NotFound();

                var companyId = GetCompanyId();
                if (app.JobOffer?.CompanyId != companyId) return Forbid();

                // Update extracted data with manual corrections
                if (app.AIAnalysis == null) app.AIAnalysis = new AIAnalysisResult();
                if (app.AIAnalysis.ExtractedData == null) app.AIAnalysis.ExtractedData = new ExtractedCVData();

                app.AIAnalysis.ExtractedData.WorkExperiences = dto.Experiences.Select(e => new WorkExperience
                {
                    JobTitle = e.Role,
                    Company = e.Company,
                    StartDate = TryParseDate(e.StartDate),
                    EndDate = e.IsCurrent ? null : TryParseDate(e.EndDate),
                    IsCurrent = e.IsCurrent,
                    Description = e.Description
                }).ToList();

                app.AIAnalysis.ExtractedData.Educations = dto.Education.Select(e => new Education
                {
                    Degree = e.Degree,
                    Institution = e.School,
                    EndDate = TryParseDate(e.Year)
                }).ToList();

                app.AIAnalysis.ExtractedData.Skills = dto.Skills;
                app.AIAnalysis.IdentifiedSkills = dto.Skills;

                // Re-score using the AI service if CV file exists
                var offer = app.JobOffer;
                if (offer != null)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(app.CVUrl))
                        {
                            var userLang = Request.Headers["Accept-Language"].ToString().Split(',').FirstOrDefault()?.Split('-').FirstOrDefault()?.ToLower() ?? "fr";
                            if (!new[] { "fr", "en", "ar" }.Contains(userLang)) userLang = "fr";

                            var analysisResult = await _aiService.AnalyzeCVAsync(
                                app.CVUrl, offer.Title, offer.Description,
                                offer.WeightExperience, offer.WeightEducation, offer.WeightSkills,
                                userLang);

                            // Take scores from re-analysis
                            app.AIAnalysis.OverallScore = analysisResult.OverallScore;
                            app.AIAnalysis.ExperienceScore = analysisResult.ExperienceScore;
                            app.AIAnalysis.EducationScore = analysisResult.EducationScore;
                            app.AIAnalysis.SkillsScore = analysisResult.SkillsScore;
                            app.AIAnalysis.TotalYearsExperience = analysisResult.TotalYearsExperience;
                            app.AIAnalysis.AutoGeneratedSummary = analysisResult.AutoGeneratedSummary;
                            app.AIAnalysis.Strengths = analysisResult.Strengths;
                            app.AIAnalysis.Weaknesses = analysisResult.Weaknesses;
                            app.AIAnalysis.AIRecommendation = analysisResult.AIRecommendation;
                            app.AiScore = analysisResult.OverallScore;

                            // Keep the manually-edited extracted data (don't overwrite with re-analysis)
                            // Skills: merge AI-detected + manual
                            var mergedSkills = new HashSet<string>(dto.Skills, StringComparer.OrdinalIgnoreCase);
                            if (analysisResult.IdentifiedSkills != null)
                                foreach (var s in analysisResult.IdentifiedSkills) mergedSkills.Add(s);
                            app.AIAnalysis.IdentifiedSkills = mergedSkills.ToList();
                            app.AIAnalysis.ExtractedData.Skills = mergedSkills.ToList();
                        }
                    }
                    catch (Exception aiEx)
                    {
                        Console.WriteLine($"[RecruiterController] Re-scoring failed: {aiEx.Message}. Saving edits without score update.");
                    }
                }

                app.AIAnalysis.AnalyzedAt = DateTime.UtcNow;
                app.UpdatedAt = DateTime.UtcNow;

                await _jobApplicationRepository.UpdateAsync(app);
                await _jobApplicationRepository.SaveChangesAsync();

                // Return updated scores for immediate UI update
                return Ok(new
                {
                    message = "Données mises à jour avec succès",
                    overallScore = app.AIAnalysis.OverallScore,
                    experienceScore = app.AIAnalysis.ExperienceScore,
                    educationScore = app.AIAnalysis.EducationScore,
                    skillsScore = app.AIAnalysis.SkillsScore
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtient les statistiques avancées d'analyse de performance (matching, conversion, compétences clés).
        /// </summary>
        /// <param name="jobOfferId">Identifiant d'offre optionnel.</param>
        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics([FromQuery] Guid? jobOfferId)
        {
            try
            {
                var companyId = GetCompanyId();
                var applications = await _jobApplicationRepository.GetByCompanyIdAsync(companyId);
                var offers = await _jobOfferRepository.GetByCompanyIdAsync(companyId);

                // Filter by Job Offer if ID is provided
                if (jobOfferId.HasValue)
                {
                    applications = applications.Where(a => a.JobOfferId == jobOfferId.Value);
                }

                var totalApps = applications.Count();
                var analyzedApps = applications.Count(a => a.AiScore.HasValue);
                var avgScore = totalApps > 0 ? (int)applications.Average(a => a.AiScore ?? 0) : 0;

                // Advanced Metric: Conversion Rate (Shortlisted / Total)
                var shortlistedCount = applications.Count(a => a.Status == ApplicationStatus.Shortlisted || a.Status == ApplicationStatus.Interview || a.Status == ApplicationStatus.Accepted);
                var conversionRate = totalApps > 0 ? (int)((double)shortlistedCount / totalApps * 100) : 0;

                // Aggregate skills for TopSkills chart
                var topSkills = applications
                    .Where(a => a.AIAnalysis != null && a.AIAnalysis.IdentifiedSkills != null)
                    .SelectMany(a => a.AIAnalysis!.IdentifiedSkills)
                    .GroupBy(s => s)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToList();

                var result = new
                {
                    Kpis = new[]
                    {
                        new { Label = "Taux de Conversion", Value = $"{conversionRate}%", Trend = 5.2, Color = "#0ea5e9" },
                        new { Label = "Précision IA (Analyses)", Value = analyzedApps.ToString("N0"), Trend = 12.4, Color = "#10b981" },
                        new { Label = "Score de Matching Moyen", Value = $"{avgScore}%", Trend = -2.1, Color = "#f59e0b" },
                        new { Label = "Candidatures Cibles", Value = totalApps.ToString("N0"), Trend = 8.7, Color = "#8b5cf6" }
                    },
                    PipelineData = Enum.GetValues(typeof(ApplicationStatus)).Cast<ApplicationStatus>()
                        .OrderBy(s => (int)s)
                        .Select(s => new
                        {
                            Name = s.ToString(),
                            Count = applications.Count(a => a.Status == s)
                        }),
                    ScoreRanges = new[]
                    {
                        new { Label = "Profils Elite (85%+)", Class = "high", Count = applications.Count(a => a.AiScore >= 85) },
                        new { Label = "Profils Qualifiés (70-84%)", Class = "mid", Count = applications.Count(a => a.AiScore >= 70 && a.AiScore < 85) },
                        new { Label = "À réévaluer (<70%)", Class = "low", Count = applications.Count(a => a.AiScore < 70) }
                    },
                    TopOffers = offers.Select(o => new
                    {
                        Title = o.Title,
                        Count = applications.Count(a => a.JobOfferId == o.Id)
                    }).OrderByDescending(x => x.Count).Take(5),
                    TopSkills = topSkills
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RecruiterController] Error in GetAnalytics: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Génère et télécharge le rapport PDF global de performance d'une offre d'emploi.
        /// </summary>
        /// <param name="jobOfferId">ID de l'offre d'emploi concernée.</param>
        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportPdf([FromQuery] Guid jobOfferId)
        {
            try
            {
                var offer = await _jobOfferRepository.GetByIdAsync(jobOfferId);
                if (offer == null) return NotFound();

                var apps = await _jobApplicationRepository.GetByJobOfferIdAsync(jobOfferId);
                var pdfBytes = await _exportService.GenerateJobOfferReportPdfAsync(offer, apps);

                return File(pdfBytes, "application/pdf", $"Rapport_{offer.Title.Replace(" ", "_")}.pdf");
            }
            catch (Exception ex)
            {
                var errorMsg = $"PDF Export Exception: {ex.Message} | Stack: {ex.StackTrace}";
                Console.WriteLine(errorMsg);
                return StatusCode(500, errorMsg);
            }
        }

        /// <summary>
        /// Génère et télécharge le rapport Excel récapitulatif d'une offre d'emploi et de ses candidats.
        /// </summary>
        /// <param name="jobOfferId">ID de l'offre d'emploi concernée.</param>
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportExcel([FromQuery] Guid jobOfferId)
        {
            try
            {
                var offer = await _jobOfferRepository.GetByIdAsync(jobOfferId);
                if (offer == null) return NotFound();

                var apps = await _jobApplicationRepository.GetByJobOfferIdAsync(jobOfferId);
                var excelBytes = await _exportService.GenerateJobOfferReportExcelAsync(offer, apps);

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Rapport_{offer.Title.Replace(" ", "_")}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Génère et télécharge la fiche profil PDF détaillée d'un candidat spécifique.
        /// </summary>
        /// <param name="applicationId">ID unique de la candidature.</param>
        [HttpGet("applications/{applicationId}/export-pdf")]
        public async Task<IActionResult> ExportCandidatePdf(Guid applicationId)
        {
            try
            {
                var app = await _jobApplicationRepository.GetByIdWithDetailsAsync(applicationId);
                if (app == null) return NotFound("Candidature introuvable.");

                var companyId = GetCompanyId();
                if (app.JobOffer?.CompanyId != companyId) return Forbid("Accès refusé à cette candidature.");

                var pdfBytes = await _exportService.GenerateCandidateProfilePdfAsync(app);
                var fileName = $"Profil_{app.Candidate?.FirstName}_{app.Candidate?.LastName}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la génération du PDF: {ex.Message}");
            }
        }

        /// <summary>
        /// Récupère les données publiques de l'entreprise connectée.
        /// </summary>
        [HttpGet("company-info")]
        public async Task<IActionResult> GetCompanyInfo()
        {
            try
            {
                var companyId = GetCompanyId();
                var company = await _companyRepository.GetActiveCompanyByIdAsync(companyId);
                if (company == null) return NotFound();

                return Ok(company);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Ajoute un nouveau commentaire collaboratif interne sur le dossier d'un candidat.
        /// </summary>
        /// <param name="id">ID unique de la candidature.</param>
        /// <param name="request">Le texte du commentaire.</param>
        [HttpPost("applications/{id}/comments")]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] CommentRequest request)
        {
            try
            {
                var app = await _jobApplicationRepository.GetByIdWithJobOfferAsync(id);
                if (app == null) return NotFound();

                var companyId = GetCompanyId();
                if (app.JobOffer?.CompanyId != companyId) return Forbid();

                var userId = GetUserId();
                var comment = new JobApplicationComment
                {
                    JobApplicationId = id,
                    AuthorId = userId,
                    Content = request.Content,
                    CreatedAt = DateTime.UtcNow
                };

                // Add directly to DbSet to avoid NullReferenceException if app.Comments isn't loaded
                _db.JobApplicationComments!.Add(comment);
                await _db.SaveChangesAsync();

                return Ok(new ApplicationCommentDto
                {
                    Id = comment.Id,
                    AuthorName = "Moi", // Will be refreshed on reload
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt.ToString("g"),
                    TimeAgo = "À l'instant"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class CommentRequest
        {
            public string Content { get; set; } = string.Empty;
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.UtcNow - dateTime;
            if (span.TotalMinutes < 60) return $"il y a {(int)span.TotalMinutes} min";
            if (span.TotalHours < 24) return $"il y a {(int)span.TotalHours} h";
            return $"le {dateTime:dd/MM}";
        }
        /// <summary>
        /// Planifie un entretien avec un candidat (en visioconférence, en personne ou par téléphone).
        /// Envoie automatiquement un e-mail d'invitation avec calendrier ICS en pièce jointe.
        /// </summary>
        /// <param name="applicationId">ID de la candidature concernée.</param>
        /// <param name="dto">Les paramètres de l'entretien (date, heure, type, message personnalisé).</param>
        [HttpPost("applications/{applicationId}/interviews")]
        public async Task<IActionResult> ScheduleInterview(Guid applicationId, [FromBody] InterviewCreateDto dto)
        {
            var companyId = GetCompanyId();
            var application = await _db.JobApplications!
                .Include(a => a.JobOffer)
                .Include(a => a.Candidate)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobOffer!.CompanyId == companyId);

            if (application == null) return NotFound("Candidature non trouvée.");

            // Parsing robuste de la date (format ISO yyyy-MM-dd envoyé par le navigateur)
            _logger.LogInformation("ScheduleInterview reçu: Date='{Date}', Time='{Time}', Type='{Type}'", dto.Date, dto.Time, dto.Type);

            if (!DateTime.TryParseExact(dto.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDate))
            {
                _logger.LogWarning("Format de date invalide: '{Date}'", dto.Date);
                return BadRequest(new { message = $"Format de date invalide : '{dto.Date}'. Attendu : yyyy-MM-dd." });
            }

            if (!TimeSpan.TryParse(dto.Time, out var parsedTime))
            {
                _logger.LogWarning("Format d'heure invalide: '{Time}'", dto.Time);
                return BadRequest(new { message = $"Format d'heure invalide : '{dto.Time}'. Attendu : HH:mm." });
            }

            var scheduledAt = DateTime.SpecifyKind(parsedDate.Date.Add(parsedTime), DateTimeKind.Utc);
            _logger.LogInformation("scheduledAt calculé: {ScheduledAt}, UtcNow: {UtcNow}", scheduledAt, DateTime.UtcNow);

            // Si un entretien du même type est déjà planifié, on l'annule automatiquement
            var existingSameType = await _db.Interviews!
                .Where(i => i.JobApplicationId == applicationId && 
                           i.Type == dto.Type && 
                           (i.Status == InterviewStatus.Planned || i.Status == InterviewStatus.Rescheduled))
                .ToListAsync();

            foreach (var old in existingSameType)
            {
                old.Status = InterviewStatus.Cancelled;
            }

            var interview = new Interview
            {
                Id = Guid.NewGuid(),
                JobApplicationId = applicationId,
                RecruiterId = _currentUserService.UserId ?? Guid.Empty,
                ScheduledAt = scheduledAt,
                Type = dto.Type,
                LocationOrLink = dto.Type == "visio" 
                    ? "Lien envoyé par e-mail" 
                    : dto.Type == "phone" 
                        ? (application.Candidate?.PhoneNumber ?? "Téléphone du candidat") 
                        : "À définir",
                Message = dto.Message,
                Status = InterviewStatus.Planned,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Interviews!.AddAsync(interview);

            // Mise à jour de l'état de la candidature
            application.Status = ApplicationStatus.Interview;
            application.UpdatedAt = DateTime.UtcNow;

            // Création d'une notification
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                JobApplicationId = applicationId,
                Title = "Entretien Planifié",
                Message = $"Un entretien a été planifié pour le {dto.Date:dd/MM/yyyy} à {dto.Time}.",
                Type = "interview_scheduled",
                CreatedAt = DateTime.UtcNow
            };
            await _db.Notifications!.AddAsync(notification);

            await _db.SaveChangesAsync();

            // Envoi de l'e-mail
            var typeLabels = new Dictionary<string, string>
            {
                { "visio", "Visioconférence" },
                { "phone", "Appel Téléphonique" },
                { "onsite", "En Personne" }
            };
            var formatLabel = typeLabels.ContainsKey(dto.Type) ? typeLabels[dto.Type] : dto.Type;
            var messageHtml = System.Net.WebUtility.HtmlEncode(dto.Message).Replace("\n", "<br>");

            var emailBody = $@"
                <div style='font-family: ""Inter"", -apple-system, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f8fafc; padding: 40px 20px;'>
                    <div style='background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1); border: 1px solid #e2e8f0;'>
                        <div style='background: linear-gradient(135deg, #0ea5e9, #06b6d4); padding: 32px; text-align: center;'>
                            <h1 style='margin: 0; font-size: 24px; font-weight: 700; color: #ffffff;'>Invitation à un Entretien</h1>
                        </div>
                        <div style='padding: 40px;'>
                            <div style='background-color: #f0f9ff; border-left: 4px solid #0ea5e9; border-radius: 8px; padding: 20px; margin-bottom: 24px;'>
                                <p style='margin: 0 0 8px; font-size: 13px; font-weight: 700; color: #64748b; text-transform: uppercase; letter-spacing: 0.05em;'>Détails de l'entretien</p>
                                <p style='margin: 4px 0; font-size: 15px; color: #0f172a;'><strong>Date :</strong> {scheduledAt.ToString("dddd dd MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"))}</p>
                                <p style='margin: 4px 0; font-size: 15px; color: #0f172a;'><strong>Heure :</strong> {scheduledAt.ToString("HH:mm")}</p>
                                <p style='margin: 4px 0; font-size: 15px; color: #0f172a;'><strong>Format :</strong> {formatLabel}</p>
                            </div>
                            <div style='font-size: 15px; color: #334155; line-height: 1.7;'>
                                {messageHtml}
                            </div>
                        </div>
                        <div style='padding: 24px; background: rgba(0,0,0,0.02); text-align: center; border-top: 1px solid #e2e8f0;'>
                            <p style='margin: 0; font-size: 11px; color: #94a3b8; text-transform: uppercase; letter-spacing: 2px;'>NovaHire — Excellence en Recrutement AI</p>
                        </div>
                    </div>
                </div>";

            var emailSent = await _emailService.SendInterviewInvitationWithCalendarAsync(
                application.Candidate!.Email,
                application.Candidate!.FirstName,
                dto.Subject,
                emailBody,
                scheduledAt,
                60, // 1 hour duration
                dto.Type == "visio" ? "Lien Visioconférence" : "Bureaux de l'entreprise"
            );

            if (!emailSent)
            {
                _logger.LogWarning("L'envoi de l'invitation e-mail a échoué pour {CandidateEmail}, mais l'entretien a bien été enregistré en base.", application.Candidate!.Email);
                return Ok(new { message = "Entretien planifié avec succès (e-mail non envoyé — vérifiez la configuration SMTP).", emailSent = false });
            }

            return Ok(new { message = "Entretien planifié avec succès", emailSent = true });
        }

        /// <summary>
        /// Modifie les détails logistiques d'un entretien existant.
        /// </summary>
        /// <param name="id">ID unique de l'entretien.</param>
        /// <param name="dto">Les nouvelles informations.</param>
        [HttpPatch("interviews/{id}")]
        public async Task<IActionResult> UpdateInterview(Guid id, [FromBody] InterviewUpdateDto dto)
        {
            var companyId = GetCompanyId();
            var interview = await _db.Interviews!
                .Include(i => i.JobApplication)
                    .ThenInclude(a => a.JobOffer)
                .FirstOrDefaultAsync(i => i.Id == id && i.JobApplication!.JobOffer!.CompanyId == companyId);

            if (interview == null) return NotFound("Entretien non trouvé.");

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                if (!Enum.TryParse<InterviewStatus>(dto.Status, true, out var interviewStatus))
                    return BadRequest("Statut d'entretien invalide.");

                interview.Status = interviewStatus;
                if (interview.JobApplication != null)
                    InterviewApplicationSync.SyncApplicationFromInterviewStatus(
                        interview.JobApplication, interviewStatus);
            }

            if (dto.LocationOrLink != null)
                interview.LocationOrLink = dto.LocationOrLink;
            if (dto.Message != null)
                interview.Message = dto.Message;

            if (dto.Date.HasValue && !string.IsNullOrWhiteSpace(dto.Time))
            {
                if (!TimeSpan.TryParse(dto.Time.Trim(), out var timeOfDay))
                    return BadRequest("Format d'heure invalide (attendu HH:mm).");

                var scheduledAt = DateTime.SpecifyKind(
                    dto.Date.Value.Date.Add(timeOfDay),
                    DateTimeKind.Utc);

                if (scheduledAt < DateTime.UtcNow.AddMinutes(-5))
                    return BadRequest("Impossible de planifier un entretien dans le passé.");

                interview.ScheduledAt = scheduledAt;
                if (interview.Status is InterviewStatus.Planned or InterviewStatus.Rescheduled)
                    interview.Status = InterviewStatus.Rescheduled;
            }

            interview.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Entretien mis à jour avec succès",
                scheduledAt = interview.ScheduledAt
            });
        }
        /// <summary>
        /// Met à jour de façon interactive la liste des questions et notes du guide d'entretien du candidat.
        /// </summary>
        /// <param name="id">ID de la candidature.</param>
        /// <param name="questions">La liste des questions révisées.</param>
        [HttpPatch("applications/{id}/interview-guide")]
        public async Task<IActionResult> UpdateInterviewGuide(Guid id, [FromBody] List<InterviewQuestionDto> questions)
        {
            try
            {
                var app = await _jobApplicationRepository.GetByIdWithDetailsAsync(id);
                if (app == null) return NotFound();

                if (app.AIAnalysis == null) app.AIAnalysis = new AIAnalysisResult();

                // Clear and rebuild the questions list to preserve tracking
                app.AIAnalysis.InterviewQuestions.Clear();
                app.AIAnalysis.InterviewQuestions.AddRange(questions.Select(q => new InterviewQuestionRecord
                {
                    Category = q.Category,
                    Question = q.Question,
                    Purpose = q.Purpose,
                    Score = q.Score,
                    Notes = q.Notes
                }));

                await _jobApplicationRepository.UpdateAsync(app);
                await _jobApplicationRepository.SaveChangesAsync();

                return Ok(new { message = "Guide d'entretien mis à jour avec succès" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Permet à un recruteur de laisser ou mettre à jour une évaluation (note sur 5 et appréciation).
        /// </summary>
        /// <param name="id">ID de la candidature.</param>
        /// <param name="request">La note et le commentaire d'évaluation.</param>
        [HttpPost("applications/{id}/ratings")]
        public async Task<IActionResult> AddRating(Guid id, [FromBody] ApplicationRatingRequest request)
        {
            try
            {
                var app = await _jobApplicationRepository.GetByIdWithJobOfferAsync(id);
                if (app == null) return NotFound();

                var companyId = GetCompanyId();
                if (app.JobOffer?.CompanyId != companyId) return Forbid();

                var userId = GetUserId();
                
                // Vérifier si une note existe déjà pour ce recruteur
                var existingRating = await _db.ApplicationRatings!
                    .FirstOrDefaultAsync(r => r.JobApplicationId == id && r.RecruiterId == userId);

                if (existingRating != null)
                {
                    existingRating.Score = request.Score;
                    existingRating.Comment = request.Comment;
                    existingRating.UpdatedAt = DateTime.UtcNow;
                    _db.ApplicationRatings!.Update(existingRating);
                }
                else
                {
                    var rating = new ApplicationRating
                    {
                        Id = Guid.NewGuid(),
                        JobApplicationId = id,
                        RecruiterId = userId,
                        Score = request.Score,
                        Comment = request.Comment,
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.ApplicationRatings!.Add(rating);
                }

                await _db.SaveChangesAsync();
                return Ok(new { message = "Note enregistrée avec succès" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Compare les profils de deux candidats en confrontant leurs scores et compétences clés.
        /// </summary>
        /// <param name="ids">Les IDs uniques des deux candidatures séparés par une virgule.</param>
        [HttpGet("applications/compare")]
        public async Task<IActionResult> CompareCandidates([FromQuery] string ids)
        {
            try
            {
                if (string.IsNullOrEmpty(ids)) return BadRequest("Aucun ID fourni.");
                
                var guidIds = ids.Split(',')
                    .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                    .Where(g => g != Guid.Empty)
                    .ToList();

                if (guidIds.Count == 0) return BadRequest("IDs invalides.");
                if (guidIds.Count != 2) return BadRequest("La comparaison nécessite exactement 2 candidats.");

                var companyId = GetCompanyId();
                var apps = await _db.JobApplications!
                    .Include(a => a.Candidate)
                    .Include(a => a.JobOffer)
                    .Include(a => a.AIAnalysis)
                    .Where(a => guidIds.Contains(a.Id) && a.JobOffer!.CompanyId == companyId)
                    .ToListAsync();

                if (apps.Count != 2)
                    return NotFound(new { message = "Une ou plusieurs candidatures sont introuvables." });

                var withoutAnalysis = apps.Where(a =>
                    !a.AiScore.HasValue &&
                    (a.AIAnalysis == null ||
                     (a.AIAnalysis.OverallScore <= 0 &&
                      (a.AIAnalysis.IdentifiedSkills == null || a.AIAnalysis.IdentifiedSkills.Count == 0))))
                    .ToList();

                if (withoutAnalysis.Count > 0)
                {
                    var names = string.Join(", ", withoutAnalysis.Select(a =>
                        a.Candidate != null ? $"{a.Candidate.FirstName} {a.Candidate.LastName}" : "Candidat"));
                    return BadRequest(new
                    {
                        message = $"Analyse IA requise avant la comparaison. Lancez l'analyse CV pour : {names}."
                    });
                }

                var comparison = new ComparisonDto
                {
                    Candidates = apps.Select(a => new CandidateComparisonItemDto
                    {
                        Id = a.Id,
                        Name = a.Candidate != null ? $"{a.Candidate.FirstName} {a.Candidate.LastName}" : "Inconnu",
                        OverallScore = a.AIAnalysis?.OverallScore ?? a.AiScore ?? 0,
                        ExperienceScore = a.AIAnalysis?.ExperienceScore ?? 0,
                        EducationScore = a.AIAnalysis?.EducationScore ?? 0,
                        SkillsScore = a.AIAnalysis?.SkillsScore ?? 0,
                        TopSkills = a.AIAnalysis?.IdentifiedSkills?.Take(8).ToList() ?? new List<string>(),
                        Initials = ((a.Candidate?.FirstName?.Length > 0 ? a.Candidate.FirstName[0].ToString() : "C") + (a.Candidate?.LastName?.Length > 0 ? a.Candidate.LastName[0].ToString() : "A")).ToUpper()
                    }).ToList(),
                    CommonSkills = apps
                        .Where(a => a.AIAnalysis?.IdentifiedSkills != null)
                        .SelectMany(a => a.AIAnalysis!.IdentifiedSkills!)
                        .GroupBy(s => s)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key)
                        .Take(10)
                        .ToList()
                };

                return Ok(comparison);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Récupère le quiz d'évaluation associé à une offre d'emploi pour examen interne.
        /// </summary>
        /// <param name="id">ID de l'offre d'emploi.</param>
        [HttpGet("job-offers/{id}/quiz")]
        public async Task<IActionResult> GetQuizForOffer(Guid id)
        {
            try
            {
                var companyId = GetCompanyId();
                var quiz = await _db.Quizzes!
                    .Include(q => q.Questions)
                    .Include(q => q.JobOffer)
                    .FirstOrDefaultAsync(q => q.JobOfferId == id && q.JobOffer!.CompanyId == companyId);

                if (quiz == null) return NotFound(new { message = "Aucun quiz trouvé pour cette offre." });

                return Ok(new QuizDto
                {
                    Id = quiz.Id,
                    JobOfferId = quiz.JobOfferId,
                    Title = quiz.Title,
                    Description = quiz.Description,
                    TimeLimitMinutes = quiz.TimeLimitMinutes,
                    Questions = quiz.Questions.Select(q => new QuizQuestionDto
                    {
                        Id = q.Id,
                        Text = q.Text,
                        Type = q.Type,
                        Options = System.Text.Json.JsonSerializer.Deserialize<List<string>>(q.OptionsJson ?? "[]") ?? new List<string>(),
                        CorrectAnswerIndex = q.CorrectAnswerIndex,
                        Explanation = q.Explanation
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class GenerateJDDto
        {
            public string JobTitle { get; set; } = string.Empty;
            public string? Keywords { get; set; }
        }

        /// <summary>
        /// Fait appel à l'IA pour générer automatiquement une fiche descriptive de poste à partir de son titre et mots-clés.
        /// </summary>
        /// <param name="dto">Le titre du poste cible et mots-clés à incorporer.</param>
        [HttpPost("job-offers/ai/generate-description")]
        public async Task<IActionResult> GenerateJobDescription([FromBody] GenerateJDDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.JobTitle))
                    return BadRequest(new { message = "Le titre du poste est requis pour générer une description." });

                var description = await _aiService.GenerateJobDescriptionAsync(dto.JobTitle, dto.Keywords);
                return Ok(new { description = description });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Envoie massivement le quiz d'évaluation à tous les candidats présélectionnés de cette offre.
        /// </summary>
        /// <param name="id">ID de l'offre d'emploi concernée.</param>
        [HttpPost("job-offers/{id}/send-quiz-to-all")]
        public async Task<IActionResult> SendQuizToAll(Guid id)
        {
            try
            {
                var companyId = GetCompanyId();
                var applications = await _db.JobApplications!
                    .Include(a => a.Candidate)
                    .Include(a => a.JobOffer)
                    .Include(a => a.JobOffer!.Company)
                    .Where(a => a.JobOfferId == id && a.JobOffer!.CompanyId == companyId && a.Status == ApplicationStatus.Shortlisted)
                    .ToListAsync();

                if (!applications.Any())
                    return BadRequest(new { message = "Aucun candidat en étape de présélection trouvé pour cette offre." });

                int sentCount = 0;
                foreach (var app in applications)
                {
                    if (app.Candidate == null) continue;

                    var frontendUrl = _configuration["AppSettings:FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:3010";
                    var quizUrl = $"{frontendUrl}/public/quiz/{app.JobOffer?.ShareToken}?appId={app.Id}";
                    var companyName = app.JobOffer?.Company?.Name ?? "NovaHire";
                    var jobTitle = app.JobOffer?.Title ?? "le poste";
                    var candidateName = $"{app.Candidate.FirstName} {app.Candidate.LastName}";

                    bool sent = await _emailService.SendQuizInvitationAsync(
                        app.Candidate.Email,
                        candidateName,
                        jobTitle,
                        quizUrl,
                        companyName);
                    
                    if (sent) sentCount++;
                }

                return Ok(new { message = $"{sentCount} invitations envoyées avec succès !" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Supprime définitivement un quiz du système.
        /// </summary>
        /// <param name="id">L'identifiant du quiz.</param>
        [HttpDelete("quizzes/{id}")]
        public async Task<IActionResult> DeleteQuiz(Guid id)
        {
            try
            {
                var companyId = GetCompanyId();
                var quiz = await _db.Quizzes!
                    .Include(q => q.JobOffer)
                    .FirstOrDefaultAsync(q => q.Id == id && q.JobOffer!.CompanyId == companyId);

                if (quiz == null) return NotFound(new { message = "Quiz introuvable." });

                _db.Quizzes!.Remove(quiz);
                await _db.SaveChangesAsync();

                return Ok(new { message = "Quiz supprimé avec succès." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Envoie de façon unitaire l'invitation à passer le quiz technique de compétences à un candidat donné.
        /// </summary>
        /// <param name="id">ID de la candidature ciblée.</param>
        [HttpPost("applications/{id}/send-quiz-invitation")]
        public async Task<IActionResult> SendQuizInvitation(Guid id)
        {
            try
            {
                var companyId = GetCompanyId();
                var application = await _db.JobApplications!
                    .Include(a => a.Candidate)
                    .Include(a => a.JobOffer)
                    .Include(a => a.JobOffer!.Company)
                    .FirstOrDefaultAsync(a => a.Id == id && a.JobOffer!.CompanyId == companyId);

                if (application == null) return NotFound(new { message = "Candidature non trouvée." });
                if (application.Candidate == null) return BadRequest(new { message = "Données candidat manquantes." });

                var frontendUrl = _configuration["AppSettings:FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:3010";
                var quizUrl = $"{frontendUrl}/public/quiz/{application.JobOffer?.ShareToken}?appId={application.Id}";
                var companyName = application.JobOffer?.Company?.Name ?? "NovaHire";
                var jobTitle = application.JobOffer?.Title ?? "le poste";
                var candidateName = $"{application.Candidate.FirstName} {application.Candidate.LastName}";

                bool sent = await _emailService.SendQuizInvitationAsync(
                    application.Candidate.Email,
                    candidateName,
                    jobTitle,
                    quizUrl,
                    companyName);

                if (sent) return Ok(new { message = "Invitation envoyée avec succès !" });
                return BadRequest(new { message = "Erreur lors de l'envoi de l'email." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Envoie des invitations de quiz en lot à plusieurs candidatures sélectionnées.
        /// </summary>
        /// <param name="applicationIds">La liste des identifiants des candidatures concernées.</param>
        [HttpPost("applications/bulk-send-quiz")]
        public async Task<IActionResult> BulkSendQuiz([FromBody] List<Guid> applicationIds)
        {
            try
            {
                var companyId = GetCompanyId();
                var applications = await _db.JobApplications!
                    .Include(a => a.Candidate)
                    .Include(a => a.JobOffer)
                    .Include(a => a.JobOffer!.Company)
                    .Where(a => applicationIds.Contains(a.Id) && a.JobOffer!.CompanyId == companyId)
                    .ToListAsync();

                int sentCount = 0;
                foreach (var app in applications)
                {
                    if (app.Candidate == null) continue;

                    var frontendUrl = _configuration["AppSettings:FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:3010";
                    var quizUrl = $"{frontendUrl}/public/quiz/{app.JobOffer?.ShareToken}?appId={app.Id}";
                    var companyName = app.JobOffer?.Company?.Name ?? "NovaHire";
                    var jobTitle = app.JobOffer?.Title ?? "le poste";
                    var candidateName = $"{app.Candidate.FirstName} {app.Candidate.LastName}";

                    bool sent = await _emailService.SendQuizInvitationAsync(
                        app.Candidate.Email,
                        candidateName,
                        jobTitle,
                        quizUrl,
                        companyName);
                    
                    if (sent) 
                    {
                        sentCount++;
                        app.QuizSent = true;
                        app.QuizExpiresAt = DateTime.UtcNow.AddHours(48); // 48h deadline
                        app.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _db.SaveChangesAsync();

                return Ok(new { message = $"{sentCount} invitations envoyées avec succès !" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Envoie manuellement le lien public de passage de test technique à une adresse e-mail.
        /// </summary>
        /// <param name="request">L'adresse e-mail cible et l'offre associée.</param>
        [HttpPost("quiz/send-invitation")]
        public async Task<IActionResult> SendManualQuizInvitation([FromBody] QuizInvitationRequest request)
        {
            try
            {
                var companyId = GetCompanyId();
                var offer = await _db.JobOffers!
                    .Include(o => o.Company)
                    .FirstOrDefaultAsync(o => o.Id == request.JobOfferId && o.CompanyId == companyId);

                if (offer == null) return NotFound(new { message = "Offre non trouvée." });

                var frontendUrl = _configuration["AppSettings:FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:3010";
                var quizUrl = $"{frontendUrl}/public/quiz/{offer.ShareToken}";
                var companyName = offer.Company?.Name ?? "NovaHire";
                var jobTitle = offer.Title ?? "le poste";

                bool sent = await _emailService.SendQuizInvitationAsync(
                    request.Email,
                    "Candidat",
                    jobTitle,
                    quizUrl,
                    companyName);

                if (sent) return Ok(new { message = "Invitation envoyée avec succès !" });
                return BadRequest(new { message = "Erreur lors de l'envoi de l'email." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Envoie un e-mail de refus personnalisé ou rédigé de façon bienveillante par l'IA.
        /// </summary>
        /// <param name="id">ID de la candidature.</param>
        /// <param name="request">La raison de refus facultative et le drapeau d'utilisation de l'IA.</param>
        [HttpPost("applications/{id}/send-rejection-email")]
        public async Task<IActionResult> SendRejectionEmail(Guid id, [FromBody] RejectionRequest request)
        {
            try
            {
                var companyId = GetCompanyId();
                var app = await _db.JobApplications!
                    .Include(a => a.Candidate)
                    .Include(a => a.JobOffer)
                    .Include(a => a.JobOffer!.Company)
                    .FirstOrDefaultAsync(a => a.Id == id && a.JobOffer!.CompanyId == companyId);

                if (app == null || app.Candidate == null) return NotFound(new { message = "Candidature non trouvée." });

                var companyName = app.JobOffer?.Company?.Name ?? "NovaHire";
                var jobTitle = app.JobOffer?.Title ?? "le poste";
                var candidateName = $"{app.Candidate.FirstName} {app.Candidate.LastName}";

                string? finalReason = request.Reason;
                if (request.UseAI)
                {
                    finalReason = await _aiService.GenerateRejectionFeedbackAsync(candidateName, jobTitle, request.Reason ?? "");
                }

                bool sent = await _emailService.SendRejectionEmailAsync(
                    app.Candidate.Email,
                    candidateName,
                    jobTitle,
                    companyName,
                    finalReason);

                if (sent) return Ok(new { message = "Email de refus envoyé avec succès !" });
                return BadRequest(new { message = "Erreur lors de l'envoi de l'email." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Envoie par e-mail la lettre d'offre de proposition d'embauche finalisée au candidat.
        /// </summary>
        /// <param name="id">ID de la candidature.</param>
        /// <param name="request">Le contenu formatté HTML de la proposition.</param>
        [HttpPost("applications/{id}/send-offer-email")]
        public async Task<IActionResult> SendOfferEmail(Guid id, [FromBody] SendOfferEmailRequest request)
        {
            try
            {
                var companyId = GetCompanyId();
                var app = await _db.JobApplications!
                    .Include(a => a.Candidate)
                    .Include(a => a.JobOffer)
                    .Include(a => a.JobOffer!.Company)
                    .FirstOrDefaultAsync(a => a.Id == id && a.JobOffer!.CompanyId == companyId);

                if (app == null || app.Candidate == null) return NotFound(new { message = "Candidature non trouvée." });

                var companyName = app.JobOffer?.Company?.Name ?? "NovaHire";
                var jobTitle = app.JobOffer?.Title ?? "le poste";

                var subject = $"Proposition d'embauche : {jobTitle} chez {companyName}";
                
                // DO NOT Replace newlines with <br> anymore because the content is already HTML
                var body = request.Content;

                bool sent = await _emailService.SendEmailAsync(app.Candidate.Email, subject, body);

                if (sent) return Ok(new { message = "Offre envoyée par email avec succès !" });
                return BadRequest(new { message = "Erreur lors de l'envoi de l'email." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur interne", details = ex.Message });
            }
        }

        /// <summary>
        /// Envoie un e-mail au candidat pour l'informer que son dossier a été présélectionné pour la suite.
        /// </summary>
        /// <param name="id">ID de la candidature.</param>
        [HttpPost("applications/{id}/send-shortlisted-email")]
        public async Task<IActionResult> SendShortlistedEmail(Guid id)
        {
            try
            {
                var companyId = GetCompanyId();
                var app = await _db.JobApplications!
                    .Include(a => a.Candidate)
                    .Include(a => a.JobOffer)
                    .Include(a => a.JobOffer!.Company)
                    .FirstOrDefaultAsync(a => a.Id == id && a.JobOffer!.CompanyId == companyId);

                if (app == null || app.Candidate == null) return NotFound(new { message = "Candidature non trouvée." });

                var companyName = app.JobOffer?.Company?.Name ?? "NovaHire";
                var jobTitle = app.JobOffer?.Title ?? "le poste";
                var candidateName = $"{app.Candidate.FirstName} {app.Candidate.LastName}";

                bool sent = await _emailService.SendShortlistedEmailAsync(
                    app.Candidate.Email,
                    candidateName,
                    jobTitle,
                    companyName);

                if (sent) return Ok(new { message = "Email de présélection envoyé avec succès !" });
                return BadRequest(new { message = "Erreur lors de l'envoi de l'email." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Rédige automatiquement à l'aide de l'IA une proposition d'embauche premium personnalisée.
        /// </summary>
        /// <param name="id">ID de la candidature.</param>
        /// <param name="request">Les paramètres financiers et logistiques (salaire, avantages, date de début).</param>
        [HttpPost("applications/{id}/generate-offer-letter")]
        public async Task<IActionResult> GenerateOfferLetter(Guid id, [FromBody] OfferLetterRequest request)
        {
            try
            {
                var companyId = GetCompanyId();
                var app = await _db.JobApplications!
                    .Include(a => a.Candidate)
                    .Include(a => a.JobOffer)
                    .Include(a => a.JobOffer!.Company)
                    .FirstOrDefaultAsync(a => a.Id == id && a.JobOffer!.CompanyId == companyId);

                if (app == null || app.Candidate == null) return NotFound(new { message = "Candidature non trouvée." });

                var candidateName = $"{app.Candidate.FirstName} {app.Candidate.LastName}";
                var jobTitle = app.JobOffer?.Title ?? "le poste";
                var companyName = app.JobOffer?.Company?.Name ?? "NovaHire";
                
                // Format date to dd/MM/yyyy for the AI prompt
                string formattedStartDate = request.StartDate;
                if (DateTime.TryParse(request.StartDate, out DateTime parsedDate))
                {
                    formattedStartDate = parsedDate.ToString("dd/MM/yyyy");
                }
                
                // Get recruiter name from claims or default
                var recruiterName = User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "Le Recruteur";

                var offerLetter = await _aiService.GenerateOfferLetterAsync(
                    candidateName,
                    jobTitle,
                    request.Salary,
                    request.Currency ?? "DT",
                    formattedStartDate,
                    request.Benefits,
                    recruiterName,
                    companyName,
                    "fr"
                );

                if (string.IsNullOrEmpty(offerLetter))
                {
                    return StatusCode(503, new { message = "Impossible de générer l'offre. Le service IA (Python) est déconnecté ou injoignable." });
                }

                app.OfferLetterContent = offerLetter;
                app.Status = ApplicationStatus.OfferSent;
                await _db.SaveChangesAsync();

                return Ok(new { offerLetter = offerLetter });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur interne lors de la génération de l'offre.", details = ex.Message });
            }
        }
    }

    public class OfferLetterRequest
    {
        public decimal Salary { get; set; }
        public string? Currency { get; set; }
        public string StartDate { get; set; } = string.Empty;
        public string Benefits { get; set; } = string.Empty;
    }

    public class RejectionRequest
    {
        public string? Reason { get; set; }
        public bool UseAI { get; set; }
    }

    public class QuizInvitationRequest
    {
        public string Email { get; set; } = string.Empty;
        public Guid JobOfferId { get; set; }
    }

    public class SendOfferEmailRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}
