using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    /// <summary>
    /// Interface du repository pour les sociétés
    /// </summary>
    public interface ICompanyRepository
    {
        Task<Company?> GetByIdAsync(Guid id);
        Task<Company?> GetByIdWithUsersAsync(Guid id);
        Task<Company?> GetByNameAsync(string name);
        Task<bool> IsNameTakenAsync(string name);
        Task<IEnumerable<Company>> GetAllActiveAsync();
        Task AddAsync(Company company);
        Task UpdateAsync(Company company);
        Task SaveChangesAsync();

        // Méthodes spécifiques pour l'authentification
        Task<Company?> GetActiveCompanyByIdAsync(Guid id);
        Task<bool> CanAddUserAsync(Guid companyId);
        Task<int> GetUserCountAsync(Guid companyId);
    }
}