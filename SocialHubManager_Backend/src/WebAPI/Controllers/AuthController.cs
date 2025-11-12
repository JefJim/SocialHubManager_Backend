using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocialHubManager_Backend.src.Application;
using SocialHubManager_Backend.src.Core.DTOs;
using SocialHubManager_Backend.src.Core.Entities;
using SocialHubManager_Backend.src.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SocialHubManager_Backend.src.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config; 



        public AuthController(AuthService authService, AppDbContext context, IConfiguration config)
        {
            _authService = authService;
            _context = context;
            _config = config; 

        }

        /// ---------------------------
        /// Get information about the currently authenticated user
        /// ---------------------------
        /// 
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult> GetCurrentUser()
        {
            // Cambiar "sub" por NameIdentifier
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            return Ok(new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                twoFactorEnabled = user.TwoFactorEnabled,
            });
        }
        // ---------------------------
        // Registro de usuario
        // ---------------------------
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _authService.Register(registerDto);
                return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ---------------------------
        // Login de usuario
        // ---------------------------
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // El servicio Login debe retornar:
                // (User user, string tokenCompleto, bool requires2FA)
                var (user, tokenCompleto, requires2FA) = await _authService.Login(loginDto);

                if (user == null)
                    return Unauthorized(new { message = "Usuario o contraseña incorrecta" });

                if (requires2FA)
                {
                    // Generar token parcial solo con userId (y opcional email) para validar 2FA
                    var claimsParciales = new[]
                    {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

                    var key = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "SuperSecretKey")
                    );
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var tokenParcial = new JwtSecurityToken(
                        issuer: _config["Jwt:Issuer"],
                        audience: _config["Jwt:Audience"],
                        claims: claimsParciales,
                        expires: DateTime.UtcNow.AddMinutes(10), // token parcial válido solo unos minutos
                        signingCredentials: creds
                    );

                    return Ok(new
                    {
                        requires2FA = true,
                        token = new JwtSecurityTokenHandler().WriteToken(tokenParcial),
                        user = new { id = user.Id, email = user.Email }
                    });
                }

                // Si no requiere 2FA, devolver token completo
                return Ok(new { token = tokenCompleto, user = new { id = user.Id, name = user.Name, email = user.Email, twoFactorEnabled = user.TwoFactorEnabled } });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
