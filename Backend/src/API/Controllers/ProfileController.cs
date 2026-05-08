using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(IUserRepository userRepository, IAuthService authService, IWebHostEnvironment environment)
        {
            _userRepository = userRepository;
            _authService = authService;
            _environment = environment;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("Utilisateur non identifié.");
            }
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetUserId();
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return NotFound();

                return Ok(new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber,
                    user.JobTitle,
                    user.Role,
                    user.CompanyId,
                    user.CreatedAt,
                    user.AvatarUrl,
                    user.DepartmentId,
                    department = user.Department?.Name
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            try
            {
                var userId = GetUserId();
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return NotFound();

                user.FirstName = dto.FirstName ?? user.FirstName;
                user.LastName = dto.LastName ?? user.LastName;
                user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _authService.ChangePasswordAsync(userId, dto);

                if (result)
                {
                    return Ok(new { message = "Mot de passe changé avec succès" });
                }

                return BadRequest(new { message = "Erreur lors du changement de mot de passe" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Aucun fichier sélectionné." });

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();

                if (System.Linq.Enumerable.All(allowedExtensions, ext => ext != extension))
                    return BadRequest(new { message = "Format de fichier non supporté." });

                var userId = GetUserId();
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return NotFound();

                // Dossier de stockage
                var uploadPath = System.IO.Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                if (!System.IO.Directory.Exists(uploadPath))
                    System.IO.Directory.CreateDirectory(uploadPath);

                var fileName = $"{userId}_{DateTime.UtcNow.Ticks}{extension}";
                var filePath = System.IO.Path.Combine(uploadPath, fileName);

                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // URL relative pour le frontend
                var avatarUrl = $"/uploads/avatars/{fileName}";
                user.AvatarUrl = avatarUrl;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                return Ok(new { avatarUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class UpdateProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
