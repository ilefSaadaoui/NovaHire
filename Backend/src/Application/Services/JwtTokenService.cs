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
    /// Service de gestion des JWT tokens
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpirationMinutes;
        private readonly int _refreshTokenExpirationDays;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;

            // Add null checks with meaningful exceptions
            _secretKey = _configuration["JwtSettings:SecretKey"]
                ?? throw new ArgumentNullException("JwtSettings:SecretKey", "JWT SecretKey is required in configuration");
            _issuer = _configuration["JwtSettings:Issuer"]
                ?? throw new ArgumentNullException("JwtSettings:Issuer", "JWT Issuer is required in configuration");
            _audience = _configuration["JwtSettings:Audience"]
                ?? throw new ArgumentNullException("JwtSettings:Audience", "JWT Audience is required in configuration");

            // Parse with default values if not provided
            _accessTokenExpirationMinutes = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "60");
            _refreshTokenExpirationDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");

            // Validation de la clé secrète
            if (_secretKey.Length < 32)
            {
                throw new ArgumentException("JWT SecretKey must be at least 32 characters long");
            }
        }

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

            // Ajouter le CompanyId si présent (pour le multi-tenant)
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

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

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
                    ClockSkew = TimeSpan.Zero
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

                // Use ValidateTokenAsync instead of the synchronous version
                // For synchronous contexts, we can call .GetAwaiter().GetResult() but that's not ideal
                // Better to make the calling methods async, but for now we'll use a workaround
                var validationTask = handler.ValidateTokenAsync(token, validationParameters);
                validationTask.Wait(); // This blocks - not ideal but works
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

        public string? GetEmailFromToken(string token)
        {
            var principal = ValidateToken(token);
            return principal?.FindFirst(ClaimTypes.Email)?.Value;
        }

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

        public DateTime GetAccessTokenExpiration()
        {
            return DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes);
        }

        public DateTime GetRefreshTokenExpiration(bool rememberMe = false)
        {
            // Si "remember me", étendre la durée de vie du refresh token
            var days = rememberMe ? _refreshTokenExpirationDays * 4 : _refreshTokenExpirationDays;
            return DateTime.UtcNow.AddDays(days);
        }
    }
}