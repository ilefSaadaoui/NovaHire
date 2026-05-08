#pragma warning disable CS8602, CS8604
using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using System.Security.Claims;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "SuperAdminOnly")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IConfiguration _configuration;

        public AdminController(ApplicationDbContext context, ICurrentUserService currentUserService, IConfiguration configuration)
        {
            _context = context;
            _currentUserService = currentUserService;
            _configuration = configuration;
        }

        [HttpGet("health")]
        public async Task<IActionResult> GetSystemHealth()
        {
            var health = new
            {
                api = new { status = "Stable", uptime = "100%", message = "Online" },
                database = new { status = "Down", message = "Disconnected" },
                storage = new { status = "Stable", message = "Connected" },
                ai = new { status = "Down", message = "Offline" }
            };

            try
            {
                // Check DB
                bool canConnect = await _context.Database.CanConnectAsync();
                health = health with { database = new { status = canConnect ? "Stable" : "Down", message = canConnect ? "Optimale" : "Error" } };

                // Check AI Service
                var aiUrl = _configuration["AISettings:PythonServiceUrl"] ?? _configuration["AIService:Url"] ?? "http://localhost:8000";
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(2);
                    try 
                    {
                        var response = await client.GetAsync(aiUrl);
                        bool isUp = response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound; // Even 404 means it's up
                        health = health with { ai = new { status = isUp ? "Stable" : "Down", message = isUp ? "Actif" : "Offline" } };
                    }
                    catch { /* Service down */ }
                }

                // Check Storage (Cloudinary)
                var cloudName = _configuration["CloudinarySettings:CloudName"];
                if (!string.IsNullOrEmpty(cloudName))
                {
                    // Simple ping to Cloudinary API base
                    using (var client = new System.Net.Http.HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(2);
                        try
                        {
                            var response = await client.GetAsync($"https://res.cloudinary.com/{cloudName}/image/upload/sample.jpg");
                            bool isUp = response.IsSuccessStatusCode;
                            health = health with { storage = new { status = isUp ? "Stable" : "Down", message = isUp ? "98% Libre" : "Restricted" } };
                        }
                        catch { /* Cloudinary unreachable */ }
                    }
                }

                return Ok(health);
            }
            catch (Exception ex)
            {
                return Ok(health); // Return partial health if crash
            }
        }

        // Get all tables data
        [HttpGet("tables")]
        public async Task<IActionResult> GetAllTablesData()
        {
            var data = new
            {
                users = await _context.Users.ToListAsync(),
                companies = await _context.Companies.ToListAsync(),
                jobOffers = await _context.JobOffers.ToListAsync(),
                jobApplications = await _context.JobApplications.Include(ja => ja.Candidate).ToListAsync()
            };
            return Ok(data);
        }

        // Get table summary (counts)
        [HttpGet("summary")]
        public async Task<IActionResult> GetTablesSummary()
        {
            try
            {
                if (_context.Users == null || _context.Candidates == null || _context.Companies == null)
                {
                    return StatusCode(500, "Database context is not properly initialized.");
                }

                var totalUsers = await _context.Users.CountAsync();
                var totalCandidates = await _context.Candidates.CountAsync();
                var totalCompanies = await _context.Companies.CountAsync();
                var totalJobOffers = _context.JobOffers != null ? await _context.JobOffers.CountAsync() : 0;
                var totalJobApplications = _context.JobApplications != null ? await _context.JobApplications.CountAsync() : 0;

                // Counts for active entities
                var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
                var activeCompanies = await _context.Companies.CountAsync(c => c.IsActive);

                // Role distribution (Users + Candidates)
                // Materialize first to avoid translation issues with ToString() on enum in some EF providers
                var rolesGrouped = await _context.Users
                    .GroupBy(u => u.Role)
                    .Select(g => new { Role = g.Key, Count = g.Count() })
                    .ToListAsync();

                var userRoles = rolesGrouped.ToDictionary(x => x.Role.ToString(), x => x.Count);

                var roleDistribution = new Dictionary<string, int>();
                foreach (var role in Enum.GetValues(typeof(UserRole)))
                {
                    var roleName = role.ToString()!;
                    roleDistribution[roleName] = userRoles.ContainsKey(roleName) ? userRoles[roleName] : 0;
                }
                roleDistribution["Candidate"] = totalCandidates;

                var summary = new
                {
                    totalUsers,
                    totalCandidates,
                    totalCompanies,
                    totalJobOffers,
                    totalJobApplications,
                    totalCandidatesCount = totalCandidates, // Alias for backward compatibility if needed
                    activeUsers,
                    activeCompanies,
                    roleDistribution
                };
                return Ok(summary);
            }
            catch (Exception ex)
            {
                // Simple console log for local dev
                Console.WriteLine($"[AdminController] Error in GetTablesSummary: {ex.Message}");
                // Return a 500 but with a message that the store can handle
                return StatusCode(500, new { message = "Erreur lors de la recuperation du resume admin.", detail = ex.Message });
            }
        }

        // ==================== USERS ====================

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, [FromQuery] UserRole? role = null, [FromQuery] Guid? companyId = null, [FromQuery] string? search = null)
        {
            var query = _context.Users!.AsNoTracking().AsQueryable();

            if (role.HasValue) query = query.Where(u => u.Role == role.Value);
            if (companyId.HasValue) query = query.Where(u => u.CompanyId == companyId.Value);
            if (!string.IsNullOrEmpty(search)) query = query.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search));

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var total = await _context.Users.CountAsync();
            return Ok(new { data = users, total, pageNumber, pageSize });
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password ?? "DefaultPassword123!"),
                Role = dto.Role,
                CompanyId = dto.CompanyId,
                IsActive = dto.IsActive ?? true,
                EmailConfirmed = false
            };

            _context.Users.Add(user);
            await LogActivity("CREATE", "User", user.Id.ToString(), $"Création utilisateur: {user.Email}");
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.FirstName = dto.FirstName ?? user.FirstName;
            user.LastName = dto.LastName ?? user.LastName;
            user.Email = dto.Email ?? user.Email;
            user.Role = dto.Role ?? user.Role;
            user.CompanyId = dto.CompanyId ?? user.CompanyId;
            user.IsActive = dto.IsActive ?? user.IsActive;

            _context.Users.Update(user);
            await LogActivity("UPDATE", "User", user.Id.ToString(), $"Mise à jour utilisateur: {user.Email}");
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await LogActivity("DELETE", "User", user.Id.ToString(), $"Suppression utilisateur: {user.Email}");
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==================== COMPANIES ====================

        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var companies = await _context.Companies
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var total = await _context.Companies.CountAsync();
            return Ok(new { data = companies, total, pageNumber, pageSize });
        }

        [HttpGet("companies/{id}")]
        public async Task<IActionResult> GetCompanyById(Guid id)
        {
            var company = await _context.Companies
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (company == null) return NotFound();
            return Ok(company);
        }

        [HttpPost("companies")]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDto dto)
        {
            var company = new Company
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                Address = dto.Address,
                City = dto.City,
                PostalCode = dto.PostalCode,
                Country = dto.Country,
                LogoUrl = dto.LogoUrl,
                PrimaryColor = dto.PrimaryColor ?? "#FFD700",
                SecondaryColor = dto.SecondaryColor ?? "#000000",

                IsActive = dto.IsActive ?? true
            };

            _context.Companies.Add(company);
            await LogActivity("CREATE", "Company", company.Id.ToString(), $"Création entreprise: {company.Name}");
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCompanyById), new { id = company.Id }, company);
        }

        [HttpPut("companies/{id}")]
        public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] UpdateCompanyDto dto)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            company.Name = dto.Name ?? company.Name;
            company.Description = dto.Description ?? company.Description;
            company.ContactEmail = dto.ContactEmail ?? company.ContactEmail;
            company.ContactPhone = dto.ContactPhone ?? company.ContactPhone;
            company.Address = dto.Address ?? company.Address;
            company.City = dto.City ?? company.City;
            company.PostalCode = dto.PostalCode ?? company.PostalCode;
            company.Country = dto.Country ?? company.Country;
            company.LogoUrl = dto.LogoUrl ?? company.LogoUrl;
            company.PrimaryColor = dto.PrimaryColor ?? company.PrimaryColor;
            company.SecondaryColor = dto.SecondaryColor ?? company.SecondaryColor;

            if (dto.IsActive.HasValue) company.IsActive = dto.IsActive.Value;
            if (dto.Status.HasValue) company.Status = dto.Status.Value;

            company.UpdatedAt = DateTime.UtcNow;
            _context.Companies.Update(company);
            await LogActivity("UPDATE", "Company", company.Id.ToString(), $"Mise à jour entreprise: {company.Name}");
            await _context.SaveChangesAsync();
            return Ok(company);
        }

        [HttpGet("companies/pending")]
        public async Task<IActionResult> GetPendingCompanies()
        {
            var companies = await _context.Companies
                .Where(c => c.Status == CompanyStatus.Pending)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return Ok(companies);
        }

        [HttpPost("companies/{id}/approve")]
        public async Task<IActionResult> ApproveCompany(Guid id, [FromServices] IEmailService emailService)
        {
            var company = await _context.Companies
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (company == null) return NotFound();

            company.Status = CompanyStatus.Approved;
            company.IsActive = true;
            
            await LogActivity("APPROVE", "Company", company.Id.ToString(), $"Approbation entreprise: {company.Name}");
            
            // Activer tous les utilisateurs de cette entreprise (normalement juste l'admin au début)
            foreach (var user in company.Users)
            {
                user.IsActive = true;
            }

            await _context.SaveChangesAsync();

            // Envoyer l'email d'activation à l'admin
            var admin = company.Users.FirstOrDefault(u => u.Role == UserRole.CompanyAdmin);
            if (admin != null)
            {
                await emailService.SendAccountActivationAsync(admin.Email, admin.FirstName, company.Name);
            }

            return Ok(new { message = "Entreprise approuvée et activée." });
        }

        [HttpPost("companies/{id}/reject")]
        public async Task<IActionResult> RejectCompany(Guid id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            company.Status = CompanyStatus.Rejected;
            company.IsActive = false;
            
            await LogActivity("REJECT", "Company", company.Id.ToString(), $"Rejet entreprise: {company.Name}");
            await _context.SaveChangesAsync();
            return Ok(new { message = "Entreprise rejetée." });
        }

        [HttpGet("contact-messages")]
        public async Task<IActionResult> GetContactMessages()
        {
            var messages = await _context.ContactMessages!
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
            return Ok(messages);
        }

        [HttpPut("contact-messages/{id}/status")]
        public async Task<IActionResult> UpdateContactMessageStatus(Guid id, [FromBody] UpdateContactMessageStatusDto dto)
        {
            var message = await _context.ContactMessages!.FindAsync(id);
            if (message == null) return NotFound();

            message.Status = dto.Status;
            if (dto.Status == ContactMessageStatus.Resolved)
            {
                message.RespondedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(message);
        }

        [HttpDelete("companies/{id}")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==================== JOB OFFERS ====================

        [HttpGet("joboffers")]
        public async Task<IActionResult> GetJobOffers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var offers = await _context.JobOffers!
                .AsNoTracking()
                .Include(j => j.Company)
                .OrderByDescending(j => j.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var total = await _context.JobOffers!.CountAsync();
            return Ok(new { data = offers, total, pageNumber, pageSize });
        }

        [HttpGet("joboffers/{id}")]
        public async Task<IActionResult> GetJobOfferById(Guid id)
        {
            var offer = await _context.JobOffers!
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == id);
            if (offer == null) return NotFound();
            return Ok(offer);
        }

        [HttpDelete("joboffers/{id}")]
        public async Task<IActionResult> DeleteJobOffer(Guid id)
        {
            var offer = await _context.JobOffers.FindAsync(id);
            if (offer == null) return NotFound();

            _context.JobOffers.Remove(offer);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==================== JOB APPLICATIONS ====================

        [HttpGet("jobapplications")]
        public async Task<IActionResult> GetJobApplications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var applications = await _context.JobApplications!
                .AsNoTracking()
                .Include(ja => ja.JobOffer)
                .Include(ja => ja.Candidate)
                .OrderByDescending(ja => ja.AppliedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var total = await _context.JobApplications!.CountAsync();
            return Ok(new { data = applications, total, pageNumber, pageSize });
        }

        [HttpGet("jobapplications/{id}")]
        public async Task<IActionResult> GetJobApplicationById(Guid id)
        {
            var app = await _context.JobApplications!
                .Include(ja => ja.JobOffer)
                .Include(ja => ja.Candidate)
                .FirstOrDefaultAsync(ja => ja.Id == id);
            if (app == null) return NotFound();
            return Ok(app);
        }

        [HttpDelete("jobapplications/{id}")]
        public async Task<IActionResult> DeleteJobApplication(Guid id)
        {
            var app = await _context.JobApplications.FindAsync(id);
            if (app == null) return NotFound();

            _context.JobApplications!.Remove(app);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==================== CANDIDATES ====================

        [HttpGet("candidates/{id}")]
        public async Task<IActionResult> GetCandidateById(Guid id)
        {
            var candidate = await _context.Candidates
                .AsNoTracking()
                .Include(c => c.Applications)
                    .ThenInclude(a => a.JobOffer)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (candidate == null) return NotFound();

            var dto = new
            {
                candidate.Id,
                candidate.FirstName,
                candidate.LastName,
                candidate.Email,
                candidate.PhoneNumber,
                candidate.LinkedInUrl,
                candidate.PortfolioUrl,
                candidate.MainCVUrl,
                candidate.CreatedAt,
                candidate.UpdatedAt,
                applications = candidate.Applications
                    .OrderByDescending(a => a.AppliedAt)
                    .Select(a => new
                    {
                        a.Id,
                        a.JobOfferId,
                        jobTitle = a.JobOffer != null ? a.JobOffer.Title : null,
                        a.Status,
                        a.AppliedAt,
                        a.UpdatedAt,
                        a.RecruiterNotes,
                        a.CVUrl,
                        a.CoverLetterUrl,
                        a.AiScore
                    })
            };

            return Ok(dto);
        }

        [HttpPut("candidates/{id}")]
        public async Task<IActionResult> UpdateCandidate(Guid id, [FromBody] UpdateCandidateDto dto)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var exists = await _context.Candidates
                    .AnyAsync(c => c.Email.ToLower() == dto.Email.ToLower() && c.Id != id);

                if (exists)
                    return BadRequest(new { message = "Un candidat avec cet email existe deja." });
            }

            if (dto.FirstName != null) candidate.FirstName = dto.FirstName;
            if (dto.LastName != null) candidate.LastName = dto.LastName;
            if (dto.Email != null) candidate.Email = dto.Email.ToLower();
            if (dto.PhoneNumber != null) candidate.PhoneNumber = dto.PhoneNumber;
            if (dto.LinkedInUrl != null) candidate.LinkedInUrl = dto.LinkedInUrl;
            if (dto.PortfolioUrl != null) candidate.PortfolioUrl = dto.PortfolioUrl;
            if (dto.MainCVUrl != null) candidate.MainCVUrl = dto.MainCVUrl;

            candidate.UpdatedAt = DateTime.UtcNow;

            _context.Candidates.Update(candidate);
            await _context.SaveChangesAsync();
            return Ok(candidate);
        }

        [HttpDelete("candidates/{id}")]
        public async Task<IActionResult> DeleteCandidate(Guid id)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null) return NotFound();

            _context.Candidates.Remove(candidate);
            await _context.SaveChangesAsync();
            return NoContent();
        }



        // ==================== ACTIVITY LOGS ====================

        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs([FromQuery] int limit = 100)
        {
            try
            {
                if (_context.ActivityLogs == null)
                {
                    return Ok(new List<ActivityLog>());
                }

                var logs = await _context.ActivityLogs
                    .AsNoTracking()
                    .OrderByDescending(l => l.Timestamp)
                    .Take(limit)
                    .ToListAsync();

                return Ok(logs ?? new List<ActivityLog>());
            }
            catch (Exception ex)
            {
                // Log the actual exception for debugging
                System.Diagnostics.Debug.WriteLine($"Error fetching logs: {ex.Message}");
                return Ok(new List<ActivityLog>()); // Return empty list on error
            }
        }

        // ==================== PROFILE ====================

        [HttpGet("profile")]
        public async Task<IActionResult> GetAdminProfile()
        {
            try
            {
                if (!_currentUserService.UserId.HasValue) 
                    return Unauthorized();

                if (_context.Users == null)
                    return StatusCode(500, new { message = "Le contexte utilisateur n'est pas initialisé." });

                var user = await _context.Users.FindAsync(_currentUserService.UserId.Value);
                if (user == null) 
                    return NotFound(new { message = "Profil administrateur non trouvé." });

                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AdminController] Error in GetAdminProfile: {ex.Message}");
                return StatusCode(500, new { message = "Erreur interne lors de la récupération du profil.", detail = ex.Message });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateAdminProfile([FromBody] UpdateAdminProfileDto dto)
        {
            try
            {
                if (!_currentUserService.UserId.HasValue) 
                    return Unauthorized();

                if (_context.Users == null)
                    return StatusCode(500, new { message = "Le contexte utilisateur n'est pas initialisé." });

                var user = await _context.Users.FindAsync(_currentUserService.UserId.Value);
                if (user == null) 
                    return NotFound(new { message = "Profil administrateur non trouvé." });

                if (dto.FirstName != null) user.FirstName = dto.FirstName;
                if (dto.LastName != null) user.LastName = dto.LastName;
                if (dto.Email != null) user.Email = dto.Email.ToLower();
                if (!string.IsNullOrEmpty(dto.Password)) user.PasswordHash = HashPassword(dto.Password);
                if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
                if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;
                if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;
                if (dto.EmailConfirmed.HasValue) user.EmailConfirmed = dto.EmailConfirmed.Value;
                if (dto.MustChangePassword.HasValue) user.MustChangePassword = dto.MustChangePassword.Value;
                
                user.UpdatedAt = DateTime.UtcNow;

                _context.Users.Update(user);
                await LogActivity("UPDATE_PROFILE", "Admin", user.Id.ToString(), "Mise à jour du profil administrateur");
                await _context.SaveChangesAsync();
                
                return Ok(new { message = "Profil mis à jour avec succès", user });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AdminController] Error in UpdateAdminProfile: {ex.Message}");
                return StatusCode(500, new { message = "Erreur interne lors de la mise à jour du profil.", detail = ex.Message });
            }
        }

        #region Private Helper Methods

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private async Task LogActivity(string action, string entityType, string entityId, string? details = null)
        {
            try
            {
                var userId = _currentUserService.UserId ?? Guid.Empty;
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name ?? "SuperAdmin";

                var log = new ActivityLog
                {
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    UserId = userId,
                    UserEmail = userEmail,
                    Details = details,
                    Timestamp = DateTime.UtcNow
                };

                if (_context.ActivityLogs != null)
                {
                    await _context.ActivityLogs.AddAsync(log);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AdminController] Error logging activity: {ex.Message}");
            }
        }

        #endregion
    }

    // DTOs for User operations
    public class CreateUserDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public string? Password { get; set; }
        public UserRole Role { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UpdateUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public UserRole? Role { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsActive { get; set; }
    }

    // DTOs for Company operations
    public class CreateCompanyDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? LogoUrl { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }

        public bool? IsActive { get; set; } = true;
    }

    public class UpdateCompanyDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? LogoUrl { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }

        public bool? IsActive { get; set; }
        public CompanyStatus? Status { get; set; }
    }



    public class UpdateCandidateDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? MainCVUrl { get; set; }
    }

    public class UpdateAdminProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public bool? IsActive { get; set; }
        public bool? EmailConfirmed { get; set; }
        public bool? MustChangePassword { get; set; }
    }

    public class UpdateContactMessageStatusDto
    {
        public ContactMessageStatus Status { get; set; }
    }
}
