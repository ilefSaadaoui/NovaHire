using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repository d'accès aux données pour les candidatures.
    /// Fournit les opérations CRUD et les requêtes enrichies (eager loading) pour la gestion complète
    /// des candidatures à travers les différentes vues métier (offre, entreprise, recruteur).
    /// </summary>
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="JobApplicationRepository"/>.
        /// </summary>
        /// <param name="context">Le contexte de base de données Entity Framework Core.</param>
        public JobApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère une candidature par son identifiant unique (sans données associées).
        /// Utilise <c>FindAsync</c> pour bénéficier du cache de premier niveau d'EF Core.
        /// </summary>
        /// <param name="id">L'identifiant unique de la candidature.</param>
        /// <returns>La candidature ou null si introuvable.</returns>
        public async Task<JobApplication?> GetByIdAsync(Guid id)
        {
            return await _context.JobApplications.FindAsync(id);
        }

        /// <summary>
        /// Récupère une candidature par son identifiant avec l'offre d'emploi, le candidat
        /// et l'analyse IA (y compris les données extraites) chargés en eager loading.
        /// </summary>
        /// <param name="id">L'identifiant unique de la candidature.</param>
        /// <returns>La candidature enrichie ou null si introuvable.</returns>
        public async Task<JobApplication?> GetByIdWithJobOfferAsync(Guid id)
        {
            return await _context.JobApplications
                .Include(ja => ja.JobOffer)
                .Include(ja => ja.Candidate)
                .Include(ja => ja.AIAnalysis)
                    .ThenInclude(ai => ai!.ExtractedData) // !.ExtractedData car AIAnalysis peut être null
                .FirstOrDefaultAsync(ja => ja.Id == id);
        }

        /// <summary>
        /// Récupère une candidature complète avec toutes ses relations associées :
        /// offre, candidat, analyse IA, commentaires (avec leurs auteurs), notifications et évaluations.
        /// Utilisé pour les vues de détail nécessitant toutes les informations.
        /// </summary>
        /// <param name="id">L'identifiant unique de la candidature.</param>
        /// <returns>La candidature complète ou null si introuvable.</returns>
        public async Task<JobApplication?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.JobApplications
                .Include(ja => ja.JobOffer)
                .Include(ja => ja.Candidate)
                .Include(ja => ja.AIAnalysis)
                    .ThenInclude(ai => ai!.ExtractedData)
                .Include(ja => ja.Comments)
                    .ThenInclude(c => c.Author) // Charge l'auteur de chaque commentaire
                .Include(ja => ja.Notifications)
                .Include(ja => ja.Ratings)
                .FirstOrDefaultAsync(ja => ja.Id == id);
        }

        /// <summary>
        /// Récupère toutes les candidatures associées à une offre d'emploi donnée,
        /// avec le candidat, l'analyse IA et les évaluations chargés.
        /// Résultats triés par date de candidature décroissante.
        /// </summary>
        /// <param name="jobOfferId">L'identifiant unique de l'offre d'emploi.</param>
        /// <returns>La liste des candidatures pour l'offre, triées du plus récent au plus ancien.</returns>
        public async Task<IEnumerable<JobApplication>> GetByJobOfferIdAsync(Guid jobOfferId)
        {
            return await _context.JobApplications
                .AsNoTracking()
                .Include(ja => ja.JobOffer)
                .Include(ja => ja.Candidate)
                .Include(ja => ja.AIAnalysis)
                .Include(ja => ja.Ratings)
                .Where(ja => ja.JobOfferId == jobOfferId)
                .OrderByDescending(ja => ja.AppliedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère toutes les candidatures reçues par une entreprise (tous recruteurs confondus),
        /// avec les détails de l'offre, du candidat, de l'analyse IA et des évaluations.
        /// Résultats triés par date de candidature décroissante.
        /// </summary>
        /// <param name="companyId">L'identifiant unique de l'entreprise.</param>
        /// <returns>La liste des candidatures reçues par l'entreprise.</returns>
        public async Task<IEnumerable<JobApplication>> GetByCompanyIdAsync(Guid companyId)
        {
            return await _context.JobApplications
                .AsNoTracking()
                .Include(ja => ja.JobOffer)
                .Include(ja => ja.Candidate)
                .Include(ja => ja.AIAnalysis)
                .Include(ja => ja.Ratings)
                .Where(ja => ja.JobOffer.CompanyId == companyId) // Filtrage via l'offre, non le tenant
                .OrderByDescending(ja => ja.AppliedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère toutes les candidatures liées aux offres créées par un utilisateur spécifique.
        /// Utilisé par les recruteurs pour voir uniquement les candidatures sur leurs propres offres.
        /// </summary>
        /// <param name="userId">L'identifiant du recruteur (créateur des offres).</param>
        /// <returns>La liste des candidatures sur les offres du recruteur.</returns>
        public async Task<IEnumerable<JobApplication>> GetByCreatedByIdAsync(Guid userId)
        {
            return await _context.JobApplications
                .AsNoTracking()
                .Include(ja => ja.JobOffer)
                .Include(ja => ja.Candidate)
                .Include(ja => ja.AIAnalysis)
                .Include(ja => ja.Ratings)
                .Where(ja => ja.JobOffer.CreatedById == userId) // Filtrage par créateur de l'offre
                .OrderByDescending(ja => ja.AppliedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Retourne le nombre total de candidatures reçues par une entreprise.
        /// </summary>
        /// <param name="companyId">L'identifiant unique de l'entreprise.</param>
        /// <returns>Le nombre de candidatures liées aux offres de l'entreprise.</returns>
        public async Task<int> CountByCompanyIdAsync(Guid companyId)
        {
            return await _context.JobApplications
                .Include(ja => ja.JobOffer)
                .CountAsync(ja => ja.JobOffer.CompanyId == companyId);
        }

        /// <summary>
        /// Retourne le nombre total de candidatures liées aux offres créées par un utilisateur.
        /// </summary>
        /// <param name="userId">L'identifiant du recruteur (créateur des offres).</param>
        /// <returns>Le nombre de candidatures sur les offres du recruteur.</returns>
        public async Task<int> CountByCreatedByIdAsync(Guid userId)
        {
            return await _context.JobApplications
                .Include(ja => ja.JobOffer)
                .CountAsync(ja => ja.JobOffer.CreatedById == userId);
        }

        /// <summary>
        /// Ajoute une nouvelle candidature en base de données.
        /// </summary>
        /// <param name="jobApplication">La candidature à créer.</param>
        public async Task AddAsync(JobApplication jobApplication)
        {
            await _context.JobApplications.AddAsync(jobApplication);
        }

        /// <summary>
        /// Met à jour une candidature existante en base de données.
        /// </summary>
        /// <param name="jobApplication">La candidature modifiée à sauvegarder.</param>
        public async Task UpdateAsync(JobApplication jobApplication)
        {
            _context.JobApplications.Update(jobApplication);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Supprime une candidature de la base de données.
        /// </summary>
        /// <param name="jobApplication">La candidature à supprimer.</param>
        public async Task DeleteAsync(JobApplication jobApplication)
        {
            _context.JobApplications.Remove(jobApplication);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Persiste toutes les modifications en attente dans la base de données.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
