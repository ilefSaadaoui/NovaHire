using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IJobOfferRepository
    {
        Task<JobOffer?> GetByIdAsync(Guid id);
        Task<JobOffer?> GetByIdWithCompanyAsync(Guid id);
        Task<IEnumerable<JobOffer>> GetByCompanyIdAsync(Guid companyId);
        Task<IEnumerable<JobOffer>> GetByCreatedByIdAsync(Guid userId);
        Task<(IEnumerable<JobOffer> Items, int TotalCount)> GetByCompanyIdPagedAsync(Guid companyId, int page, int pageSize, JobOfferStatus? status = null);
        Task<JobOffer?> FindRecentDuplicateAsync(Guid companyId, Guid userId, string title, string? location, string? department, TimeSpan within);
        Task<int> CountByCompanyIdAsync(Guid companyId);
        Task<int> CountByCreatedByIdAsync(Guid userId);
        Task AddAsync(JobOffer jobOffer);
        Task UpdateAsync(JobOffer jobOffer);
        Task DeleteAsync(JobOffer jobOffer);
        Task SaveChangesAsync();
    }
}
