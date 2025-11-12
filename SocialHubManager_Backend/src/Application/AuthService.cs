using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SocialHubManager_Backend.src.Core.DTOs;
using SocialHubManager_Backend.src.Core.Entities;
using SocialHubManager_Backend.src.Infrastructure.Data;

namespace SocialHubManager_Backend.src.Application
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ---------------------------
        // Registro de usuario
        // ---------------------------
        public async Task<User?> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new Exception("El correo ya está registrado.");

            var user = new User
            {
                Name = dto.Username,
                Email = dto.Email,
                Password = HashPassword(dto.Password),
                TwoFactorEnabled = false,
                OtpSecret = null
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        // ---------------------------
        // Login de usuario
        // ---------------------------
        public async Task<(User? user, string? token, bool requires2FA)> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !VerifyPassword(dto.Password, user.Password))
                return (null, null, false);

            // Si el usuario tiene 2FA activo, no generamos token todavía
            if (user.TwoFactorEnabled)
                return (user, null, true);

            // Caso normal: generar token JWT
            var token = GenerateJwtToken(user);
            return (user, token, false);
        }

        // ---------------------------
        // Hash de contraseñas
        // ---------------------------
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        // ---------------------------
        // Generar token JWT
        // ---------------------------
        public string GenerateJwtToken(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (user.Id <= 0)
                throw new InvalidOperationException("El usuario no tiene un ID válido para el claim 'sub'.");

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),  // 👈 Claim estándar 'sub'
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim("name", user.Name)
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "SuperSecretKey")
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            // 🪵 Log opcional para verificar el claim 'sub'
            Console.WriteLine($"[JWT] Generado token con claim sub={user.Id}");

            return jwt;
        }

        // ---------------------------
        // Obtener usuario por ID
        // ---------------------------
        public async Task<User?> GetById(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        // ---------------------------
        // Actualizar estado de 2FA
        // ---------------------------
        public async Task EnableTwoFactorAsync(int userId, string secret)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("Usuario no encontrado.");

            user.TwoFactorEnabled = true;
            user.OtpSecret = secret;
            user.UpdatedAt = DateTime.UtcNow; // <-- clave: asegurarse de UTC
            await _context.SaveChangesAsync();
        }
    }
}
