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
    public class JobOfferRepository : IJobOfferRepository
    {
        private readonly ApplicationDbContext _context;

        public JobOfferRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JobOffer?> GetByIdAsync(Guid id)
        {
            return await _context.JobOffers
                .Include(j => j.Applications)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<JobOffer?> GetByIdWithCompanyAsync(Guid id)
        {
            return await _context.JobOffers
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<IEnumerable<JobOffer>> GetByCompanyIdAsync(Guid companyId)
        {
            return await _context.JobOffers
                .AsNoTracking()
                .Where(j => j.CompanyId == companyId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobOffer>> GetByCreatedByIdAsync(Guid userId)
        {
            return await _context.JobOffers
                .AsNoTracking()
                .Where(j => j.CreatedById == userId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<JobOffer> Items, int TotalCount)> GetByCompanyIdPagedAsync(Guid companyId, int page, int pageSize, JobOfferStatus? status = null)
        {
            var query = _context.JobOffers.Include(j => j.Applications).Where(j => j.CompanyId == companyId);

            if (status.HasValue)
            {
                query = query.Where(j => j.Status == status.Value);
            }
            else
            {
                // Par défaut, ne pas afficher les offres archivées sauf si explicitement demandé par statut
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

        public async Task<int> CountByCompanyIdAsync(Guid companyId)
        {
            return await _context.JobOffers.CountAsync(j => j.CompanyId == companyId && j.Status != JobOfferStatus.Archived);
        }

        public async Task<int> CountByCreatedByIdAsync(Guid userId)
        {
            return await _context.JobOffers.CountAsync(j => j.CreatedById == userId && j.Status != JobOfferStatus.Archived);
        }

        public async Task AddAsync(JobOffer jobOffer)
        {
            await _context.JobOffers.AddAsync(jobOffer);
        }

        public async Task UpdateAsync(JobOffer jobOffer)
        {
            _context.JobOffers.Update(jobOffer);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(JobOffer jobOffer)
        {
            _context.JobOffers.Remove(jobOffer);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
