#pragma warning disable CS8601, CS8602, CS8604
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs.Auth;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Contrôleur gérant toutes les opérations d'authentification et d'autorisation des utilisateurs.
    /// Traite l'inscription, la connexion, la gestion des sessions (jetons JWT/Refresh), la confirmation d'email
    /// et la réinitialisation de mot de passe. Les exceptions sont gérées par GlobalExceptionMiddleware.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="AuthController"/>.
        /// </summary>
        /// <param name="authService">Service d'authentification métier.</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Inscrit un nouvel utilisateur standard (candidat) dans le système.
        /// </summary>
        /// <param name="registerDto">Les informations requises pour l'inscription de l'utilisateur.</param>
        /// <returns>Les informations d'authentification incluant le token JWT généré et le Refresh Token.</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            var response = await _authService.RegisterAsync(registerDto);
            return Ok(response);
        }

        /// <summary>
        /// Inscrit une nouvelle entreprise ainsi que son utilisateur administrateur associé.
        /// </summary>
        /// <param name="registerDto">Les informations requises pour l'entreprise et son administrateur.</param>
        /// <returns>Les informations d'authentification de l'administrateur de l'entreprise créé.</returns>
        [HttpPost("register-company")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AuthResponseDto>> RegisterCompany([FromBody] CompanyRegisterDto registerDto)
        {
            var response = await _authService.RegisterCompanyAsync(registerDto);
            return Ok(response);
        }

        /// <summary>
        /// Connecte un utilisateur existant en vérifiant ses identifiants.
        /// </summary>
        /// <param name="loginDto">Les identifiants de connexion (Email et Mot de passe).</param>
        /// <returns>Les jetons d'accès et de rafraîchissement JWT si la connexion réussit.</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);
            return Ok(response);
        }

        /// <summary>
        /// Génère un nouveau token d'accès JWT valide en échange d'un Refresh Token valide et non expiré.
        /// </summary>
        /// <param name="refreshTokenDto">Le Refresh Token actuel de l'utilisateur.</param>
        /// <returns>Un nouveau token JWT d'accès ainsi qu'un nouveau Refresh Token.</returns>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var response = await _authService.RefreshTokenAsync(refreshTokenDto);
            return Ok(response);
        }

        /// <summary>
        /// Déconnecte l'utilisateur actuellement connecté en invalidant son Refresh Token actif.
        /// Nécessite une authentification JWT.
        /// </summary>
        /// <returns>Un message indiquant le succès de la déconnexion.</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Logout()
        {
            var userId = GetCurrentUserId();
            await _authService.LogoutAsync(userId);
            return Ok(new { message = "Déconnexion réussie" });
        }

        /// <summary>
        /// Initie une procédure de réinitialisation de mot de passe en envoyant un e-mail contenant un lien sécurisé.
        /// </summary>
        /// <param name="forgotPasswordDto">L'adresse e-mail de l'utilisateur ayant oublié son mot de passe.</param>
        /// <returns>Un message de confirmation générique pour des raisons de sécurité.</returns>
        [HttpPost("forgot-password")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            await _authService.ForgotPasswordAsync(forgotPasswordDto);
            return Ok(new { message = "Si cet email existe, un lien de réinitialisation a été envoyé" });
        }

        /// <summary>
        /// Réinitialise le mot de passe d'un utilisateur en utilisant le jeton sécurisé reçu par e-mail.
        /// </summary>
        /// <param name="resetPasswordDto">Le nouveau mot de passe accompagné du jeton de validation.</param>
        /// <returns>Un message indiquant le succès de l'opération de réinitialisation.</returns>
        [HttpPost("reset-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            await _authService.ResetPasswordAsync(resetPasswordDto);
            return Ok(new { message = "Mot de passe réinitialisé avec succès" });
        }

        /// <summary>
        /// Permet à un utilisateur authentifié de modifier son mot de passe actuel.
        /// Nécessite une authentification JWT.
        /// </summary>
        /// <param name="changePasswordDto">L'ancien mot de passe et le nouveau mot de passe souhaité.</param>
        /// <returns>Un message de succès.</returns>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userId = GetCurrentUserId();
            await _authService.ChangePasswordAsync(userId, changePasswordDto);
            return Ok(new { message = "Mot de passe changé avec succès" });
        }

        /// <summary>
        /// Force la modification du mot de passe temporaire généré lors de la création d'un utilisateur par un tiers.
        /// Nécessite une authentification JWT.
        /// </summary>
        /// <param name="changePasswordDto">Les informations de changement de mot de passe.</param>
        /// <returns>Un message indiquant le succès de la mise à jour.</returns>
        [HttpPost("change-initial-password")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ChangeInitialPassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userId = GetCurrentUserId();
            await _authService.ChangeInitialPasswordAsync(userId, changePasswordDto);
            return Ok(new { message = "Mot de passe initial mis à jour avec succès" });
        }

        /// <summary>
        /// Confirme l'adresse e-mail d'un utilisateur en validant le jeton de confirmation envoyé après inscription.
        /// </summary>
        /// <param name="confirmEmailDto">L'adresse e-mail et le jeton de confirmation.</param>
        /// <returns>Un message indiquant que l'adresse e-mail a été confirmée avec succès.</returns>
        [HttpPost("confirm-email")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto confirmEmailDto)
        {
            await _authService.ConfirmEmailAsync(confirmEmailDto);
            return Ok(new { message = "Email confirmé avec succès" });
        }

        /// <summary>
        /// Renvoyer l'e-mail contenant le jeton de confirmation à l'adresse spécifiée.
        /// </summary>
        /// <param name="email">L'adresse e-mail de l'utilisateur concerné.</param>
        /// <returns>Un message indiquant le renvoi de l'e-mail.</returns>
        [HttpPost("resend-confirmation")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ResendConfirmation([FromBody] string email)
        {
            await _authService.ResendConfirmationEmailAsync(email);
            return Ok(new { message = "Email de confirmation renvoyé" });
        }

        /// <summary>
        /// Vérifie si une adresse e-mail est déjà associée à un compte utilisateur existant.
        /// </summary>
        /// <param name="email">L'adresse e-mail à vérifier.</param>
        /// <returns>Un objet JSON indiquant la disponibilité de l'adresse email (available = true/false).</returns>
        [HttpGet("check-email")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            var isTaken = await _authService.IsEmailTakenAsync(email);
            return Ok(new { available = !isTaken });
        }

        /// <summary>
        /// Récupère les informations clés du profil de l'utilisateur actuellement connecté.
        /// Nécessite une authentification JWT.
        /// </summary>
        /// <returns>Un dictionnaire contenant l'ID utilisateur, l'e-mail, le nom, le prénom, le rôle et le CompanyId de l'utilisateur.</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(200)]
        public IActionResult GetCurrentUser()
        {
            // Les informations sont extraites directement des claims inclus dans le jeton JWT
            return Ok(new
            {
                userId = GetCurrentUserId(),
                email = User.FindFirstValue(ClaimTypes.Email),
                firstName = User.FindFirstValue(ClaimTypes.GivenName),
                lastName = User.FindFirstValue(ClaimTypes.Surname),
                role = User.FindFirstValue(ClaimTypes.Role),
                companyId = User.FindFirstValue("CompanyId")
            });
        }

        #region Helper Methods

        /// <summary>
        /// Extrait l'identifiant de l'utilisateur (UserId) à partir des claims du token JWT.
        /// </summary>
        /// <returns>L'identifiant unique de l'utilisateur sous forme de Guid.</returns>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdClaim);
        }

        #endregion
    }
}
