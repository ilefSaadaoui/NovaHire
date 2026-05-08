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
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public JobApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JobApplication?> GetByIdAsync(Guid id)
        {
            return await _context.JobApplications.FindAsync(id);
        }

        public async Task<JobApplication?> GetByIdWithJobOfferAsync(Guid id)
        {
            return await _context.JobApplications
                .Include(ja => ja.JobOffer)
                .Include(ja => ja.Candidate)
                .Include(ja => ja.AIAnalysis)
                    .ThenInclude(ai => ai!.ExtractedData)
                .FirstOrDefaultAsync(ja => ja.Id == id);
        }

        public async Task<JobApplication?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.JobApplications
                .Include(ja => ja.JobOffer)
                .Include(ja => ja.Candidate)
                .Include(ja => ja.AIAnalysis)
                    .ThenInclude(ai => ai!.ExtractedData)
                .Include(ja => ja.Comments)
                    .ThenInclude(c => c.Author)
                .Include(ja => ja.Notifications)
                .Include(ja => ja.Ratings)
                .FirstOrDefaultAsync(ja => ja.Id == id);
        }

        public async Task<IEnumerable<JobApplication>> GetByJobOfferIdAsync(Guid jobOfferId)
        {
            return await _context.JobApplications
                .AsNoTracking()
                .Include(ja => ja.Candidate)
                .Include(ja => ja.AIAnalysis)
                .Include(ja => ja.Ratings)
                .Where(ja => ja.JobOfferId == jobOfferId)
                .OrderByDescending(ja => ja.AppliedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobApplication>> GetByCompanyIdAsync(Guid companyId)
        {
            return await _context.JobApplications
                .AsNoTracking()
                .Include(ja => ja.JobOffer)
                .Include(ja => ja.Candidate)
                .Include(ja => ja.AIAnalysis)
                .Include(ja => ja.Ratings)
                .Where(ja => ja.JobOffer.CompanyId == companyId)
                .OrderByDescending(ja => ja.AppliedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobApplication>> GetByCreatedByIdAsync(Guid userId)
        {
            return await _context.JobApplications
                .AsNoTracking()
                .Include(ja => ja.JobOffer)
                .Include(ja => ja.Candidate)
                .Include(ja => ja.AIAnalysis)
                .Include(ja => ja.Ratings)
                .Where(ja => ja.JobOffer.CreatedById == userId)
                .OrderByDescending(ja => ja.AppliedAt)
                .ToListAsync();
        }

        public async Task<int> CountByCompanyIdAsync(Guid companyId)
        {
            return await _context.JobApplications
                .Include(ja => ja.JobOffer)
                .CountAsync(ja => ja.JobOffer.CompanyId == companyId);
        }

        public async Task<int> CountByCreatedByIdAsync(Guid userId)
        {
            return await _context.JobApplications
                .Include(ja => ja.JobOffer)
                .CountAsync(ja => ja.JobOffer.CreatedById == userId);
        }

        public async Task AddAsync(JobApplication jobApplication)
        {
            await _context.JobApplications.AddAsync(jobApplication);
        }

        public async Task UpdateAsync(JobApplication jobApplication)
        {
            _context.JobApplications.Update(jobApplication);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(JobApplication jobApplication)
        {
            _context.JobApplications.Remove(jobApplication);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
