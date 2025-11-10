using System;
using Microsoft.EntityFrameworkCore;
using SocialHubManager_Backend.src.Core.DTOs;
using SocialHubManager_Backend.src.Core.Entities;
using SocialHubManager_Backend.src.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace SocialHubManager_Backend.src.Application
{
    public class AuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> Register(RegisterDto dto)
        {
            // Verificar si ya existe el email
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return null;

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return null;

            return VerifyPassword(dto.Password, user.PasswordHash) ? user : null;
        }

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
    }
}
