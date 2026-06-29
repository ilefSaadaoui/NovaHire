using System;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repository d'accès aux données pour les utilisateurs.
    /// Gère les opérations CRUD et les recherches spécifiques à l'authentification.
    /// Plusieurs méthodes contournent volontairement le filtre multi-tenant pour permettre
    /// l'authentification globale (ex. : connexion, refresh token).
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="UserRepository"/>.
        /// </summary>
        /// <param name="context">Le contexte de base de données Entity Framework Core.</param>
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère un utilisateur par son identifiant unique, avec son entreprise et son département chargés.
        /// Respecte le filtre multi-tenant actif.
        /// </summary>
        /// <param name="id">L'identifiant unique de l'utilisateur.</param>
        /// <returns>L'utilisateur ou null s'il est introuvable dans le tenant courant.</returns>
        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Company)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Récupère un utilisateur par son adresse e-mail, tous tenants confondus.
        /// Contourne le filtre multi-tenant car nécessaire pour l'authentification globale.
        /// </summary>
        /// <param name="email">L'adresse e-mail de l'utilisateur (insensible à la casse).</param>
        /// <returns>L'utilisateur ou null si aucun compte ne correspond.</returns>
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .IgnoreQueryFilters() // Auth : contourne le filtre multi-tenant pour la connexion globale
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        /// <summary>
        /// Récupère un utilisateur par son refresh token, avec son entreprise chargée.
        /// Contourne le filtre multi-tenant pour permettre le renouvellement de token inter-tenant.
        /// </summary>
        /// <param name="refreshToken">Le refresh token JWT à rechercher.</param>
        /// <returns>L'utilisateur correspondant ou null si le token est invalide ou expiré.</returns>
        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .IgnoreQueryFilters() // Auth : le refresh token doit être recherché globalement
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        /// <summary>
        /// Récupère un utilisateur par e-mail avec son entreprise associée, tous tenants confondus.
        /// Utilisé lors de la connexion pour accéder aux informations de l'entreprise dans le JWT.
        /// </summary>
        /// <param name="email">L'adresse e-mail de l'utilisateur (insensible à la casse).</param>
        /// <returns>L'utilisateur avec son entreprise ou null si introuvable.</returns>
        public async Task<User?> GetByEmailWithCompanyAsync(string email)
        {
            return await _context.Users
                .IgnoreQueryFilters() // Auth : contourne le filtre multi-tenant pour charger l'entreprise
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        /// <summary>
        /// Vérifie si une adresse e-mail est déjà utilisée par un compte existant, tous tenants confondus.
        /// Contourne le filtre multi-tenant pour garantir l'unicité globale des e-mails.
        /// </summary>
        /// <param name="email">L'adresse e-mail à vérifier (insensible à la casse).</param>
        /// <returns><c>true</c> si l'e-mail est déjà pris ; sinon <c>false</c>.</returns>
        public async Task<bool> IsEmailTakenAsync(string email)
        {
            return await _context.Users
                .IgnoreQueryFilters() // Auth : unicité e-mail doit être vérifiée globalement
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        /// <summary>
        /// Ajoute un nouvel utilisateur en base de données.
        /// </summary>
        /// <param name="user">L'utilisateur à créer.</param>
        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        /// <summary>
        /// Met à jour un utilisateur existant en base de données.
        /// </summary>
        /// <param name="user">L'utilisateur modifié à sauvegarder.</param>
        public Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Récupère tous les utilisateurs appartenant à une entreprise donnée.
        /// </summary>
        /// <param name="companyId">L'identifiant de l'entreprise.</param>
        /// <returns>La liste des utilisateurs de l'entreprise.</returns>
        public async Task<IEnumerable<User>> GetByCompanyIdAsync(Guid companyId)
        {
            return await _context.Users
                .Where(u => u.CompanyId == companyId)
                .ToListAsync();
        }

        /// <summary>
        /// Persiste toutes les modifications en attente dans la base de données.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}