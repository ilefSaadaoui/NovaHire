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
    /// Contrôleur d'authentification — les exceptions sont gérées par GlobalExceptionMiddleware
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Inscription d'un nouvel utilisateur
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            var response = await _authService.RegisterAsync(registerDto);
            return Ok(response);
        }

        /// <summary>
        /// Inscription d'une nouvelle entreprise avec son administrateur
        /// </summary>
        [HttpPost("register-company")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AuthResponseDto>> RegisterCompany([FromBody] CompanyRegisterDto registerDto)
        {
            var response = await _authService.RegisterCompanyAsync(registerDto);
            return Ok(response);
        }

        /// <summary>
        /// Connexion d'un utilisateur
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);
            return Ok(response);
        }

        /// <summary>
        /// Rafraîchir le token d'accès
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var response = await _authService.RefreshTokenAsync(refreshTokenDto);
            return Ok(response);
        }

        /// <summary>
        /// Déconnexion de l'utilisateur
        /// </summary>
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
        /// Demande de réinitialisation de mot de passe
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            await _authService.ForgotPasswordAsync(forgotPasswordDto);
            return Ok(new { message = "Si cet email existe, un lien de réinitialisation a été envoyé" });
        }

        /// <summary>
        /// Réinitialiser le mot de passe
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            await _authService.ResetPasswordAsync(resetPasswordDto);
            return Ok(new { message = "Mot de passe réinitialisé avec succès" });
        }

        /// <summary>
        /// Changer le mot de passe (utilisateur connecté)
        /// </summary>
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
        /// Changer le mot de passe initial (première connexion)
        /// </summary>
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
        /// Confirmer l'email
        /// </summary>
        [HttpPost("confirm-email")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto confirmEmailDto)
        {
            await _authService.ConfirmEmailAsync(confirmEmailDto);
            return Ok(new { message = "Email confirmé avec succès" });
        }

        /// <summary>
        /// Renvoyer l'email de confirmation
        /// </summary>
        [HttpPost("resend-confirmation")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ResendConfirmation([FromBody] string email)
        {
            await _authService.ResendConfirmationEmailAsync(email);
            return Ok(new { message = "Email de confirmation renvoyé" });
        }

        /// <summary>
        /// Vérifier si un email est disponible
        /// </summary>
        [HttpGet("check-email")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            var isTaken = await _authService.IsEmailTakenAsync(email);
            return Ok(new { available = !isTaken });
        }

        /// <summary>
        /// Obtenir les informations de l'utilisateur connecté
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(200)]
        public IActionResult GetCurrentUser()
        {
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

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdClaim);
        }

        #endregion
    }
}
