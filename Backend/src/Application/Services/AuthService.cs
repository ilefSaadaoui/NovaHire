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
    /// Service d'authentification pour NovaHire.
    /// Gère l'inscription des utilisateurs et des entreprises, la connexion, la gestion des tokens de session (JWT),
    /// le renouvellement (Refresh Token), ainsi que les flux de réinitialisation et de changement de mots de passe.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="AuthService"/>.
        /// </summary>
        /// <param name="userRepository">Repository de gestion des données utilisateurs.</param>
        /// <param name="companyRepository">Repository de gestion des données d'entreprises.</param>
        /// <param name="jwtTokenService">Service de génération et validation des tokens JWT.</param>
        /// <param name="emailService">Service d'envoi d'e-mails.</param>
        /// <param name="logger">Logger pour les évènements de sécurité et d'audit.</param>
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

        /// <summary>
        /// Enregistre un nouvel utilisateur au sein d'une entreprise existante.
        /// Valide l'unicité de l'adresse email et la limite des comptes autorisés par l'abonnement entreprise.
        /// </summary>
        /// <param name="registerDto">Les données d'inscription de l'utilisateur.</param>
        /// <returns>Les informations d'authentification incluant les tokens d'accès.</returns>
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

                    // Vérifier la limite d'utilisateurs autorisés au sein de l'entreprise
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

                // Générer les tokens même si l'email n'est pas encore validé par l'utilisateur
                return await GenerateAuthResponse(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'inscription");
                throw;
            }
        }

        /// <summary>
        /// Enregistre une nouvelle entreprise partenaire ainsi que son administrateur principal.
        /// Optionnellement, invite un recruteur secondaire avec un mot de passe temporaire généré automatiquement.
        /// </summary>
        /// <param name="registerDto">Les informations de l'entreprise et du compte administrateur.</param>
        /// <returns>Les informations de réponse d'authentification stipulant l'approbation requise.</returns>
        public async Task<AuthResponseDto> RegisterCompanyAsync(CompanyRegisterDto registerDto)
        {
            try
            {
                // 1. Vérifications initiales d'unicité
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

        /// <summary>
        /// Connecte un utilisateur en vérifiant son e-mail, son mot de passe et son statut d'activité globale.
        /// </summary>
        /// <param name="loginDto">Les données de connexion.</param>
        /// <returns>La réponse d'authentification incluant les jetons JWT.</returns>
        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var email = loginDto.Email?.Trim().ToLowerInvariant() ?? string.Empty;
                var password = loginDto.Password?.Trim() ?? string.Empty;
                var user = await _userRepository.GetByEmailWithCompanyAsync(email);

                if (user == null)
                {
                    throw new UnauthorizedAccessException("Email ou mot de passe incorrect");
                }

                // Vérification du mot de passe avec BCrypt ou fallback sécurisé vers SHA256
                if (!VerifyPassword(password, user.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Email ou mot de passe incorrect");
                }

                if (!user.IsActive)
                {
                    throw new UnauthorizedAccessException(
                        "Ce compte est désactivé. Si vous venez de vous inscrire, attendez la validation de votre entreprise par l'administrateur.");
                }

                if (user.Role != UserRole.SuperAdmin && user.CompanyId.HasValue)
                {
                    var company = user.Company;
                    if (company != null)
                    {
                        if (company.Status == CompanyStatus.Pending)
                        {
                            throw new UnauthorizedAccessException(
                                "Votre entreprise est en attente de validation par l'administrateur de la plateforme.");
                        }

                        if (!company.IsActive
                            || company.Status == CompanyStatus.Rejected
                            || company.Status == CompanyStatus.Suspended)
                        {
                            throw new UnauthorizedAccessException(
                                "Accès refusé : votre entreprise est inactive, refusée ou suspendue.");
                        }
                    }
                }

                // Le système d'abonnement est supprimé. La vérification est toujours positive.

                // Mettre à jour la date de dernière connexion pour des raisons de statistiques RH
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

        /// <summary>
        /// Renouvelle le jeton d'accès Access Token en échange d'un Refresh Token valide.
        /// </summary>
        /// <param name="refreshTokenDto">Le Refresh Token courant de l'utilisateur.</param>
        /// <returns>Une nouvelle paire de jetons d'accès.</returns>
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

                // Optionnel : Valider la signature du jeton d'accès expiré
                if (!string.IsNullOrEmpty(refreshTokenDto.AccessToken))
                {
                    var principal = await _jwtTokenService.ValidateTokenAsync(refreshTokenDto.AccessToken);
                    if (principal == null)
                    {
                        _logger.LogWarning("Access token invalide lors du refresh");
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

        /// <summary>
        /// Révoque la session de l'utilisateur connecté en supprimant ses jetons Refresh Token en base de données.
        /// </summary>
        /// <param name="userId">Identifiant unique de l'utilisateur connecté.</param>
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

        /// <summary>
        /// Initie la procédure de mot de passe oublié en générant un jeton à validité limitée et en envoyant un e-mail de réinitialisation.
        /// </summary>
        /// <param name="forgotPasswordDto">Les informations de réinitialisation de mot de passe.</param>
        /// <returns>True si le processus a été complété sans divulguer l'existence ou non du compte.</returns>
        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(
                    forgotPasswordDto.Email?.Trim().ToLowerInvariant() ?? string.Empty);

                if (user == null)
                {
                    // Protection contre l'énumération des emails
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

        /// <summary>
        /// Réinitialise le mot de passe utilisateur à l'aide d'un jeton de réinitialisation valide envoyé par email.
        /// </summary>
        /// <param name="resetPasswordDto">Le nouveau mot de passe et le jeton d'authentification associé.</param>
        /// <returns>True si le mot de passe a été modifié avec succès.</returns>
        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var email = resetPasswordDto.Email?.Trim().ToLowerInvariant() ?? string.Empty;
                var token = NormalizeToken(resetPasswordDto.Token);
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null || !user.IsPasswordResetTokenValid() || !TokenEquals(user.PasswordResetToken, token))
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

        /// <summary>
        /// Modifie le mot de passe actuel d'un utilisateur authentifié.
        /// </summary>
        /// <param name="userId">Identifiant unique de l'utilisateur émetteur.</param>
        /// <param name="changePasswordDto">Les informations de mot de passe actuel et de nouveau mot de passe.</param>
        /// <returns>True si le changement a été appliqué avec succès.</returns>
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

        /// <summary>
        /// Confirme l'adresse e-mail d'un utilisateur à l'aide de son token de confirmation.
        /// </summary>
        /// <param name="confirmEmailDto">L'email de l'utilisateur et le jeton de confirmation reçu.</param>
        /// <returns>True si l'email a été validé.</returns>
        public async Task<bool> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto)
        {
            try
            {
                var email = confirmEmailDto.Email?.Trim().ToLowerInvariant() ?? string.Empty;
                var token = NormalizeToken(confirmEmailDto.Token);
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null || !TokenEquals(user.EmailConfirmationToken, token))
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

        /// <summary>
        /// Renvoie l'e-mail de confirmation avec un nouveau token généré de manière aléatoire.
        /// </summary>
        /// <param name="email">L'e-mail destinataire.</param>
        /// <returns>True si le renvoi a été exécuté.</returns>
        public async Task<bool> ResendConfirmationEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null || user.EmailConfirmed)
                {
                    return true; // Protection contre la divulgation d'email existant
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

        /// <summary>
        /// Vérifie si une adresse email est déjà enregistrée en base de données.
        /// </summary>
        /// <param name="email">L'email à analyser.</param>
        /// <returns>True si l'email est déjà utilisé, False sinon.</returns>
        public async Task<bool> IsEmailTakenAsync(string email)
        {
            return await _userRepository.IsEmailTakenAsync(email);
        }

        /// <summary>
        /// Modifie le mot de passe temporaire attribué lors d'une invitation entreprise.
        /// Désactive le drapeau 'MustChangePassword' requis.
        /// </summary>
        /// <param name="userId">Identifiant unique de l'utilisateur.</param>
        /// <param name="changePasswordDto">Le nouveau mot de passe souhaité.</param>
        /// <returns>True si le mot de passe initial a été mis à jour.</returns>
        public async Task<bool> ChangeInitialPasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) throw new InvalidOperationException("Utilisateur introuvable");

                // Pas besoin de revérifier le mot de passe temporaire car l'utilisateur s'est déjà connecté avec succès 
                // et l'identifiant est extrait de manière sécurisée depuis les claims JWT.

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
            if (string.IsNullOrEmpty(hash))
            {
                return false;
            }

            // BCrypt hashes always start with $2a$, $2b$, $2y$, etc.
            if (hash.StartsWith("$2", StringComparison.Ordinal))
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }

            // Legacy SHA256 hashes (données seed / anciens comptes)
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes) == hash;
        }

        private string GenerateRandomToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            // URL-safe : évite la corruption des + / = dans les liens e-mail
            return Convert.ToBase64String(randomNumber)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string NormalizeToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return string.Empty;
            }

            var normalized = Uri.UnescapeDataString(token.Trim());
            // Les query strings transforment parfois '+' en espace
            return normalized.Replace(' ', '+');
        }

        private static bool TokenEquals(string? storedToken, string providedToken)
        {
            if (string.IsNullOrEmpty(storedToken) || string.IsNullOrEmpty(providedToken))
            {
                return false;
            }

            return string.Equals(
                NormalizeToken(storedToken),
                NormalizeToken(providedToken),
                StringComparison.Ordinal);
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