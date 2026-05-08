using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IExportService
    {
        Task<byte[]> GenerateJobOfferReportPdfAsync(JobOffer offer, IEnumerable<JobApplication> applications);
        Task<byte[]> GenerateJobOfferReportExcelAsync(JobOffer offer, IEnumerable<JobApplication> applications);
        Task<byte[]> GenerateCandidateProfilePdfAsync(JobApplication application);
    }
}
