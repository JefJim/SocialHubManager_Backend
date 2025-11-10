using Microsoft.AspNetCore.Mvc;
using SocialHubManager_Backend.src.Application;
using SocialHubManager_Backend.src.Core.DTOs;
using SocialHubManager_Backend.src.Core.Entities;

namespace SocialHubManager_Backend.src.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
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
        public async Task<ActionResult<User>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _authService.Login(loginDto);
                if (user == null)
                    return Unauthorized(new { message = "Usuario o contraseña incorrecta" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
