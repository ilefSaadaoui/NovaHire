using System;
using System.Security.Claims;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services
{
    /// <summary>
    /// Service fournissant les informations de l'utilisateur actuellement authentifié.
    /// Extrait les claims d'identité (UserId, CompanyId, Rôle) directement depuis le contexte HTTP
    /// en lisant les claims du token JWT décodé par le middleware d'authentification.
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="CurrentUserService"/>.
        /// </summary>
        /// <param name="httpContextAccessor">Fournisseur du contexte HTTP courant.</param>
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Identifiant unique (GUID) de l'utilisateur connecté, extrait du claim <c>NameIdentifier</c> du token JWT.
        /// Retourne <c>null</c> si aucune session active n'est détectée.
        /// </summary>
        public Guid? UserId
        {
            get
            {
                var id = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return string.IsNullOrEmpty(id) ? null : Guid.Parse(id);
            }
        }

        /// <summary>
        /// Identifiant unique (GUID) de l'entreprise de l'utilisateur connecté, extrait du claim personnalisé <c>CompanyId</c>.
        /// Permet d'appliquer automatiquement le filtre multitenancy sur toutes les requêtes de base de données.
        /// Retourne <c>null</c> si l'utilisateur est un SuperAdmin (pas rattaché à une entreprise).
        /// </summary>
        public Guid? CompanyId
        {
            get
            {
                var companyId = _httpContextAccessor.HttpContext?.User?.FindFirst("CompanyId")?.Value;
                return string.IsNullOrEmpty(companyId) ? null : Guid.Parse(companyId);
            }
        }

        /// <summary>
        /// Rôle de l'utilisateur connecté (ex: "Recruiter", "CompanyAdmin", "SuperAdmin"),
        /// extrait du claim <c>Role</c> du token JWT.
        /// </summary>
        public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

        /// <summary>
        /// Indique si l'utilisateur courant possède une session authentifiée active.
        /// </summary>
        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
