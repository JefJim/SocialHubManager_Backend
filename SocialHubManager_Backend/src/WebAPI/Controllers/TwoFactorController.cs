using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtpNet;
using QRCoder;
using SocialHubManager_Backend.src.Application;
using SocialHubManager_Backend.src.Core.DTOs;
using SocialHubManager_Backend.src.Core.Entities;
using System;
using System.Drawing.Printing;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SocialHubManager_Backend.src.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TwoFactorController : ControllerBase
    {
        private readonly AuthService _authService;

        public TwoFactorController(AuthService authService)
        {
            _authService = authService;
        }

        // ----------------------------------------------------
        // generate setup info (secret + QR) for 2FA
        // ----------------------------------------------------
        [HttpGet("setup")]
        public async Task<IActionResult> Setup()
        {
            // Obtener el ID desde el claim NameIdentifier
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "No se encontró el claim NameIdentifier en el token." });

            if (!int.TryParse(userIdClaim, out int userId))
                return BadRequest(new { message = "El ID de usuario en el claim no es válido." });

            var user = await _authService.GetById(userId);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado." });

            // generate secret key
            var key = KeyGeneration.GenerateRandomKey(20);
            var secret = Base32Encoding.ToString(key);
            // generate otpauth URI compliant with Google Authenticator
            var issuer = Uri.EscapeDataString("SocialHubManager");
            var email = Uri.EscapeDataString(user.Email);
            var uri = $"otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}&digits=6";

            // Generate QR code image
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrBytes = qrCode.GetGraphic(20);
            var qrBase64 = Convert.ToBase64String(qrBytes);

            return Ok(new
            {
                secret,
                qrImage = $"data:image/png;base64,{qrBase64}"
                
            });
        }

        // ----------------------------------------------------
        // Confirm and enable 2FA
        // ----------------------------------------------------
        [HttpPost("enable")]
        public async Task<IActionResult> Enable([FromBody] TwoFactorVerifyDto request)
        {
            // Obtener el ID desde el claim NameIdentifier
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "No se encontró el claim NameIdentifier en el token." });

            if (!int.TryParse(userIdClaim, out int userId))
                return BadRequest(new { message = "El ID de usuario en el claim no es válido." });

            // Buscar usuario en la BD
            var user = await _authService.GetById(userId);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado." });

            // Validar el código TOTP
            var totp = new Totp(Base32Encoding.ToBytes(request.Secret));
            var valid = totp.VerifyTotp(request.Code, out long _, VerificationWindow.RfcSpecifiedNetworkDelay);

            if (!valid)
                return BadRequest(new { message = "Código incorrecto o expirado." });

            // Habilitar 2FA para el usuario
            await _authService.EnableTwoFactorAsync(userId, request.Secret);

            return Ok(new { message = "Autenticación de dos factores activada correctamente." });
        }


        // ----------------------------------------------------
        // Verify 2FA code during login
        // ----------------------------------------------------
        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] TwoFactorVerifyDto request)
        {
            // Obtener el ID desde el claim NameIdentifier
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "No se encontró el claim NameIdentifier en el token." });

            if (!int.TryParse(userIdClaim, out int userId))
                return BadRequest(new { message = "El ID de usuario en el claim no es válido." });

            // Buscar usuario
            var user = await _authService.GetById(userId);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado." });

            if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.OtpSecret))
                return BadRequest(new { message = "El usuario no tiene 2FA activo." });

            // Verificar código con la secret guardada en BD
            var totp = new Totp(Base32Encoding.ToBytes(user.OtpSecret));
            
            Console.WriteLine("Verifying TOTP code for user ID: " + totp);
            var valid = totp.VerifyTotp(request.Code, out long _, VerificationWindow.RfcSpecifiedNetworkDelay);
            Console.WriteLine("TOTP code valid: " + valid);

            if (!valid)
                return BadRequest(new { message = "Código incorrecto o expirado." });

            // Si es válido, generar nuevo token JWT
            var token = _authService.GenerateJwtToken(user);
            return Ok(new { token });
        }

        // ----------------------------------------------------
        // Disable 2FA
        // ----------------------------------------------------
        [Authorize]
        [HttpPost("disable")]
        public async Task<IActionResult> Disable([FromQuery] int userId)
        {
            var user = await _authService.GetById(userId);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado." });

            user.TwoFactorEnabled = false;
            user.OtpSecret = null;

            await _authService.EnableTwoFactorAsync(userId, null);

            return Ok(new { message = "Autenticación de dos factores desactivada." });
        }
    }
}
