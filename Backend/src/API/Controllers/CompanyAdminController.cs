#pragma warning disable CS8601, CS8602, CS8604
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs.CompanyAdmin;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    /// <summary>
    /// Contrôleur gérant toutes les opérations d'administration internes d'une entreprise (CompanyAdmin).
    /// Gère les statistiques du tableau de bord (KPIs, entonnoir de recrutement, graphiques mensuels),
    /// le branding de la marque (couleurs, logos), et la gestion de l'équipe (recruteurs, départements).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "CompanyAdminOrAbove")]
    public class CompanyAdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _storageService;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="CompanyAdminController"/>.
        /// </summary>
        /// <param name="context">Contexte Entity Framework de l'application.</param>
        /// <param name="storageService">Service de stockage cloud (Cloudinary).</param>
        public CompanyAdminController(ApplicationDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        /// <summary>
        /// Extrait l'identifiant de l'entreprise (CompanyId) à partir des claims du token JWT de l'utilisateur.
        /// </summary>
        /// <returns>L'ID de l'entreprise sous forme de Guid.</returns>
        /// <exception cref="UnauthorizedAccessException">Si le CompanyId est introuvable.</exception>
        private Guid GetCompanyId()
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim))
                throw new UnauthorizedAccessException("Company ID non trouvé dans les claims.");

            return Guid.Parse(companyIdClaim);
        }

        /// <summary>
        /// Journalise une action d'administration de l'entreprise dans le journal d'activité de l'audit.
        /// </summary>
        /// <param name="action">L'action réalisée (ex: "Mise à jour Branding").</param>
        /// <param name="entityType">Le type d'entité concernée (ex: "Branding", "User").</param>
        /// <param name="entityId">L'identifiant unique de l'entité concernée.</param>
        /// <param name="details">Informations textuelles optionnelles.</param>
        private async Task LogActivity(string action, string entityType, string entityId, string? details = null)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = !string.IsNullOrEmpty(userIdString) ? Guid.Parse(userIdString) : Guid.Empty;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name;
            var companyId = GetCompanyId();

            var log = new ActivityLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                UserEmail = userEmail,
                CompanyId = companyId,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            await _context.ActivityLogs!.AddAsync(log);
        }

        /// <summary>
        /// Calcule et récupère l'ensemble des statistiques de recrutement et de performance pour le tableau de bord de l'entreprise.
        /// Calcule les KPIs (candidatures, offres actives, interviews prévues), la répartition par canal (Sourcing),
        /// l'entonnoir de recrutement, le temps d'embauche et les statistiques individuelles de l'équipe.
        /// </summary>
        /// <returns>Un DTO <see cref="CompanyStatsDto"/> contenant les données statistiques compilées.</returns>
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try 
            {
                var companyId = GetCompanyId();
                var today = DateTime.UtcNow.Date;
                var sevenDaysAgo = today.AddDays(-7);

                // Récupération des informations de l'entreprise de session
                var company = await _context.Companies!
                    .FirstOrDefaultAsync(c => c.Id == companyId);

                if (company == null) return NotFound();

                // Requête de base sur les candidatures pour l'entreprise connectée
                var appsQuery = _context.JobApplications!.Where(a => a.JobOffer != null && a.JobOffer.CompanyId == companyId);

                // Indicateurs clés de performance (KPIs) des candidatures
                var totalApps = await appsQuery.CountAsync();
                var appsToday = await appsQuery.CountAsync(a => a.AppliedAt >= today);
                var appsThisWeek = await appsQuery.CountAsync(a => a.AppliedAt >= sevenDaysAgo);

                var activeOffers = await _context.JobOffers!
                    .CountAsync(o => o.CompanyId == companyId && o.Status == JobOfferStatus.Published);

                var aiAppsQuery = appsQuery.Where(a => a.AiScore.HasValue);
                var aiAnalysesCount = await aiAppsQuery.CountAsync();
                var avgAiScore = aiAnalysesCount > 0 ? await aiAppsQuery.AverageAsync(a => (double)(a.AiScore ?? 0)) : 0;

                var plannedInterviews = await _context.Interviews!
                    .CountAsync(i => i.JobApplication != null && i.JobApplication.JobOffer != null && i.JobApplication.JobOffer.CompanyId == companyId && i.ScheduledAt > DateTime.UtcNow);

                // Construction de l'historique des candidatures des 6 derniers mois
                var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
                sixMonthsAgo = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

                var monthlyData = await appsQuery
                    .Where(a => a.AppliedAt >= sixMonthsAgo)
                    .GroupBy(a => new { a.AppliedAt.Year, a.AppliedAt.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                    .ToListAsync();

                var monthlyApplications = new List<int>();
                for (int i = 0; i < 6; i++)
                {
                    var targetMonth = sixMonthsAgo.AddMonths(i);
                    var count = monthlyData.FirstOrDefault(m => m.Year == targetMonth.Year && m.Month == targetMonth.Month)?.Count ?? 0;
                    monthlyApplications.Add(count);
                }

                // Données de l'entonnoir (funnel) de recrutement
                var funnelData = new GlobalFunnelDto
                {
                    Sourcing = (int)totalApps,
                    Screening = await appsQuery.CountAsync(a => a.Status >= ApplicationStatus.UnderReview),
                    Interview = await appsQuery.CountAsync(a => a.Status >= ApplicationStatus.Interview),
                    Offer = await appsQuery.CountAsync(a => a.Status == ApplicationStatus.Accepted)
                };



                // Mesures d'efficacité du recrutement (Time-to-Hire)
                var acceptedAppsData = await appsQuery
                    .Where(a => a.Status == ApplicationStatus.Accepted && a.ReviewedAt.HasValue)
                    .Select(a => new { a.AppliedAt, a.ReviewedAt })
                    .ToListAsync();

                double avgTimeToHire = acceptedAppsData.Any() 
                    ? acceptedAppsData.Average(a => (a.ReviewedAt!.Value - a.AppliedAt).TotalDays) 
                    : 12.5;
                
                double aiEfficiency = totalApps > 0 ? (double)aiAnalysesCount / totalApps * 100 : 0;

                // Statistiques globales de performance de l'équipe de recrutement
                var rawTeam = await _context.Users!
                    .Where(u => u.CompanyId == companyId)
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        u.Role,
                        OffersCount = _context.JobOffers!.Count(o => o.CreatedById == u.Id),
                        InterviewsCount = _context.Interviews!.Count(i => i.RecruiterId == u.Id)
                    })
                    .ToListAsync();

                var teamStats = rawTeam.Select(u => new TeamMemberStatsDto
                {
                    Id = u.Id,
                    Name = $"{u.FirstName} {u.LastName}".Trim(),
                    Initials = ((u.FirstName?.Length > 0 ? u.FirstName[0].ToString() : "N") + (u.LastName?.Length > 0 ? u.LastName[0].ToString() : "H")).ToUpper(),
                    Role = u.Role.ToString(),
                    OffersCount = u.OffersCount,
                    InterviewsCount = u.InterviewsCount
                }).ToList();

                // Statistiques cumulées par département
                var rawDeptStats = await _context.JobOffers!
                    .Where(o => o.CompanyId == companyId)
                    .Select(o => new { o.Department, Id = o.Id })
                    .ToListAsync();

                var departmentIds = rawDeptStats.Select(o => o.Id).ToList();
                var appCountsByOffer = await _context.JobApplications!
                    .Where(a => departmentIds.Contains(a.JobOfferId))
                    .GroupBy(a => a.JobOfferId)
                    .Select(g => new { OfferId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var departmentStats = rawDeptStats
                    .GroupBy(x => x.Department ?? "Non Classé")
                    .Select(g => {
                        var offerIds = g.Select(x => x.Id).ToList();
                        return new DepartmentStatDto
                        {
                            Name = g.Key,
                            OffersCount = g.Count(),
                            ApplicationsCount = appCountsByOffer.Where(x => offerIds.Contains(x.OfferId)).Sum(x => x.Count)
                        };
                    })
                    .ToList();

                // Récupération des dernières offres ajoutées
                var recentOffers = await _context.JobOffers!
                    .Where(o => o.CompanyId == companyId)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .Select(o => new RecentOfferDto
                    {
                        Id = o.Id,
                        Title = o.Title,
                        Location = o.Location,
                        ApplicationsCount = _context.JobApplications!.Count(a => a.JobOfferId == o.Id)
                    })
                    .ToListAsync();

                // Historique des activités récentes de l'entreprise
                var allowedEntities = new[] { "JobOffer", "User", "Department", "Configuration", "Branding" };
                var rawActivities = await _context.ActivityLogs!
                    .Where(l => l.CompanyId == companyId && allowedEntities.Contains(l.EntityType))
                    .OrderByDescending(l => l.Timestamp)
                    .Take(10)
                    .Select(l => new
                    {
                        l.Action,
                        l.Details,
                        l.EntityType,
                        l.UserEmail,
                        l.Timestamp
                    })
                    .ToListAsync();

                var recentActivities = rawActivities.Select(a => new RecentActivityDto
                {
                    Title = string.IsNullOrEmpty(a.Details) ? a.Action : $"{a.Action} : {(a.Details.Length > 30 ? a.Details.Substring(0, 27) + "..." : a.Details)}",
                    Type = a.EntityType ?? "System",
                    TimeAgo = GetTimeAgo(a.Timestamp)
                }).ToList();

                var stats = new CompanyStatsDto
                {
                    ActiveJobOffers = activeOffers,
                    TotalApplications = totalApps,
                    ApplicationsToday = appsToday,
                    ApplicationsThisWeek = appsThisWeek,
                    AiAnalysesCount = aiAnalysesCount,
                    AvgAiScore = Math.Round(avgAiScore, 1),
                    PlannedInterviews = plannedInterviews,
                    MonthlyApplications = monthlyApplications,
                    TeamStats = teamStats,
                    RecentOffers = recentOffers,
                    DepartmentStats = departmentStats,
                    FunnelData = funnelData,
                    AverageTimeToHire = Math.Round(avgTimeToHire, 1),
                    AiEfficiencyRate = Math.Round(aiEfficiency, 1),
                    RecentActivities = recentActivities
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL ERROR] GetDashboardStats: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, new { message = "Erreur interne du serveur lors du calcul des statistiques.", error = ex.Message });
            }
        }

        /// <summary>
        /// Traduit le statut interne de la candidature en français lisible.
        /// </summary>
        /// <param name="status">Statut interne de la candidature.</param>
        /// <returns>Chaîne traduite en français.</returns>
        private static string TranslateStatus(ApplicationStatus status)
        {
            return status switch
            {
                ApplicationStatus.Submitted => "Candidature reçue",
                ApplicationStatus.UnderReview => "Examen en cours",
                ApplicationStatus.Shortlisted => "Présélectionné",
                ApplicationStatus.Interview => "Entretien prévu",
                ApplicationStatus.Interviewed => "Entretien réalisé",
                ApplicationStatus.Rejected => "Non retenu",
                ApplicationStatus.Accepted => "Engagé",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// Calcule un affichage temporel relatif ("il y a 5 min") à partir d'un horodatage.
        /// </summary>
        /// <param name="dateTime">Horodatage de référence.</param>
        /// <returns>Chaîne relative du temps écoulé.</returns>
        private static string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.UtcNow - dateTime;
            if (span.TotalMinutes < 1) return "À l'instant";
            if (span.TotalMinutes < 60) return $"Il y a {(int)span.TotalMinutes} min";
            if (span.TotalHours < 24) return $"Il y a {(int)span.TotalHours} h";
            return $"Il y a {(int)span.TotalDays} j";
        }

        /// <summary>
        /// Récupère la charte graphique et l'identité (Branding) de l'entreprise connectée.
        /// </summary>
        /// <returns>Les informations de branding et de contact de l'entreprise.</returns>
        [HttpGet("branding")]
        public async Task<IActionResult> GetBranding()
        {
            var companyId = GetCompanyId();
            var company = await _context.Companies!
                .Where(c => c.Id == companyId)
                .Select(c => new CompanyBrandingDto
                {
                    CompanyName = c.Name,
                    LogoUrl = c.LogoUrl,
                    PrimaryColor = c.PrimaryColor,
                    SecondaryColor = c.SecondaryColor,
                    Description = c.Description,
                    Industry = c.Industry,
                    Website = c.Website,
                    Country = c.Country,
                    ContactPhone = c.ContactPhone
                })
                .FirstOrDefaultAsync();

            if (company == null) return NotFound();
            return Ok(company);
        }

        /// <summary>
        /// Met à jour la charte graphique (couleurs, logo) et les informations d'identité publique de l'entreprise.
        /// </summary>
        /// <param name="request">Les nouveaux éléments du branding.</param>
        [HttpPut("branding")]
        public async Task<IActionResult> UpdateBranding([FromBody] UpdateBrandingRequest request)
        {
            var companyId = GetCompanyId();
            var company = await _context.Companies!.FindAsync(companyId);

            if (company == null) return NotFound();

            if (!string.IsNullOrEmpty(request.PrimaryColor)) company.PrimaryColor = request.PrimaryColor;
            if (!string.IsNullOrEmpty(request.SecondaryColor)) company.SecondaryColor = request.SecondaryColor;
            if (request.LogoUrl != null) company.LogoUrl = request.LogoUrl;
            if (request.CompanyName != null) company.Name = request.CompanyName;
            if (request.Description != null) company.Description = request.Description;
            if (request.Industry != null) company.Industry = request.Industry;
            if (request.Website != null) company.Website = request.Website;
            if (request.Country != null) company.Country = request.Country;
            if (request.ContactPhone != null) company.ContactPhone = request.ContactPhone;

            company.UpdatedAt = DateTime.UtcNow;
            
            // Journalisation de l'activité
            await LogActivity("Mise à jour Branding", "Branding", companyId.ToString(), $"Société: {company.Name}");
            
            await _context.SaveChangesAsync();

            return Ok(new { message = "Branding mis à jour avec succès" });
        }

        /// <summary>
        /// Téléverse le logo de l'entreprise sur Cloudinary et met à jour l'URL du logo en base de données.
        /// </summary>
        /// <param name="file">Le fichier image du logo (jpg, jpeg, png, webp — max 2 Mo).</param>
        /// <returns>L'URL Cloudinary sécurisée du nouveau logo.</returns>
        [HttpPost("branding/logo")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Aucun fichier sélectionné." });

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Format non supporté. Utilisez PNG, JPG ou WebP." });

                if (file.Length > 2 * 1024 * 1024)
                    return BadRequest(new { message = "Fichier trop volumineux. Maximum 2 Mo." });

                var companyId = GetCompanyId();
                var company = await _context.Companies!.FindAsync(companyId);
                if (company == null) return NotFound();

                // Supprimer l'ancien logo Cloudinary s'il existe
                if (!string.IsNullOrEmpty(company.LogoUrl) && company.LogoUrl.Contains("cloudinary.com"))
                {
                    await _storageService.DeleteFileAsync(company.LogoUrl);
                }

                // Dossier de destination sur Cloudinary
                var companyName = (company.Name ?? "General").Replace(" ", "_");
                var folderPath = $"novahire/companies/{companyName}/logos";

                string logoUrl;
                using (var stream = file.OpenReadStream())
                {
                    logoUrl = await _storageService.UploadFileAsync(stream, file.FileName, folderPath);
                }

                company.LogoUrl = logoUrl;
                company.UpdatedAt = DateTime.UtcNow;

                await LogActivity("Upload Logo", "Branding", companyId.ToString(), $"Société: {company.Name}");
                await _context.SaveChangesAsync();

                return Ok(new { logoUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Récupère la liste de tous les membres de l'équipe (recruteurs et administrateurs) associés à l'entreprise.
        /// </summary>
        /// <returns>Une liste de <see cref="TeamMemberDto"/>.</returns>
        [HttpGet("team")]
        public async Task<IActionResult> GetTeam()
        {
            var companyId = GetCompanyId();
            var team = await _context.Users!
                .Where(u => u.CompanyId == companyId)
                .OrderBy(u => u.LastName)
                .Select(u => new TeamMemberDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    JobTitle = u.JobTitle,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    DepartmentId = u.DepartmentId,
                    AvatarUrl = u.AvatarUrl,
                    OffersCount = _context.JobOffers!.Count(o => o.CreatedById == u.Id),
                    InterviewsCount = _context.Interviews!.Count(i => i.RecruiterId == u.Id)
                })
                .ToListAsync();

            return Ok(team);
        }

        /// <summary>
        /// Supprime un membre de l'équipe de l'entreprise.
        /// Empêche un administrateur de supprimer son propre compte de session.
        /// </summary>
        /// <param name="id">L'identifiant du membre à supprimer.</param>
        [HttpDelete("team/{id}")]
        public async Task<IActionResult> RemoveMember(Guid id)
        {
            var companyId = GetCompanyId();
            var user = await _context.Users!.FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == companyId);

            if (user == null) return NotFound();

            // Règle de sécurité : Un utilisateur ne peut pas s'auto-supprimer de l'équipe
            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            if (user.Id == currentUserId) return BadRequest("Vous ne pouvez pas supprimer votre propre compte.");

            _context.Users.Remove(user);
            
            // Journalisation de l'activité
            await LogActivity("Suppression membre", "User", user.Id.ToString(), $"Email: {user.Email}");
            
            await _context.SaveChangesAsync();

            return Ok(new { message = "Membre supprimé" });
        }

        /// <summary>
        /// Met à jour les informations et assignations (département, état actif/inactif) d'un membre de l'équipe.
        /// </summary>
        /// <param name="id">L'identifiant unique du membre à modifier.</param>
        /// <param name="request">Les informations mises à jour.</param>
        [HttpPatch("team/{id}")]
        public async Task<IActionResult> UpdateMemberDetails(Guid id, [FromBody] UpdateMemberDetailsRequest request)
        {
            var companyId = GetCompanyId();
            var user = await _context.Users!.FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == companyId);

            if (user == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(request.FirstName)) user.FirstName = request.FirstName;
            if (!string.IsNullOrWhiteSpace(request.LastName)) user.LastName = request.LastName;
            if (!string.IsNullOrWhiteSpace(request.Email)) user.Email = request.Email;
            if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;
            if (request.DepartmentId.HasValue)
                user.DepartmentId = request.DepartmentId.Value == Guid.Empty ? null : request.DepartmentId.Value;

            user.UpdatedAt = DateTime.UtcNow;
            
            // Journalisation de la mise à jour
            await LogActivity("Mise à jour membre", "User", user.Id.ToString(), $"Email: {user.Email}");
            
            await _context.SaveChangesAsync();

            return Ok(new { message = "Membre mis à jour avec succès" });
        }
    }
}
