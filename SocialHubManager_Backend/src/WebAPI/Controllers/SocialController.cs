using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialHubManager_Backend.src.Core.Entities;
using SocialHubManager_Backend.src.Infrastructure.Data;
using System.Security.Claims;

namespace SocialHubManager_Backend.src.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 protege el endpoint con JWT
    public class SocialController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SocialController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/social/configs
        [HttpGet("configs")]
        public async Task<ActionResult> GetUserSocialConfigs()
        {
            // ✅ Obtiene el ID real del usuario autenticado desde el token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "No se encontró el ID de usuario en el token." });

            if (!int.TryParse(userIdClaim, out int userId))
                return BadRequest(new { message = "El ID de usuario del token no es válido." });

            // ✅ Consulta la tabla correcta (ya mapeada con [Table("SocialAccounts")])
            var socialNetworks = await _context.UserSocialNetworks
                .Where(u => u.UserId == userId)
                .ToListAsync();

            return Ok(new { configs = socialNetworks });
        }
    }
}
