using System;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Department;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    /// <summary>
    /// Contrôleur gérant les opérations liées aux départements d'une entreprise.
    /// Nécessite une authentification (token JWT).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Extrait l'ID de l'entreprise (CompanyId) à partir des claims du token JWT de l'utilisateur connecté.
        /// </summary>
        /// <returns>Le CompanyId sous forme de Guid.</returns>
        /// <exception cref="UnauthorizedAccessException">Si le claim n'est pas présent.</exception>
        private Guid GetCompanyId()
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim))
                throw new UnauthorizedAccessException("Company ID non trouvé dans les claims.");

            return Guid.Parse(companyIdClaim);
        }

        /// <summary>
        /// Récupère la liste de tous les départements appartenant à l'entreprise de l'utilisateur connecté.
        /// </summary>
        /// <returns>Une liste de <see cref="DepartmentDto"/>.</returns>
        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var companyId = GetCompanyId();
            var departments = await _context.Departments!
                .Where(d => d.CompanyId == companyId)
                .Include(d => d.Users)
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    MembersCount = d.Users.Count,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            return Ok(departments);
        }

        /// <summary>
        /// Crée un nouveau département pour l'entreprise.
        /// Uniquement accessible aux administrateurs de l'entreprise ou SuperAdmin.
        /// </summary>
        /// <param name="request">Les informations du nouveau département.</param>
        /// <returns>L'ID du département créé.</returns>
        [HttpPost]
        [Authorize(Policy = "CompanyAdminOrAbove")]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Le nom du département est requis." });

            var companyId = GetCompanyId();

            var exists = await _context.Departments!
                .AnyAsync(d => d.CompanyId == companyId && d.Name.ToLower() == request.Name.ToLower());

            if (exists)
                return BadRequest(new { message = "Un département avec ce nom existe déjà." });

            var department = new Department
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Departments!.Add(department);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDepartments), new { id = department.Id }, new { message = "Département créé avec succès", id = department.Id });
        }

        /// <summary>
        /// Met à jour les informations d'un département existant.
        /// Uniquement accessible aux administrateurs de l'entreprise ou SuperAdmin.
        /// </summary>
        /// <param name="id">L'ID du département à modifier.</param>
        /// <param name="request">Les nouvelles informations du département.</param>
        [HttpPut("{id}")]
        [Authorize(Policy = "CompanyAdminOrAbove")]
        public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Le nom du département est requis." });

            var companyId = GetCompanyId();

            var department = await _context.Departments!
                .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId);

            if (department == null)
                return NotFound(new { message = "Département introuvable." });

            // Check name uniqueness if changed
            if (department.Name.ToLower() != request.Name.ToLower())
            {
                var exists = await _context.Departments!
                    .AnyAsync(d => d.CompanyId == companyId && d.Name.ToLower() == request.Name.ToLower());

                if (exists)
                    return BadRequest(new { message = "Un département avec ce nom existe déjà." });
            }

            department.Name = request.Name;
            department.Description = request.Description;
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Département mis à jour avec succès" });
        }

        /// <summary>
        /// Supprime un département existant.
        /// Impossible de supprimer un département s'il contient encore des utilisateurs.
        /// Uniquement accessible aux administrateurs de l'entreprise ou SuperAdmin.
        /// </summary>
        /// <param name="id">L'ID du département à supprimer.</param>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CompanyAdminOrAbove")]
        public async Task<IActionResult> DeleteDepartment(Guid id)
        {
            var companyId = GetCompanyId();

            var department = await _context.Departments!
                .Include(d => d.Users)
                .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId);

            if (department == null)
                return NotFound(new { message = "Département introuvable." });

            if (department.Users.Any())
                return BadRequest(new { message = "Impossible de supprimer ce département car il contient des membres. Veuillez d'abord réassigner ou supprimer les membres." });

            _context.Departments!.Remove(department);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Département supprimé avec succès" });
        }
    }
}
