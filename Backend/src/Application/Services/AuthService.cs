using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    /// <summary>
    /// Service d'authentification
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IJwtTokenService jwtTokenService,
            IEmailService emailService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Vérifier si l'email existe déjà
                if (await _userRepository.IsEmailTakenAsync(registerDto.Email))
                {
                    throw new InvalidOperationException("Cet email est déjà utilisé");
                }

                // Parser le rôle
                if (!Enum.TryParse<UserRole>(registerDto.Role, out var role))
                {
                    throw new InvalidOperationException("Rôle invalide");
                }

                // Vérifier que CompanyId est fourni
                if (!registerDto.CompanyId.HasValue)
                {
                    throw new InvalidOperationException("Un CompanyId est requis");
                }

                // Vérifier que la société existe si fournie
                Company? company = null;
                if (registerDto.CompanyId.HasValue)
                {
                    company = await _companyRepository.GetActiveCompanyByIdAsync(registerDto.CompanyId.Value);

                    if (company == null)
                    {
                        throw new InvalidOperationException("Société introuvable ou inactive");
                    }

                    // Vérifier la limite d'utilisateurs
                    var canAddUser = await _companyRepository.CanAddUserAsync(registerDto.CompanyId.Value);
                    if (!canAddUser)
                    {
                        throw new InvalidOperationException("Limite d'utilisateurs atteinte pour cette société");
                    }
                }

                // Créer l'utilisateur
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Email = registerDto.Email.ToLower(),
                    PhoneNumber = registerDto.PhoneNumber,
                    CompanyId = registerDto.CompanyId,
                    Role = role,
                    PasswordHash = HashPassword(registerDto.Password),
                    EmailConfirmationToken = GenerateRandomToken(),
                    IsActive = true,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation($"Nouvel utilisateur créé: {user.Email}");

                // Envoyer l'email de confirmation
                var emailSent = await _emailService.SendEmailConfirmationAsync(user.Email, user.EmailConfirmationToken);
                if (!emailSent)
                {
                    _logger.LogWarning($"Échec de l'envoi de l'email de confirmation à: {user.Email}");
                }

                // Générer les tokens même si l'email n'est pas confirmé
                return await GenerateAuthResponse(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'inscription");
                throw;
            }
        }

        public async Task<AuthResponseDto> RegisterCompanyAsync(CompanyRegisterDto registerDto)
        {
            try
            {
                // 1. Vérifications initiales
                if (await _userRepository.IsEmailTakenAsync(registerDto.AdminEmail))
                {
                    throw new InvalidOperationException("L'email de l'administrateur est déjà utilisé");
                }

                if (await _companyRepository.IsNameTakenAsync(registerDto.CompanyName))
                {
                    throw new InvalidOperationException("Le nom de cette entreprise est déjà utilisé");
                }

                // 3. Créer la société
                var company = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = registerDto.CompanyName,
                    Industry = registerDto.Industry,
                    Size = registerDto.EmployeesRange,
                    ContactEmail = registerDto.AdminEmail,
                    IsActive = false,
                    Status = CompanyStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _companyRepository.AddAsync(company);
                // On enregistre les changements pour avoir l'ID de la société si nécessaire par les repositories
                await _companyRepository.SaveChangesAsync();

                // 4. Créer l'administrateur de l'entreprise
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = registerDto.AdminFirstName,
                    LastName = registerDto.AdminLastName,
                    Email = registerDto.AdminEmail.ToLower(),
                    CompanyId = company.Id,
                    Role = UserRole.CompanyAdmin,
                    PasswordHash = HashPassword(registerDto.AdminPassword),
                    EmailConfirmationToken = GenerateRandomToken(),
                    IsActive = false,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(adminUser);

                // 5. Créer un recruteur optionnel si l'email est fourni
                if (!string.IsNullOrWhiteSpace(registerDto.RecruiterEmail))
                {
                    // Vérifier si l'email du recruteur est différent de l'admin
                    if (registerDto.RecruiterEmail.ToLower() != registerDto.AdminEmail.ToLower())
                    {
                        var tempPassword = GenerateTemporaryPassword();
                        var recruiterUser = new User
                        {
                            Id = Guid.NewGuid(),
                            FirstName = "Recruteur",
                            LastName = company.Name,
                            Email = registerDto.RecruiterEmail.ToLower(),
                            CompanyId = company.Id,
                            Role = UserRole.Recruiter,
                            PasswordHash = HashPassword(tempPassword),
                            EmailConfirmationToken = GenerateRandomToken(),
                            IsActive = true,
                            EmailConfirmed = false,
                            MustChangePassword = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _userRepository.AddAsync(recruiterUser);

                        // Envoyer l'invitation par email avec les identifiants temporaires
                        var adminFullName = $"{registerDto.AdminFirstName} {registerDto.AdminLastName}";
                        await _emailService.SendRecruiterInvitationAsync(
                            recruiterUser.Email, tempPassword, company.Name, adminFullName);

                        _logger.LogInformation($"Recruteur secondaire créé et invitation envoyée: {recruiterUser.Email}");
                    }
                }

                await _userRepository.SaveChangesAsync();

                _logger.LogInformation($"Société créée: {company.Name} avec Admin: {adminUser.Email}");

                // 6. Envoyer l'email de confirmation à l'admin
                var emailSent = await _emailService.SendEmailConfirmationAsync(adminUser.Email, adminUser.EmailConfirmationToken);
                if (!emailSent)
                {
                    _logger.LogWarning($"Échec de l'envoi de l'email de confirmation admin à: {adminUser.Email}");
                }

                // 7. Retourner une réponse indiquant que l'approbation est requise
                return new AuthResponseDto
                {
                    UserId = adminUser.Id,
                    Email = adminUser.Email,
                    FirstName = adminUser.FirstName,
                    LastName = adminUser.LastName,
                    Role = adminUser.Role.ToString(),
                    CompanyId = company.Id,
                    CompanyName = company.Name,
                    AccessToken = string.Empty,
                    RefreshToken = string.Empty,
                    RequiresApproval = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement de l'entreprise");
                throw;
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userRepository.GetByEmailWithCompanyAsync(loginDto.Email);

                if (user == null)
                {
                    throw new UnauthorizedAccessException("Email ou mot de passe incorrect");
                }

                if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Email ou mot de passe incorrect");
                }

                if (!user.IsActive)
                {
                    throw new UnauthorizedAccessException("Ce compte est désactivé");
                }

                // Le système d'abonnement est supprimé. La vérification est toujours positive.

                // Mettre à jour la date de dernière connexion
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation($"Connexion réussie pour: {user.Email}");

                return await GenerateAuthResponse(user, loginDto.RememberMe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la connexion");
                throw;
            }
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var user = await _userRepository.GetByRefreshTokenAsync(refreshTokenDto.RefreshToken);

                if (user == null || !user.IsRefreshTokenValid())
                {
                    throw new UnauthorizedAccessException("Refresh token invalide ou expiré");
                }

                if (!user.IsActive)
                {
                    throw new UnauthorizedAccessException("Ce compte est désactivé");
                }

                // Optional: Validate the access token if provided (now it's safe because it's nullable)
                if (!string.IsNullOrEmpty(refreshTokenDto.AccessToken))
                {
                    var principal = await _jwtTokenService.ValidateTokenAsync(refreshTokenDto.AccessToken);
                    if (principal == null)
                    {
                        _logger.LogWarning("Access token invalide lors du refresh");
                        // Continue anyway - refresh token is what matters
                    }
                }

                _logger.LogInformation($"Refresh token pour: {user.Email}");

                return await GenerateAuthResponse(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du refresh token");
                throw;
            }
        }

        public async Task LogoutAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiry = null;
                    await _userRepository.UpdateAsync(user);
                    await _userRepository.SaveChangesAsync();

                    _logger.LogInformation($"Déconnexion pour: {user.Email}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la déconnexion");
                throw;
            }
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(forgotPasswordDto.Email);

                if (user == null)
                {
                    // Ne pas révéler si l'email existe ou non (sécurité)
                    return true;
                }

                user.PasswordResetToken = GenerateRandomToken();
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                var emailSent = await _emailService.SendPasswordResetAsync(user.Email, user.PasswordResetToken);

                if (emailSent)
                {
                    _logger.LogInformation($"Email de réinitialisation envoyé à: {user.Email}");
                }
                else
                {
                    _logger.LogError($"Échec technique de l'envoi de l'email de réinitialisation à: {user.Email}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la demande de réinitialisation");
                throw;
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(resetPasswordDto.Email);

                if (user == null || !user.IsPasswordResetTokenValid() || user.PasswordResetToken != resetPasswordDto.Token)
                {
                    throw new InvalidOperationException("Token de réinitialisation invalide ou expiré");
                }

                user.PasswordHash = HashPassword(resetPasswordDto.NewPassword);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation($"Mot de passe réinitialisé pour: {user.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la réinitialisation du mot de passe");
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new InvalidOperationException("Utilisateur introuvable");
                }

                if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                {
                    throw new InvalidOperationException("Mot de passe actuel incorrect");
                }

                user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation($"Mot de passe changé pour: {user.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de mot de passe");
                throw;
            }
        }

        public async Task<bool> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(confirmEmailDto.Email);

                if (user == null || user.EmailConfirmationToken != confirmEmailDto.Token)
                {
                    throw new InvalidOperationException("Token de confirmation invalide");
                }

                user.EmailConfirmed = true;
                user.EmailConfirmationToken = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation($"Email confirmé pour: {user.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la confirmation d'email");
                throw;
            }
        }

        public async Task<bool> ResendConfirmationEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null || user.EmailConfirmed)
                {
                    return true; // Ne pas révéler si l'email existe
                }

                user.EmailConfirmationToken = GenerateRandomToken();
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                await _emailService.SendEmailConfirmationAsync(user.Email, user.EmailConfirmationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du renvoi de l'email de confirmation");
                throw;
            }
        }

        public async Task<bool> IsEmailTakenAsync(string email)
        {
            return await _userRepository.IsEmailTakenAsync(email);
        }

        public async Task<bool> ChangeInitialPasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) throw new InvalidOperationException("Utilisateur introuvable");

                // No need to verify temporary password here as user just logged in with it to get this far
                // and the userId is extracted from the secure JWT claims.

                user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
                user.MustChangePassword = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation($"Mot de passe initial changé pour: {user.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de mot de passe initial");
                throw;
            }
        }

        #region Private Helper Methods

        private async Task<AuthResponseDto> GenerateAuthResponse(User user, bool rememberMe = false)
        {
            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = _jwtTokenService.GetRefreshTokenExpiration(rememberMe);
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString(),
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenExpiration = _jwtTokenService.GetAccessTokenExpiration(),
                EmailConfirmed = user.EmailConfirmed,
                MustChangePassword = user.MustChangePassword
            };
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                // Fallback for legacy SHA256 hashes during migration
                using var sha256 = SHA256.Create();
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes) == hash;
            }
        }

        private string GenerateRandomToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateTemporaryPassword(int length = 12)
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "@#$!%&";
            const string all = upper + lower + digits + special;

            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            var chars = new char[length];
            // Garantir au moins 1 majuscule, 1 minuscule, 1 chiffre, 1 spécial
            chars[0] = upper[bytes[0] % upper.Length];
            chars[1] = lower[bytes[1] % lower.Length];
            chars[2] = digits[bytes[2] % digits.Length];
            chars[3] = special[bytes[3] % special.Length];

            for (int i = 4; i < length; i++)
                chars[i] = all[bytes[i] % all.Length];

            // Mélanger
            for (int i = length - 1; i > 0; i--)
            {
                int j = bytes[i] % (i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars);
        }

        #endregion
    }
}