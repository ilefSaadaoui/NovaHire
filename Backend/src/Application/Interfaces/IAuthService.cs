using System;
using System.Threading.Tasks;
using Application.DTOs.Auth;

namespace Application.Interfaces
{
    /// <summary>
    /// Interface du service d'authentification
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Inscription d'un nouvel utilisateur
        /// </summary>
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);

        /// <summary>
        /// Inscription d'une nouvelle entreprise avec son administrateur
        /// </summary>
        Task<AuthResponseDto> RegisterCompanyAsync(CompanyRegisterDto registerDto);

        /// <summary>
        /// Connexion d'un utilisateur
        /// </summary>
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);

        /// <summary>
        /// Rafraîchir le token d'accès
        /// </summary>
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);

        /// <summary>
        /// Déconnexion (invalidation du refresh token)
        /// </summary>
        Task LogoutAsync(Guid userId);

        /// <summary>
        /// Demande de réinitialisation de mot de passe
        /// </summary>
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);

        /// <summary>
        /// Réinitialisation du mot de passe
        /// </summary>
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

        /// <summary>
        /// Changement de mot de passe pour un utilisateur connecté
        /// </summary>
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);
        Task<bool> ChangeInitialPasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);

        /// <summary>
        /// Confirmation de l'email
        /// </summary>
        Task<bool> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto);

        /// <summary>
        /// Renvoyer l'email de confirmation
        /// </summary>
        Task<bool> ResendConfirmationEmailAsync(string email);

        /// <summary>
        /// Vérifier si un email existe déjà
        /// </summary>
        Task<bool> IsEmailTakenAsync(string email);
    }
}
