using Microsoft.EntityFrameworkCore;
using SocialHubManager_Backend.src.Core.Entities;
using SocialHubManager_Backend.src.Infrastructure.Data;
using SocialHubManager_Backend.src.Core.Interfaces;

using System.Threading.Tasks;

namespace SocialHubManager_Backend.src.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
