using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs.Recruiter;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RecruiterOrAbove")]
    public class JobOfferController : ControllerBase
    {
        private readonly IJobOfferRepository _jobOfferRepository;
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context;

        public JobOfferController(
            IJobOfferRepository jobOfferRepository,
            IUserRepository userRepository,
            ApplicationDbContext context)
        {
            _jobOfferRepository = jobOfferRepository;
            _userRepository = userRepository;
            _context = context;
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
                throw new UnauthorizedAccessException("NameIdentifier claim is missing or invalid.");
            }
            return userId;
        }

        private async Task LogActivity(string action, string entityType, string entityId, string? details = null)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
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

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? status = null)
        {
            try
            {
                var companyId = GetCompanyId();

                // Parse optional status
                JobOfferStatus? statusFilter = null;
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<JobOfferStatus>(status, true, out var parsedStatus))
                {
                    statusFilter = parsedStatus;
                }

                var (offers, totalCount) = await _jobOfferRepository.GetByCompanyIdPagedAsync(companyId, page, limit, statusFilter);

                var dtosList = offers.Select(o => new JobOfferDetailDto
                {
                    Id = o.Id,
                    Title = o.Title,
                    Description = o.Description,
                    Type = o.Type.ToString(),
                    Location = o.Location,
                    Department = o.Department,
                    RemotePolicy = o.RemotePolicy?.ToString() ?? "Hybrid",
                    ExperienceLevel = o.ExperienceLevel?.ToString() ?? "Intermediate",
                    Skills = o.Skills ?? new List<string>(),
                    Deadline = o.Deadline,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt,
                    SalaryConfidential = o.DisplayConfig?.ShowSalary == false,
                    SalaryRange = o.SalaryRange,
                    ShareToken = o.ShareToken,
                    ApplicationsCount = o.Applications?.Count ?? 0,
                    FormConfig = o.FormConfig != null ? new JobOfferFormConfigDto
                    {
                        RequireFullName = o.FormConfig.RequireFullName,
                        RequireEmail = o.FormConfig.RequireEmail,
                        RequirePhone = o.FormConfig.RequirePhone,
                        RequireCV = o.FormConfig.RequireCV,
                        RequireCoverLetter = o.FormConfig.RequireCoverLetter,
                        RequirePortfolio = o.FormConfig.RequirePortfolio,
                        RequireLinkedIn = o.FormConfig.RequireLinkedIn
                    } : null,
                    WeightExperience = o.WeightExperience,
                    WeightEducation = o.WeightEducation,
                    WeightSkills = o.WeightSkills
                }).ToList(); // Forcer l'exécution pour capturer les erreurs ici

                return Ok(new
                {
                    data = dtosList,
                    total = totalCount,
                    page = page,
                    limit = limit
                });
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message = message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var offer = await _jobOfferRepository.GetByIdAsync(id);
                if (offer == null) return NotFound();

                var companyId = GetCompanyId();
                if (offer.CompanyId != companyId) return Forbid();

                var dto = new JobOfferDetailDto
                {
                    Id = offer.Id,
                    Title = offer.Title,
                    Description = offer.Description,
                    Type = offer.Type.ToString(),
                    Location = offer.Location,
                    Department = offer.Department,
                    RemotePolicy = offer.RemotePolicy?.ToString() ?? "Hybrid",
                    ExperienceLevel = offer.ExperienceLevel?.ToString() ?? "Intermediate",
                    Skills = offer.Skills ?? new List<string>(),
                    Deadline = offer.Deadline,
                    Status = offer.Status.ToString(),
                    CreatedAt = offer.CreatedAt,
                    SalaryConfidential = offer.DisplayConfig?.ShowSalary == false,
                    SalaryRange = offer.SalaryRange,
                    ShareToken = offer.ShareToken,
                    ApplicationsCount = offer.Applications?.Count ?? 0,
                    FormConfig = offer.FormConfig != null ? new JobOfferFormConfigDto
                    {
                        RequireFullName = offer.FormConfig.RequireFullName,
                        RequireEmail = offer.FormConfig.RequireEmail,
                        RequirePhone = offer.FormConfig.RequirePhone,
                        RequireCV = offer.FormConfig.RequireCV,
                        RequireCoverLetter = offer.FormConfig.RequireCoverLetter,
                        RequirePortfolio = offer.FormConfig.RequirePortfolio,
                        RequireLinkedIn = offer.FormConfig.RequireLinkedIn
                    } : null,
                    WeightExperience = offer.WeightExperience,
                    WeightEducation = offer.WeightEducation,
                    WeightSkills = offer.WeightSkills
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JobOfferCreateDto dto)
        {
            try
            {
                var companyId = GetCompanyId();
                var userId = GetUserId();



                var offer = new JobOffer
                {
                    Id = Guid.NewGuid(),
                    Title = dto.Title,
                    Description = dto.Description,
                    CompanyId = companyId,
                    CreatedById = userId,
                    Location = dto.Location,
                    Type = Enum.TryParse<JobType>(dto.Type, true, out var type) ? type : JobType.FullTime,
                    SalaryRange = dto.SalaryMin.HasValue && dto.SalaryMax.HasValue
                        ? $"{dto.SalaryMin}-{dto.SalaryMax}"
                        : null,
                    Department = dto.Department,
                    RemotePolicy = Enum.TryParse<RemotePolicy>(dto.RemotePolicy, true, out var remote) ? remote : RemotePolicy.Hybrid,
                    ExperienceLevel = Enum.TryParse<ExperienceLevel>(dto.ExperienceLevel, true, out var exp) ? exp : ExperienceLevel.Intermediate,
                    Skills = dto.Skills,
                    Deadline = dto.Deadline.HasValue ? DateTime.SpecifyKind(dto.Deadline.Value, DateTimeKind.Utc) : null,
                    Status = (dto.Visibility?.ToLower() == "public" || dto.Visibility?.ToLower() == "published")
                        ? JobOfferStatus.Published
                        : JobOfferStatus.Draft,
                    ShareToken = Guid.NewGuid().ToString("n"),
                    CreatedAt = DateTime.UtcNow,
                    DisplayConfig = new PublicDisplayConfig
                    {
                        ShowSalary = !dto.SalaryConfidential,
                        ShowLocation = true
                    },
                    FormConfig = dto.FormConfig != null ? new ApplicationFormConfig
                    {
                        RequireFullName = dto.FormConfig.RequireFullName,
                        RequireEmail = dto.FormConfig.RequireEmail,
                        RequirePhone = dto.FormConfig.RequirePhone,
                        RequireCV = dto.FormConfig.RequireCV,
                        RequireCoverLetter = dto.FormConfig.RequireCoverLetter,
                        RequirePortfolio = dto.FormConfig.RequirePortfolio,
                        RequireLinkedIn = dto.FormConfig.RequireLinkedIn
                    } : new ApplicationFormConfig(),
                    WeightExperience = dto.WeightExperience,
                    WeightEducation = dto.WeightEducation,
                    WeightSkills = dto.WeightSkills
                };

                await _jobOfferRepository.AddAsync(offer);
                
                // Logging de l'activité
                await LogActivity("Publication d'offre", "JobOffer", offer.Id.ToString(), $"Titre: {offer.Title}");
                
                await _jobOfferRepository.SaveChangesAsync();

                return Ok(new
                {
                    id = offer.Id,
                    shareToken = offer.ShareToken,
                    message = "Offre créée avec succès"
                });
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message = message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] JobOfferCreateDto dto)
        {
            try
            {
                var offer = await _jobOfferRepository.GetByIdAsync(id);
                if (offer == null) return NotFound();

                var companyId = GetCompanyId();
                if (offer.CompanyId != companyId) return Forbid();

                offer.Title = dto.Title;
                offer.Description = dto.Description;
                offer.Location = dto.Location;
                offer.Department = dto.Department;
                offer.Type = Enum.TryParse<JobType>(dto.Type, true, out var type) ? type : JobType.FullTime;
                offer.RemotePolicy = Enum.TryParse<RemotePolicy>(dto.RemotePolicy, true, out var remote) ? remote : RemotePolicy.Hybrid;
                offer.ExperienceLevel = Enum.TryParse<ExperienceLevel>(dto.ExperienceLevel, true, out var exp) ? exp : ExperienceLevel.Intermediate;
                offer.Skills = dto.Skills;
                offer.Deadline = dto.Deadline.HasValue ? DateTime.SpecifyKind(dto.Deadline.Value, DateTimeKind.Utc) : null;
                offer.SalaryRange = dto.SalaryMin.HasValue && dto.SalaryMax.HasValue
                    ? $"{dto.SalaryMin}-{dto.SalaryMax}"
                    : offer.SalaryRange;

                if (offer.DisplayConfig == null) offer.DisplayConfig = new PublicDisplayConfig();
                offer.DisplayConfig.ShowSalary = !dto.SalaryConfidential;

                if (dto.FormConfig != null)
                {
                    if (offer.FormConfig == null) offer.FormConfig = new ApplicationFormConfig();
                    offer.FormConfig.RequireFullName = dto.FormConfig.RequireFullName;
                    offer.FormConfig.RequireEmail = dto.FormConfig.RequireEmail;
                    offer.FormConfig.RequirePhone = dto.FormConfig.RequirePhone;
                    offer.FormConfig.RequireCV = dto.FormConfig.RequireCV;
                    offer.FormConfig.RequireCoverLetter = dto.FormConfig.RequireCoverLetter;
                    offer.FormConfig.RequirePortfolio = dto.FormConfig.RequirePortfolio;
                    offer.FormConfig.RequireLinkedIn = dto.FormConfig.RequireLinkedIn;
                }

                offer.WeightExperience = dto.WeightExperience;
                offer.WeightEducation = dto.WeightEducation;
                offer.WeightSkills = dto.WeightSkills;

                offer.UpdatedAt = DateTime.UtcNow;

                // Sync status if visibility is provided
                if (!string.IsNullOrEmpty(dto.Visibility))
                {
                    offer.Status = (dto.Visibility.ToLower() == "public" || dto.Visibility.ToLower() == "published")
                        ? JobOfferStatus.Published
                        : JobOfferStatus.Draft;

                    if (offer.Status == JobOfferStatus.Published && !offer.PublishedAt.HasValue)
                    {
                        offer.PublishedAt = DateTime.UtcNow;
                    }
                }

                await _jobOfferRepository.UpdateAsync(offer);
                
                // Logging
                await LogActivity("Mise à jour d'offre", "JobOffer", offer.Id.ToString(), $"Offre: {offer.Title}");
                
                await _jobOfferRepository.SaveChangesAsync();

                return Ok(new { message = "Offre mise à jour avec succès" });
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message = message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateJobStatusDto dto)
        {
            try
            {
                var offer = await _jobOfferRepository.GetByIdAsync(id);
                if (offer == null) return NotFound();

                var companyId = GetCompanyId();
                if (offer.CompanyId != companyId) return Forbid();

                if (!Enum.TryParse<JobOfferStatus>(dto.Status, true, out var newStatus))
                {
                    return BadRequest(new { message = "Statut invalide" });
                }

                offer.Status = newStatus;
                offer.UpdatedAt = DateTime.UtcNow;
                if (newStatus == JobOfferStatus.Published && !offer.PublishedAt.HasValue)
                {
                    offer.PublishedAt = DateTime.UtcNow;
                }

                await _jobOfferRepository.UpdateAsync(offer);
                
                // Logging
                await LogActivity("Changement de statut", "JobOffer", offer.Id.ToString(), $"Statut: {dto.Status}");
                
                await _jobOfferRepository.SaveChangesAsync();

                return Ok(new { message = "Statut mis à jour avec succès" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class UpdateJobStatusDto { public string Status { get; set; } = string.Empty; }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var offer = await _jobOfferRepository.GetByIdAsync(id);
                if (offer == null) return NotFound();

                var companyId = GetCompanyId();
                if (offer.CompanyId != companyId) return Forbid();

                // Soft Delete: Mark as Archived
                offer.Status = JobOfferStatus.Archived;
                offer.UpdatedAt = DateTime.UtcNow;

                await _jobOfferRepository.UpdateAsync(offer);
                
                // Logging
                await LogActivity("Archivage d'offre", "JobOffer", offer.Id.ToString(), $"Offre: {offer.Title}");
                
                await _jobOfferRepository.SaveChangesAsync();

                return Ok(new { message = "Offre archivée avec succès" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
