using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IJobApplicationRepository
    {
        Task<JobApplication?> GetByIdAsync(Guid id);
        Task<JobApplication?> GetByIdWithJobOfferAsync(Guid id);
        Task<JobApplication?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<JobApplication>> GetByJobOfferIdAsync(Guid jobOfferId);
        Task<IEnumerable<JobApplication>> GetByCompanyIdAsync(Guid companyId);
        Task<IEnumerable<JobApplication>> GetByCreatedByIdAsync(Guid userId);
        Task<int> CountByCompanyIdAsync(Guid companyId);
        Task<int> CountByCreatedByIdAsync(Guid userId);
        Task AddAsync(JobApplication jobApplication);
        Task UpdateAsync(JobApplication jobApplication);
        Task DeleteAsync(JobApplication jobApplication);
        Task SaveChangesAsync();
    }
}
