using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repository pour les sociétés
    /// </summary>
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Company?> GetByIdAsync(Guid id)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Company?> GetByIdWithUsersAsync(Guid id)
        {
            return await _context.Companies
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Company?> GetByNameAsync(string name)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<bool> IsNameTakenAsync(string name)
        {
            return await _context.Companies.AnyAsync(c => c.Name == name);
        }

        public async Task<IEnumerable<Company>> GetAllActiveAsync()
        {
            return await _context.Companies
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task AddAsync(Company company)
        {
            await _context.Companies.AddAsync(company);
        }

        public Task UpdateAsync(Company company)
        {
            _context.Companies.Update(company);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // Méthodes spécifiques pour l'authentification
        public async Task<Company?> GetActiveCompanyByIdAsync(Guid id)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        public Task<bool> CanAddUserAsync(Guid companyId)
        {
            return Task.FromResult(true); // Système d'abonnement supprimé : Illimité
        }

        public async Task<int> GetUserCountAsync(Guid companyId)
        {
            return await _context.Users
                .CountAsync(u => u.CompanyId == companyId);
        }
    }
}