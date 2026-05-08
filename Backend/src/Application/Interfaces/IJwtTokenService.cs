using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    /// <summary>
    /// Interface du service de gestion des JWT tokens
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Générer un access token JWT
        /// </summary>
        string GenerateAccessToken(User user);

        /// <summary>
        /// Générer un refresh token
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Valider et récupérer les claims d'un token (version synchrone)
        /// </summary>
        ClaimsPrincipal? ValidateToken(string token); // Made nullable

        /// <summary>
        /// Valider et récupérer les claims d'un token (version asynchrone)
        /// </summary>
        Task<ClaimsPrincipal?> ValidateTokenAsync(string token); // Added async version

        /// <summary>
        /// Extraire l'ID utilisateur d'un token
        /// </summary>
        Guid? GetUserIdFromToken(string token);

        /// <summary>
        /// Extraire l'email d'un token
        /// </summary>
        string? GetEmailFromToken(string token); // Made nullable

        /// <summary>
        /// Vérifier si un token est expiré
        /// </summary>
        bool IsTokenExpired(string token);

        /// <summary>
        /// Calculer la date d'expiration d'un access token
        /// </summary>
        DateTime GetAccessTokenExpiration();

        /// <summary>
        /// Calculer la date d'expiration d'un refresh token
        /// </summary>
        DateTime GetRefreshTokenExpiration(bool rememberMe = false);
    }
}