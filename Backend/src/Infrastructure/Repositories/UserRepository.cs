using System;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repository pour les utilisateurs
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id) // Make return type nullable
        {
            return await _context.Users
                .Include(u => u.Company)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .IgnoreQueryFilters() // Auth: must bypass multi-tenant filter
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .IgnoreQueryFilters() // Auth: must bypass multi-tenant filter
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        public async Task<User?> GetByEmailWithCompanyAsync(string email)
        {
            return await _context.Users
                .IgnoreQueryFilters() // Auth: must bypass multi-tenant filter
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> IsEmailTakenAsync(string email)
        {
            return await _context.Users
                .IgnoreQueryFilters() // Auth: must bypass multi-tenant filter
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<User>> GetByCompanyIdAsync(Guid companyId)
        {
            return await _context.Users
                .Where(u => u.CompanyId == companyId)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}