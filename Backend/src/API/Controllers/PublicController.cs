using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Public;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    /// <summary>
    /// Contrôleur gérant toutes les opérations d'accès public et anonyme (sans authentification).
    /// Gère les statistiques de la plateforme, l'affichage des offres d'emploi publiques via token de partage, 
    /// la soumission de candidatures anonymes (portail candidat) et les messages de contact général.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PublicController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJobOfferRepository _jobOfferRepository;
        private readonly IJobApplicationRepository _jobApplicationRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailService _emailService;
        private readonly IStorageService _storageService;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="PublicController"/>.
        /// </summary>
        /// <param name="context">Contexte Entity Framework de l'application.</param>
        /// <param name="jobOfferRepository">Repository pour la gestion des offres d'emploi.</param>
        /// <param name="jobApplicationRepository">Repository pour la gestion des candidatures.</param>
        /// <param name="environment">Environnement d'hébergement web.</param>
        /// <param name="emailService">Service d'envoi d'e-mails.</param>
        /// <param name="storageService">Service de stockage persistant des fichiers (ex: CVs).</param>
        public PublicController(
            ApplicationDbContext context,
            IJobOfferRepository jobOfferRepository,
            IJobApplicationRepository jobApplicationRepository,
            IWebHostEnvironment environment,
            IEmailService emailService,
            IStorageService storageService)
        {
            _context = context;
            _jobOfferRepository = jobOfferRepository;
            _jobApplicationRepository = jobApplicationRepository;
            _environment = environment;
            _emailService = emailService;
            _storageService = storageService;
        }

        /// <summary>
        /// Récupère des statistiques globales de la plateforme (nombre de recruteurs actifs).
        /// </summary>
        /// <returns>Un objet JSON contenant le nombre total de recruteurs.</returns>
        [HttpGet("platform-stats")]
        public async Task<IActionResult> GetPlatformStats()
        {
            try
            {
                // IgnoreQueryFilters est requis car les requêtes anonymes n'ont pas de claims d'entreprise (CompanyId)
                var recruitersCount = await _context.Users!
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .CountAsync(u => u.Role == UserRole.Recruiter && u.IsActive);

                return Ok(new { recruitersCount });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PublicController] Error in GetPlatformStats: {ex.Message} \n {ex.StackTrace}");
                return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        /// <summary>
        /// Récupère une offre d'emploi publiée spécifique identifiée par son token de partage unique.
        /// </summary>
        /// <param name="token">Le jeton unique de partage de l'offre d'emploi.</param>
        /// <returns>Le DTO contenant les informations de l'offre ainsi que la configuration dynamique de son formulaire de candidature.</returns>
        [HttpGet("offers/{token}")]
        public async Task<IActionResult> GetOfferByToken(string token)
        {
            try
            {
                // Recherche par token sans filtre global de multitenancy pour permettre l'affichage public
                var offer = await _context.JobOffers!
                    .IgnoreQueryFilters()
                    .Include(o => o.Company)
                    .FirstOrDefaultAsync(o => o.ShareToken == token);

                if (offer == null || offer.Status != JobOfferStatus.Published)
                {
                    return NotFound(new { message = "Offre introuvable ou fermée." });
                }

                if (offer.Deadline.HasValue && offer.Deadline.Value < DateTime.UtcNow)
                {
                    return NotFound(new { message = "La date limite pour postuler à cette offre est dépassée." });
                }

                var dto = new PublicOfferDto
                {
                    Id = offer.Id,
                    Title = offer.Title,
                    Description = offer.Description,
                    Company = offer.Company?.Name ?? "",
                    CompanyLogo = offer.Company?.LogoUrl,
                    CompanyDescription = offer.Company?.Description,
                    CompanyIndustry = offer.Company?.Industry,
                    CompanyWebsite = offer.Company?.Website,
                    CompanyCountry = offer.Company?.Country,
                    CompanyPhone = offer.Company?.ContactPhone,
                    Location = offer.Location ?? "",
                    Type = offer.Type.ToString(),
                    SalaryRange = offer.SalaryRange,
                    ShowSalary = offer.DisplayConfig?.ShowSalary ?? false,
                    RemotePolicy = offer.RemotePolicy.ToString(),
                    ExperienceLevel = offer.ExperienceLevel.ToString(),
                    PageColor = offer.Company?.PrimaryColor ?? "#00A7E1",
                    PageSecondaryColor = offer.Company?.SecondaryColor ?? "#fcd34d",
                    WelcomeMsg = offer.Company?.Name != null ? $"Explorez vos futures opportunités chez {offer.Company.Name}." : "Explorez vos futures opportunités.",
                    Skills = offer.Skills ?? new List<string>(),
                    // Mappage de la configuration dynamique du formulaire
                    RequireFullName = offer.FormConfig?.RequireFullName ?? true,
                    RequireEmail = offer.FormConfig?.RequireEmail ?? true,
                    RequirePhone = offer.FormConfig?.RequirePhone ?? false,
                    RequireCV = offer.FormConfig?.RequireCV ?? true,
                    RequireCoverLetter = offer.FormConfig?.RequireCoverLetter ?? false,
                    RequirePortfolio = offer.FormConfig?.RequirePortfolio ?? false,
                    RequireLinkedIn = offer.FormConfig?.RequireLinkedIn ?? false,
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

        /// <summary>
        /// Soumet la candidature d'un candidat à une offre d'emploi spécifique.
        /// Valide les contraintes de l'offre, gère l'enregistrement/mise à jour du candidat, 
        /// téléverse le fichier CV sur le cloud (Cloudinary) et enregistre la candidature.
        /// </summary>
        /// <param name="req">Le formulaire contenant les informations personnelles et le fichier CV du candidat.</param>
        /// <returns>Un message de succès accompagné de l'ID de la candidature créée ou mise à jour.</returns>
        [HttpPost("applications")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitApplication([FromForm] PublicApplicationRequest req)
        {
            try
            {
                Console.WriteLine($"[PublicController] Received application for token: {req?.ShareToken}, Email: {req?.Email}");
                
                if (req == null) return BadRequest(new { message = "Requête invalide (données manquantes)" });

                var offer = await _context.JobOffers!
                    .IgnoreQueryFilters()
                    .Include(o => o.Company)
                    .FirstOrDefaultAsync(o => o.ShareToken == req.ShareToken);

                if (offer == null)
                {
                    Console.WriteLine($"[PublicController] Offer not found for token: {req.ShareToken}");
                    return NotFound(new { message = "Offre parente introuvable" });
                }

                // --- REGLES METIER DE RECRUTEMENT ---

                // 1. Vérification du statut de l'offre
                if (offer.Status != JobOfferStatus.Published)
                {
                    Console.WriteLine($"[PublicController] Offer {offer.Id} is not published (Status: {offer.Status})");
                    return BadRequest(new { message = "Cette offre n'est plus ouverte aux candidatures (Statut: " + offer.Status + ")" });
                }

                // 2. Vérification de la date limite
                if (offer.Deadline.HasValue && offer.Deadline.Value < DateTime.UtcNow)
                {
                    return BadRequest(new { message = "La date limite pour postuler à cette offre est dépassée (" + offer.Deadline.Value.ToString("dd/MM/yyyy") + ")" });
                }

                // 3. Validation dynamique du formulaire selon la configuration de l'offre (FormConfig)
                var config = offer.FormConfig ?? new ApplicationFormConfig();
                
                if (config.RequireFullName && (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName)))
                {
                    return BadRequest(new { message = "Le nom et le prénom sont obligatoires pour cette offre." });
                }

                if (config.RequireEmail && string.IsNullOrWhiteSpace(req.Email))
                {
                    return BadRequest(new { message = "L'adresse e-mail est obligatoire." });
                }

                if (config.RequirePhone && string.IsNullOrWhiteSpace(req.Phone))
                {
                    return BadRequest(new { message = "Le numéro de téléphone est requis pour cette offre." });
                }

                if (config.RequireCV && (req.CVFile == null || req.CVFile.Length == 0))
                {
                    return BadRequest(new { message = "Un curriculum vitae (CV) est obligatoire pour postuler à cette offre." });
                }

                if (config.RequireLinkedIn && string.IsNullOrWhiteSpace(req.LinkedIn))
                {
                    return BadRequest(new { message = "Le lien vers votre profil LinkedIn est requis." });
                }

                if (config.RequirePortfolio && string.IsNullOrWhiteSpace(req.Portfolio))
                {
                    return BadRequest(new { message = "Le lien vers votre portfolio est requis." });
                }

                // --- FIN DES REGLES METIER ---

                // Gestion du Profil Candidat (Upsert) - Isolé par entreprise
                var candidate = await _context.Candidates!
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Email.ToLower() == req.Email.ToLower() && c.CompanyId == offer.CompanyId);

                if (candidate == null)
                {
                    candidate = new Candidate
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = offer.CompanyId,
                        Email = req.Email.ToLower(),
                        FirstName = req.FirstName,
                        LastName = req.LastName,
                        PhoneNumber = req.Phone,
                        LinkedInUrl = req.LinkedIn,
                        PortfolioUrl = req.Portfolio,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.Candidates!.AddAsync(candidate);
                }
                else
                {
                    // Mise à jour du candidat existant avec les nouvelles données soumises
                    candidate.FirstName = req.FirstName;
                    candidate.LastName = req.LastName;
                    candidate.PhoneNumber = req.Phone;
                    candidate.LinkedInUrl = req.LinkedIn;
                    candidate.PortfolioUrl = req.Portfolio;
                    candidate.UpdatedAt = DateTime.UtcNow;
                }

                // --- VERIFICATION DES DOUBLONS DE CANDIDATURES ---
                var existingApp = await _context.JobApplications!
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(a => a.CandidateId == candidate.Id && a.JobOfferId == offer.Id);

                // On autorise la mise à jour si la candidature est "Submitted" ou "UnderReview" (en cours d'analyse)
                if (existingApp != null && 
                    existingApp.Status != ApplicationStatus.Submitted && 
                    existingApp.Status != ApplicationStatus.UnderReview)
                {
                    Console.WriteLine($"[PublicController] Blocked duplicate for candidate {candidate.Id} (Status: {existingApp.Status})");
                    return BadRequest(new { message = "Vous avez déjà postulé à cette offre et votre dossier a déjà été traité ou est à un stade avancé." });
                }

                // Gestion du téléversement de fichier (vers le service de stockage Cloudinary)
                string cvUrl = existingApp?.CVUrl ?? candidate.MainCVUrl ?? "https://placeholder-cv.url";
                if (req.CVFile != null && req.CVFile.Length > 0)
                {
                    var extension = Path.GetExtension(req.CVFile.FileName).ToLowerInvariant();
                    var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };

                    if (!allowedExtensions.Contains(extension))
                        return BadRequest(new { message = "Format de CV non supporté (PDF, DOC, DOCX uniquement)." });

                    // Construction du chemin du dossier : novahire/companies/{companyName}/cvs
                    var companyName = offer.Company?.Name ?? "General";
                    var folderPath = $"novahire/companies/{companyName.Replace(" ", "_")}/cvs";

                    using (var stream = req.CVFile.OpenReadStream())
                    {
                        cvUrl = await _storageService.UploadFileAsync(stream, req.CVFile.FileName, folderPath);
                    }

                    // Mise à jour du CV principal du candidat
                    candidate.MainCVUrl = cvUrl;
                }

                if (existingApp != null)
                {
                    // MISE A JOUR D'UNE CANDIDATURE EXISTANTE
                    existingApp.CVUrl = cvUrl;
                    existingApp.CoverLetterUrl = req.CoverLetter;
                    existingApp.AppliedAt = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(req.Source)) existingApp.Source = req.Source;
                    
                    await _jobApplicationRepository.UpdateAsync(existingApp);
                    await _jobApplicationRepository.SaveChangesAsync();

                    // Envoi asynchrone (fire-and-forget) de l'email de confirmation pour ne pas ralentir le client
                    _ = _emailService.SendApplicationConfirmationAsync(
                        candidate.Email,
                        $"{candidate.FirstName} {candidate.LastName}",
                        offer.Title,
                        offer.Company?.Name ?? "NovaHire"
                    );

                    return Ok(new { message = "Votre candidature existante a été mise à jour avec succès", id = existingApp.Id });
                }
                else
                {
                    // CREATION D'UNE NOUVELLE CANDIDATURE
                    var application = new JobApplication
                    {
                        Id = Guid.NewGuid(),
                        JobOfferId = offer.Id,
                        CandidateId = candidate.Id,
                        CVUrl = cvUrl,
                        CoverLetterUrl = req.CoverLetter,
                        Status = ApplicationStatus.Submitted,
                        AppliedAt = DateTime.UtcNow,
                        Source = req.Source
                    };

                    await _jobApplicationRepository.AddAsync(application);
                    await _jobApplicationRepository.SaveChangesAsync();

                    // Envoi asynchrone (fire-and-forget) de l'email de confirmation
                    _ = _emailService.SendApplicationConfirmationAsync(
                        candidate.Email,
                        $"{candidate.FirstName} {candidate.LastName}",
                        offer.Title,
                        offer.Company?.Name ?? "NovaHire"
                    );

                    return Ok(new { message = "Candidature transmise avec succès", id = application.Id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PublicController] EXCEPTION during submission: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Soumet un message de contact général depuis le site web public.
        /// </summary>
        /// <param name="req">Le message contenant les coordonnées de l'expéditeur.</param>
        [HttpPost("contact")]
        public async Task<IActionResult> SubmitContactMessage([FromBody] ContactMessageDto req)
        {
            if (req == null) return BadRequest();

            var message = new ContactMessage
            {
                Id = Guid.NewGuid(),
                FullName = req.FullName,
                Email = req.Email,
                Company = req.Company,
                Phone = req.Phone,
                Message = req.Message,
                Status = ContactMessageStatus.New,
                CreatedAt = DateTime.UtcNow
            };

            _context.ContactMessages!.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message envoyé avec succès" });
        }

        /// <summary>
        /// DTO de requête pour la soumission d'un message de contact.
        /// </summary>
        public class ContactMessageDto
        {
            public required string FullName { get; set; }
            public required string Email { get; set; }
            public string? Company { get; set; }
            public string? Phone { get; set; }
            public required string Message { get; set; }
        }

        /// <summary>
        /// Modèle contenant les paramètres de formulaire multipart pour soumettre une candidature.
        /// </summary>
        public class PublicApplicationRequest
        {
            public string ShareToken { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? Phone { get; set; }
            public string? LinkedIn { get; set; }
            public string? Portfolio { get; set; }
            public string? CoverLetter { get; set; }
            public string? Source { get; set; }
            public IFormFile? CVFile { get; set; }
        }
    }
}
