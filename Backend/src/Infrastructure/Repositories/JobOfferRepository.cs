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
    /// Repository d'accès aux données pour les offres d'emploi.
    /// Fournit les méthodes de lecture, d'écriture et de recherche avec support multi-tenant.
    /// </summary>
    public class JobOfferRepository : IJobOfferRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="JobOfferRepository"/>.
        /// </summary>
        /// <param name="context">Le contexte de base de données Entity Framework Core.</param>
        public JobOfferRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère une offre d'emploi par son identifiant, avec les candidatures associées chargées.
        /// </summary>
        /// <param name="id">L'identifiant unique de l'offre.</param>
        /// <returns>L'offre d'emploi ou null si introuvable.</returns>
        public async Task<JobOffer?> GetByIdAsync(Guid id)
        {
            return await _context.JobOffers
                .Include(j => j.Applications)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        /// <summary>
        /// Récupère une offre d'emploi par son identifiant avec les informations complètes de l'entreprise.
        /// </summary>
        /// <param name="id">L'identifiant unique de l'offre.</param>
        /// <returns>L'offre avec son entreprise associée ou null si introuvable.</returns>
        public async Task<JobOffer?> GetByIdWithCompanyAsync(Guid id)
        {
            return await _context.JobOffers
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        /// <summary>
        /// Récupère toutes les offres d'emploi d'une entreprise, triées par date de création décroissante.
        /// </summary>
        /// <param name="companyId">L'identifiant unique de l'entreprise.</param>
        /// <returns>La liste des offres de l'entreprise.</returns>
        public async Task<IEnumerable<JobOffer>> GetByCompanyIdAsync(Guid companyId)
        {
            return await _context.JobOffers
                .AsNoTracking()
                .Where(j => j.CompanyId == companyId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère toutes les offres créées par un utilisateur spécifique.
        /// </summary>
        /// <param name="userId">L'identifiant unique du créateur.</param>
        /// <returns>La liste des offres créées par l'utilisateur.</returns>
        public async Task<IEnumerable<JobOffer>> GetByCreatedByIdAsync(Guid userId)
        {
            return await _context.JobOffers
                .AsNoTracking()
                .Where(j => j.CreatedById == userId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Recherche un doublon récent d'une offre pour le même utilisateur et la même entreprise dans une fenêtre de temps donnée.
        /// Utilisé pour prévenir la création involontaire d'offres dupliquées.
        /// </summary>
        /// <param name="companyId">L'identifiant de l'entreprise.</param>
        /// <param name="userId">L'identifiant du créateur.</param>
        /// <param name="title">Titre de l'offre à vérifier.</param>
        /// <param name="location">Localisation de l'offre (peut être null).</param>
        /// <param name="department">Département de l'offre (peut être null).</param>
        /// <param name="within">Fenêtre temporelle à partir de maintenant dans laquelle la duplication est détectée.</param>
        /// <returns>L'offre en doublon la plus récente, ou null si aucune duplication.</returns>
        public async Task<JobOffer?> FindRecentDuplicateAsync(
            Guid companyId,
            Guid userId,
            string title,
            string? location,
            string? department,
            TimeSpan within)
        {
            var since = DateTime.UtcNow.Subtract(within);
            var normalizedTitle = title.Trim();

            return await _context.JobOffers
                .AsNoTracking()
                .Where(j =>
                    j.CompanyId == companyId
                    && j.CreatedById == userId
                    && j.Title == normalizedTitle
                    && j.Location == location
                    && j.Department == department
                    && j.Status != JobOfferStatus.Archived
                    && j.CreatedAt >= since)
                .OrderByDescending(j => j.CreatedAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Récupère les offres d'emploi d'une entreprise avec pagination et filtrage optionnel par statut.
        /// Par défaut, les offres archivées sont exclues.
        /// </summary>
        /// <param name="companyId">L'identifiant de l'entreprise.</param>
        /// <param name="page">Le numéro de page (1-indexed).</param>
        /// <param name="pageSize">Le nombre d'éléments par page.</param>
        /// <param name="status">Filtre optionnel par statut (null = tous sauf Archived).</param>
        /// <returns>Un tuple contenant les items de la page et le nombre total d'enregistrements.</returns>
        public async Task<(IEnumerable<JobOffer> Items, int TotalCount)> GetByCompanyIdPagedAsync(Guid companyId, int page, int pageSize, JobOfferStatus? status = null)
        {
            var query = _context.JobOffers
                .AsNoTracking()
                .Include(j => j.Applications)
                .AsSplitQuery() // Utilise des requêtes SQL séparées pour éviter les produits cartésiens
                .Where(j => j.CompanyId == companyId);

            if (status.HasValue)
            {
                query = query.Where(j => j.Status == status.Value);
            }
            else
            {
                // Par défaut, ne pas afficher les offres archivées sauf si un statut est explicitement spécifié
                query = query.Where(j => j.Status != JobOfferStatus.Archived);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Retourne le nombre total d'offres non archivées pour une entreprise.
        /// </summary>
        /// <param name="companyId">L'identifiant unique de l'entreprise.</param>
        /// <returns>Le nombre d'offres actives et non archivées.</returns>
        public async Task<int> CountByCompanyIdAsync(Guid companyId)
        {
            return await _context.JobOffers.CountAsync(j => j.CompanyId == companyId && j.Status != JobOfferStatus.Archived);
        }

        /// <summary>
        /// Retourne le nombre d'offres non archivées créées par un utilisateur spécifique.
        /// </summary>
        /// <param name="userId">L'identifiant unique du créateur.</param>
        /// <returns>Le nombre d'offres actives créées par l'utilisateur.</returns>
        public async Task<int> CountByCreatedByIdAsync(Guid userId)
        {
            return await _context.JobOffers.CountAsync(j => j.CreatedById == userId && j.Status != JobOfferStatus.Archived);
        }

        /// <summary>
        /// Ajoute une nouvelle offre d'emploi en base de données.
        /// </summary>
        /// <param name="jobOffer">L'offre d'emploi à créer.</param>
        public async Task AddAsync(JobOffer jobOffer)
        {
            await _context.JobOffers.AddAsync(jobOffer);
        }

        /// <summary>
        /// Met à jour une offre d'emploi existante.
        /// </summary>
        /// <param name="jobOffer">L'offre modifiée à sauvegarder.</param>
        public async Task UpdateAsync(JobOffer jobOffer)
        {
            _context.JobOffers.Update(jobOffer);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Supprime une offre d'emploi de la base de données.
        /// </summary>
        /// <param name="jobOffer">L'offre à supprimer.</param>
        public async Task DeleteAsync(JobOffer jobOffer)
        {
            _context.JobOffers.Remove(jobOffer);
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
