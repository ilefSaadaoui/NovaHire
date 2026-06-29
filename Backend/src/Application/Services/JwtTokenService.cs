using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services
{
    /// <summary>
    /// Service de gestion des jetons JWT (JSON Web Tokens) pour NovaHire.
    /// Gère la génération, la validation et l'extraction des claims des Access Tokens et Refresh Tokens.
    /// S'appuie sur la librairie <c>Microsoft.IdentityModel.JsonWebTokens</c> avec l'algorithme HMAC-SHA256.
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpirationMinutes;
        private readonly int _refreshTokenExpirationDays;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="JwtTokenService"/>.
        /// Lit toutes les clés de configuration JWT depuis <c>appsettings.json</c> et valide leur présence.
        /// </summary>
        /// <param name="configuration">La configuration de l'application contenant les paramètres de la section 'JwtSettings'.</param>
        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;

            // Récupération des paramètres JWT avec contrôles de nullabilité stricts
            _secretKey = _configuration["JwtSettings:SecretKey"]
                ?? throw new ArgumentNullException("JwtSettings:SecretKey", "JWT SecretKey is required in configuration");
            _issuer = _configuration["JwtSettings:Issuer"]
                ?? throw new ArgumentNullException("JwtSettings:Issuer", "JWT Issuer is required in configuration");
            _audience = _configuration["JwtSettings:Audience"]
                ?? throw new ArgumentNullException("JwtSettings:Audience", "JWT Audience is required in configuration");

            // Valeurs par défaut : Access Token = 60 min, Refresh Token = 7 jours
            _accessTokenExpirationMinutes = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "60");
            _refreshTokenExpirationDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");

            // Exiger une clé secrète d'au moins 256 bits (32 octets) pour respecter le standard HMAC-SHA256
            if (_secretKey.Length < 32)
            {
                throw new ArgumentException("JWT SecretKey must be at least 32 characters long");
            }
        }

        /// <summary>
        /// Génère un Access Token JWT signé contenant les claims d'identité de l'utilisateur.
        /// Le token est signé avec HMAC-SHA256 et encapsule : identifiant, email, prénom, nom, rôle et CompanyId (multitenancy).
        /// </summary>
        /// <param name="user">L'entité utilisateur dont les données seront encodées dans le token.</param>
        /// <returns>Le token JWT sous forme de chaîne encodage Base64URL.</returns>
        public string GenerateAccessToken(User user)
        {
            var claims = new Dictionary<string, object>
            {
                [ClaimTypes.NameIdentifier] = user.Id.ToString(),
                [ClaimTypes.Email] = user.Email,
                [ClaimTypes.GivenName] = user.FirstName,
                [ClaimTypes.Surname] = user.LastName,
                [ClaimTypes.Role] = user.Role.ToString(),
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString()
            };

            // Le CompanyId est inclus dans le token pour permettre le filtrage multitenancy côté serveur
            if (user.CompanyId.HasValue)
            {
                claims["CompanyId"] = user.CompanyId.Value.ToString();
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var handler = new JsonWebTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = claims,
                Issuer = _issuer,
                Audience = _audience,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
                SigningCredentials = credentials
            };

            return handler.CreateToken(tokenDescriptor);
        }

        /// <summary>
        /// Génère un Refresh Token opaque de 64 octets en utilisant le générateur cryptographique sécurisé du système.
        /// Ce token n'est pas un JWT ; il est stocké en base de données et ne peut être utilisé qu'une seule fois.
        /// </summary>
        /// <returns>Une chaîne opaque en Base64 représentant le Refresh Token.</returns>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// Valide un Access Token JWT de manière asynchrone et retourne les claims décodés si valide.
        /// </summary>
        /// <param name="token">Le token JWT à valider.</param>
        /// <returns>Le <see cref="ClaimsPrincipal"/> si le token est valide, <c>null</c> sinon.</returns>
        public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var key = Encoding.UTF8.GetBytes(_secretKey);
            var handler = new JsonWebTokenHandler();

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // Pas de tolérance sur l'expiration
                };

                var validationResult = await handler.ValidateTokenAsync(token, validationParameters);

                if (validationResult.IsValid && validationResult.SecurityToken != null)
                {
                    var claimsIdentity = new ClaimsIdentity(validationResult.ClaimsIdentity.Claims);
                    return new ClaimsPrincipal(claimsIdentity);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Valide un Access Token JWT de manière synchrone (contexte hérité).
        /// </summary>
        /// <param name="token">Le token JWT à valider.</param>
        /// <returns>Le <see cref="ClaimsPrincipal"/> si valide, <c>null</c> sinon.</returns>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var key = Encoding.UTF8.GetBytes(_secretKey);
            var handler = new JsonWebTokenHandler();

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                // Appel asynchrone bloqué de manière synchrone : non-idéal mais requis pour les contextes
                // hérités non-async. À migrer vers l'API async à terme.
                var validationTask = handler.ValidateTokenAsync(token, validationParameters);
                validationTask.Wait();
                var validationResult = validationTask.Result;

                if (validationResult.IsValid && validationResult.SecurityToken != null)
                {
                    var jsonWebToken = handler.ReadJsonWebToken(token);
                    var claimsIdentity = new ClaimsIdentity(jsonWebToken.Claims);
                    return new ClaimsPrincipal(claimsIdentity);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extrait l'identifiant unique de l'utilisateur (GUID) à partir d'un token JWT valide.
        /// </summary>
        /// <param name="token">Le token JWT de l'utilisateur connecté.</param>
        /// <returns>Le GUID de l'utilisateur, ou <c>null</c> si le token est invalide ou le claim absent.</returns>
        public Guid? GetUserIdFromToken(string token)
        {
            var principal = ValidateToken(token);
            var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }

            return null;
        }

        /// <summary>
        /// Extrait l'adresse e-mail de l'utilisateur depuis un token JWT valide.
        /// </summary>
        /// <param name="token">Le token JWT de l'utilisateur connecté.</param>
        /// <returns>L'adresse e-mail, ou <c>null</c> si le token est invalide.</returns>
        public string? GetEmailFromToken(string token)
        {
            var principal = ValidateToken(token);
            return principal?.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Vérifie si un token JWT est expiré sans en valider la signature.
        /// Utile pour décider si le renouvellement est nécessaire avant d'appeler l'API.
        /// </summary>
        /// <param name="token">Le token JWT à inspecter.</param>
        /// <returns><c>true</c> si le token est expiré ou invalide, <c>false</c> s'il est encore actif.</returns>
        public bool IsTokenExpired(string token)
        {
            try
            {
                var handler = new JsonWebTokenHandler();
                var jsonWebToken = handler.ReadJsonWebToken(token);
                return jsonWebToken.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Calcule la date et heure d'expiration du prochain Access Token généré.
        /// </summary>
        /// <returns>La date UTC d'expiration de l'Access Token.</returns>
        public DateTime GetAccessTokenExpiration()
        {
            return DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes);
        }

        /// <summary>
        /// Calcule la date d'expiration du Refresh Token, en multipliant la durée par 4 en mode "Se souvenir de moi".
        /// </summary>
        /// <param name="rememberMe">Si <c>true</c>, la durée de vie est étendue (x4 jours).</param>
        /// <returns>La date UTC d'expiration du Refresh Token.</returns>
        public DateTime GetRefreshTokenExpiration(bool rememberMe = false)
        {
            // Si "remember me", étendre la durée de vie du refresh token
            var days = rememberMe ? _refreshTokenExpirationDays * 4 : _refreshTokenExpirationDays;
            return DateTime.UtcNow.AddDays(days);
        }
    }
}