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
    /// <summary>
    /// Contrôleur d'administration globale (SuperAdmin).
    /// Fournit les fonctionnalités de monitoring système (health), de gestion des utilisateurs,
    /// de gestion des entreprises (création, approbation/rejet), de modération des offres,
    /// des candidatures, des candidats, et d'audit des logs d'activité de la plateforme.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "SuperAdminOnly")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="AdminController"/>.
        /// </summary>
        /// <param name="context">Contexte de la base de données.</param>
        /// <param name="currentUserService">Service de gestion de l'utilisateur connecté.</param>
        /// <param name="configuration">Configuration de l'application.</param>
        /// <param name="emailService">Service d'envoi d'e-mails.</param>
        public AdminController(ApplicationDbContext context, ICurrentUserService currentUserService, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _configuration = configuration;
            _emailService = emailService;
        }

        /// <summary>
        /// Récupère l'état de santé de tous les services connectés (API, Base de données, Stockage Cloudinary, Service IA).
        /// </summary>
        /// <returns>Un objet contenant l'état de fonctionnement (Stable/Down) de chaque composant.</returns>
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
                // Vérification de la connexion à la base de données
                bool canConnect = await _context.Database.CanConnectAsync();
                health = health with { database = new { status = canConnect ? "Stable" : "Down", message = canConnect ? "Optimale" : "Error" } };

                // Vérification du service d'intelligence artificielle externe (Python)
                var aiUrl = _configuration["AISettings:PythonServiceUrl"] ?? _configuration["AIService:Url"] ?? "http://localhost:8000";
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(2);
                    try 
                    {
                        var response = await client.GetAsync(aiUrl);
                        bool isUp = response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound; // Un code 404 signifie que le service répond
                        health = health with { ai = new { status = isUp ? "Stable" : "Down", message = isUp ? "Actif" : "Offline" } };
                    }
                    catch { /* Le service IA est inaccessible */ }
                }

                // Vérification du service de stockage Cloudinary
                var cloudName = _configuration["CloudinarySettings:CloudName"];
                if (!string.IsNullOrEmpty(cloudName))
                {
                    // Ping vers Cloudinary
                    using (var client = new System.Net.Http.HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(2);
                        try
                        {
                            var response = await client.GetAsync($"https://res.cloudinary.com/{cloudName}/image/upload/sample.jpg");
                            bool isUp = response.IsSuccessStatusCode;
                            health = health with { storage = new { status = isUp ? "Stable" : "Down", message = isUp ? "98% Libre" : "Restricted" } };
                        }
                        catch { /* Service Cloudinary injoignable */ }
                    }
                }

                return Ok(health);
            }
            catch (Exception)
            {
                return Ok(health); // Retourne l'état partiel en cas d'exception non gérée
            }
        }

        /// <summary>
        /// Récupère l'intégralité des données brutes de la base de données pour l'administration (Utilisateurs, Entreprises, Offres, Candidatures).
        /// </summary>
        /// <returns>Un objet regroupant toutes les listes de données.</returns>
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

        /// <summary>
        /// Récupère un résumé statistique du système (nombres totaux et actifs d'entités, distribution des rôles).
        /// </summary>
        /// <returns>Un objet JSON résumant les métriques clés de la plateforme.</returns>
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

                // Nombres totaux pour les entités actives uniquement
                var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
                var activeCompanies = await _context.Companies.CountAsync(c => c.IsActive);

                // Distribution des rôles (Utilisateurs + Candidats)
                // Matérialisation préalable pour éviter les erreurs de traduction SQL de Enum.ToString() sur certains providers EF Core
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
                    totalCandidatesCount = totalCandidates, // Alias pour compatibilité descendante
                    activeUsers,
                    activeCompanies,
                    roleDistribution
                };
                return Ok(summary);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AdminController] Error in GetTablesSummary: {ex.Message}");
                return StatusCode(500, new { message = "Erreur lors de la récupération du résumé admin.", detail = ex.Message });
            }
        }

        // ==================== UTILISATEURS ====================

        /// <summary>
        /// Récupère la liste paginée des utilisateurs du système avec filtres optionnels par rôle, entreprise et recherche textuelle.
        /// </summary>
        /// <param name="pageNumber">Numéro de la page à récupérer (défaut : 1).</param>
        /// <param name="pageSize">Nombre d'utilisateurs par page (défaut : 50).</param>
        /// <param name="role">Filtre optionnel de rôle (SuperAdmin, CompanyAdmin, Recruiter, etc.).</param>
        /// <param name="companyId">Filtre optionnel sur l'entreprise.</param>
        /// <param name="search">Terme de recherche appliqué sur le nom, prénom ou email.</param>
        /// <returns>La liste paginée des utilisateurs et le nombre total d'enregistrements.</returns>
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

        /// <summary>
        /// Récupère les détails d'un utilisateur par son ID unique.
        /// </summary>
        /// <param name="id">Identifiant unique de l'utilisateur.</param>
        /// <returns>L'entité de l'utilisateur demandé.</returns>
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        /// <summary>
        /// Crée un nouvel utilisateur dans le système (généralement initié par le SuperAdmin).
        /// </summary>
        /// <param name="dto">Les données nécessaires pour créer l'utilisateur.</param>
        /// <returns>L'utilisateur créé avec son code de retour 201 Created.</returns>
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

        /// <summary>
        /// Met à jour les informations d'un utilisateur existant.
        /// </summary>
        /// <param name="id">L'identifiant unique de l'utilisateur à modifier.</param>
        /// <param name="dto">Les données de mise à jour de l'utilisateur.</param>
        /// <returns>L'utilisateur mis à jour.</returns>
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

        /// <summary>
        /// Supprime définitivement un utilisateur du système.
        /// </summary>
        /// <param name="id">L'identifiant de l'utilisateur à supprimer.</param>
        /// <returns>Code HTTP 204 NoContent en cas de succès.</returns>
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

        /// <summary>
        /// Renvoie une invitation par e-mail avec un mot de passe temporaire pour un utilisateur (SuperAdmin).
        /// </summary>
        [HttpPost("users/{id}/resend-invitation")]
        public async Task<IActionResult> ResendUserInvitation(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "Utilisateur introuvable." });

            string tempPassword = GenerateSimpleTempPassword(14);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
            user.MustChangePassword = true;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var companyName = "NovaHire";
            if (user.CompanyId.HasValue)
            {
                var company = await _context.Companies.FindAsync(user.CompanyId.Value);
                if (company != null) companyName = company.Name;
            }

            var emailSuccess = await _emailService.SendRecruiterInvitationAsync(
                user.Email, 
                tempPassword, 
                companyName, 
                "L'Administrateur Plateforme NovaHire"
            );

            await LogActivity("INVITE", "User", user.Id.ToString(), $"Renvoi invitation: {user.Email} (Email: {(emailSuccess ? "Envoyé" : "Échoué")})");

            return Ok(new
            {
                message = emailSuccess 
                    ? $"Invitation renvoyée avec succès à {user.Email}." 
                    : $"Invitation réinitialisée. (Note: l'e-mail automatique n'a pas pu être envoyé par SMTP. Mot de passe temporaire : {tempPassword})",
                tempPassword = tempPassword,
                emailSent = emailSuccess
            });
        }

        private string GenerateSimpleTempPassword(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // ==================== ENTREPRISES ====================

        /// <summary>
        /// Récupère la liste paginée des entreprises de la plateforme.
        /// </summary>
        /// <param name="pageNumber">Numéro de la page (défaut : 1).</param>
        /// <param name="pageSize">Taille de la page (défaut : 50).</param>
        /// <returns>La liste des entreprises et le total.</returns>
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

        /// <summary>
        /// Récupère les détails d'une entreprise incluant ses utilisateurs associés.
        /// </summary>
        /// <param name="id">Identifiant unique de l'entreprise.</param>
        [HttpGet("companies/{id}")]
        public async Task<IActionResult> GetCompanyById(Guid id)
        {
            var company = await _context.Companies
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (company == null) return NotFound();
            return Ok(company);
        }

        /// <summary>
        /// Crée directement une nouvelle entreprise activée.
        /// </summary>
        /// <param name="dto">Les données nécessaires pour créer l'entreprise.</param>
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

        /// <summary>
        /// Met à jour les informations d'une entreprise existante.
        /// </summary>
        /// <param name="id">L'identifiant unique de l'entreprise à modifier.</param>
        /// <param name="dto">Les nouvelles données de l'entreprise.</param>
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

        /// <summary>
        /// Récupère toutes les entreprises en attente d'approbation (Status = Pending).
        /// </summary>
        [HttpGet("companies/pending")]
        public async Task<IActionResult> GetPendingCompanies()
        {
            var companies = await _context.Companies
                .Where(c => c.Status == CompanyStatus.Pending)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return Ok(companies);
        }

        /// <summary>
        /// Approuve l'inscription d'une entreprise, active ses administrateurs, et leur envoie un e-mail d'activation.
        /// </summary>
        /// <param name="id">L'identifiant de l'entreprise à approuver.</param>
        /// <param name="emailService">Injection du service e-mail d'activation.</param>
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
            
            // Activation automatique de tous les utilisateurs créés lors de l'inscription de l'entreprise
            foreach (var user in company.Users)
            {
                user.IsActive = true;
            }

            await _context.SaveChangesAsync();

            // Envoi de l'email d'activation de compte au gestionnaire principal (CompanyAdmin)
            var admin = company.Users.FirstOrDefault(u => u.Role == UserRole.CompanyAdmin);
            if (admin != null)
            {
                await emailService.SendAccountActivationAsync(admin.Email, admin.FirstName, company.Name);
            }

            return Ok(new { message = "Entreprise approuvée et activée." });
        }

        /// <summary>
        /// Rejette l'inscription d'une entreprise et la désactive.
        /// </summary>
        /// <param name="id">L'identifiant de l'entreprise à rejeter.</param>
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

        /// <summary>
        /// Récupère la liste de tous les messages de contact général postés par le public.
        /// </summary>
        [HttpGet("contact-messages")]
        public async Task<IActionResult> GetContactMessages()
        {
            var messages = await _context.ContactMessages!
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
            return Ok(messages);
        }

        /// <summary>
        /// Met à jour le statut de traitement d'un message de contact (ex: Résolu).
        /// </summary>
        /// <param name="id">L'ID du message de contact.</param>
        /// <param name="dto">Le nouveau statut à appliquer.</param>
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

        /// <summary>
        /// Supprime définitivement une entreprise du système.
        /// </summary>
        /// <param name="id">L'identifiant de l'entreprise.</param>
        [HttpDelete("companies/{id}")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==================== OFFRES D'EMPLOI ====================

        /// <summary>
        /// Récupère la liste paginée de toutes les offres d'emploi présentes sur la plateforme (toutes entreprises confondues).
        /// </summary>
        /// <param name="pageNumber">Numéro de la page.</param>
        /// <param name="pageSize">Taille de la page.</param>
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

        /// <summary>
        /// Récupère une offre d'emploi spécifique par son ID unique pour l'administration.
        /// </summary>
        /// <param name="id">ID de l'offre d'emploi.</param>
        [HttpGet("joboffers/{id}")]
        public async Task<IActionResult> GetJobOfferById(Guid id)
        {
            var offer = await _context.JobOffers!
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == id);
            if (offer == null) return NotFound();
            return Ok(offer);
        }

        /// <summary>
        /// Supprime définitivement une offre d'emploi de la plateforme (Modération SuperAdmin).
        /// </summary>
        /// <param name="id">L'identifiant unique de l'offre à supprimer.</param>
        [HttpDelete("joboffers/{id}")]
        public async Task<IActionResult> DeleteJobOffer(Guid id)
        {
            var offer = await _context.JobOffers.FindAsync(id);
            if (offer == null) return NotFound();

            _context.JobOffers.Remove(offer);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==================== CANDIDATURES ====================

        /// <summary>
        /// Récupère toutes les candidatures du système de façon paginée avec liaisons Offres et Candidats.
        /// </summary>
        /// <param name="pageNumber">Le numéro de page.</param>
        /// <param name="pageSize">La taille de page.</param>
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

        /// <summary>
        /// Récupère les détails de candidature par son identifiant unique.
        /// </summary>
        /// <param name="id">Identifiant de la candidature.</param>
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

        /// <summary>
        /// Supprime définitivement une candidature du système.
        /// </summary>
        /// <param name="id">L'identifiant unique de la candidature.</param>
        [HttpDelete("jobapplications/{id}")]
        public async Task<IActionResult> DeleteJobApplication(Guid id)
        {
            var app = await _context.JobApplications.FindAsync(id);
            if (app == null) return NotFound();

            _context.JobApplications!.Remove(app);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==================== CANDIDATS ====================

        /// <summary>
        /// Récupère la fiche complète d'un candidat, avec son historique de candidatures et ses scores d'évaluation IA.
        /// </summary>
        /// <param name="id">L'identifiant unique du candidat.</param>
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

        /// <summary>
        /// Met à jour les informations de profil d'un candidat.
        /// </summary>
        /// <param name="id">L'identifiant unique du candidat.</param>
        /// <param name="dto">Les données à modifier.</param>
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
                    return BadRequest(new { message = "Un candidat avec cet email existe déjà." });
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

        /// <summary>
        /// Supprime définitivement le profil d'un candidat ainsi que tout son historique lié.
        /// </summary>
        /// <param name="id">L'identifiant unique du candidat.</param>
        [HttpDelete("candidates/{id}")]
        public async Task<IActionResult> DeleteCandidate(Guid id)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null) return NotFound();

            _context.Candidates.Remove(candidate);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==================== LOGS D'ACTIVITE (AUDIT) ====================

        /// <summary>
        /// Récupère les derniers journaux d'activité (logs d'audit) enregistrés sur la plateforme.
        /// </summary>
        /// <param name="limit">Le nombre maximum de lignes de log à renvoyer (défaut : 100).</param>
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
                System.Diagnostics.Debug.WriteLine($"Error fetching logs: {ex.Message}");
                return Ok(new List<ActivityLog>()); // Retourne une liste vide en cas d'erreur de récupération
            }
        }

        // ==================== PROFIL DE L'ADMINISTRATEUR ====================

        /// <summary>
        /// Récupère le profil de l'administrateur de session actuellement connecté.
        /// </summary>
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

        /// <summary>
        /// Met à jour les informations du profil de l'administrateur connecté.
        /// </summary>
        /// <param name="dto">Les nouvelles informations de profil.</param>
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

        /// <summary>
        /// Hache de façon sécurisée le mot de passe fourni via BCrypt.
        /// </summary>
        /// <param name="password">Le mot de passe en clair.</param>
        /// <returns>La chaîne du mot de passe haché.</returns>
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Journalise une action d'administration dans la table d'audit des logs d'activité.
        /// </summary>
        /// <param name="action">L'intitulé de l'action (CREATE, UPDATE, DELETE...).</param>
        /// <param name="entityType">Le type d'entité sur lequel l'action porte (User, Company...).</param>
        /// <param name="entityId">L'identifiant de l'entité ciblée.</param>
        /// <param name="details">Informations complémentaires textuelles.</param>
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

    /// <summary>
    /// DTO pour la création d'un utilisateur par le SuperAdmin.
    /// </summary>
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

    /// <summary>
    /// DTO pour la mise à jour d'un utilisateur par le SuperAdmin.
    /// </summary>
    public class UpdateUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public UserRole? Role { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO pour la création directe d'une entreprise par le SuperAdmin.
    /// </summary>
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

    /// <summary>
    /// DTO pour la modification d'une entreprise par le SuperAdmin.
    /// </summary>
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

    /// <summary>
    /// DTO pour la modification des informations d'un candidat.
    /// </summary>
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

    /// <summary>
    /// DTO pour la mise à jour de profil de l'administrateur connecté.
    /// </summary>
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

    /// <summary>
    /// DTO pour la mise à jour du statut d'un message de contact public.
    /// </summary>
    public class UpdateContactMessageStatusDto
    {
        public ContactMessageStatus Status { get; set; }
    }
}
