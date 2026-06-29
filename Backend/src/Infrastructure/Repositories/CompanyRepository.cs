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
    /// Repository d'accès aux données pour les entreprises (tenants).
    /// Fournit les opérations CRUD ainsi que des méthodes utilitaires pour la gestion multi-tenant.
    /// </summary>
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="CompanyRepository"/>.
        /// </summary>
        /// <param name="context">Le contexte de base de données Entity Framework Core.</param>
        public CompanyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère une entreprise par son identifiant unique.
        /// </summary>
        /// <param name="id">L'identifiant unique de l'entreprise.</param>
        /// <returns>L'entreprise ou null si introuvable.</returns>
        public async Task<Company?> GetByIdAsync(Guid id)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Récupère une entreprise par son identifiant avec la liste de ses utilisateurs chargée.
        /// </summary>
        /// <param name="id">L'identifiant unique de l'entreprise.</param>
        /// <returns>L'entreprise avec ses utilisateurs ou null si introuvable.</returns>
        public async Task<Company?> GetByIdWithUsersAsync(Guid id)
        {
            return await _context.Companies
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Récupère une entreprise par son nom exact.
        /// </summary>
        /// <param name="name">Le nom exact de l'entreprise (sensible à la casse).</param>
        /// <returns>L'entreprise correspondante ou null si introuvable.</returns>
        public async Task<Company?> GetByNameAsync(string name)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.Name == name);
        }

        /// <summary>
        /// Vérifie si un nom d'entreprise est déjà utilisé.
        /// Permet de garantir l'unicité des noms lors de la création d'un nouveau tenant.
        /// </summary>
        /// <param name="name">Le nom à vérifier.</param>
        /// <returns><c>true</c> si le nom est déjà pris ; sinon <c>false</c>.</returns>
        public async Task<bool> IsNameTakenAsync(string name)
        {
            return await _context.Companies.AnyAsync(c => c.Name == name);
        }

        /// <summary>
        /// Récupère toutes les entreprises actuellement actives (non désactivées).
        /// </summary>
        /// <returns>La liste des entreprises dont la propriété <c>IsActive</c> est vraie.</returns>
        public async Task<IEnumerable<Company>> GetAllActiveAsync()
        {
            return await _context.Companies
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Ajoute une nouvelle entreprise en base de données.
        /// </summary>
        /// <param name="company">L'entreprise à créer.</param>
        public async Task AddAsync(Company company)
        {
            await _context.Companies.AddAsync(company);
        }

        /// <summary>
        /// Met à jour une entreprise existante en base de données.
        /// </summary>
        /// <param name="company">L'entreprise modifiée à sauvegarder.</param>
        public Task UpdateAsync(Company company)
        {
            _context.Companies.Update(company);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Persiste toutes les modifications en attente dans la base de données.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Récupère une entreprise active par son identifiant.
        /// Utilisé lors de l'authentification pour s'assurer que le tenant est toujours actif.
        /// </summary>
        /// <param name="id">L'identifiant unique de l'entreprise.</param>
        /// <returns>L'entreprise si elle existe et est active ; sinon null.</returns>
        public async Task<Company?> GetActiveCompanyByIdAsync(Guid id)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        /// <summary>
        /// Indique si l'entreprise peut ajouter un utilisateur supplémentaire.
        /// Le système d'abonnement ayant été supprimé, cette méthode retourne toujours <c>true</c> (illimité).
        /// </summary>
        /// <param name="companyId">L'identifiant unique de l'entreprise.</param>
        /// <returns>Toujours <c>true</c> : aucune limite d'utilisateurs n'est appliquée.</returns>
        public Task<bool> CanAddUserAsync(Guid companyId)
        {
            // Système d'abonnement supprimé : le nombre d'utilisateurs est illimité
            return Task.FromResult(true);
        }

        /// <summary>
        /// Retourne le nombre total d'utilisateurs rattachés à une entreprise.
        /// </summary>
        /// <param name="companyId">L'identifiant unique de l'entreprise.</param>
        /// <returns>Le nombre d'utilisateurs de l'entreprise.</returns>
        public async Task<int> GetUserCountAsync(Guid companyId)
        {
            return await _context.Users
                .CountAsync(u => u.CompanyId == companyId);
        }
    }
}