using System;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    /// <summary>
    /// Interface du repository pour les utilisateurs
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task<User?> GetByEmailWithCompanyAsync(string email);
        Task<bool> IsEmailTakenAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task<IEnumerable<User>> GetByCompanyIdAsync(Guid companyId);
        Task SaveChangesAsync();
    }
}