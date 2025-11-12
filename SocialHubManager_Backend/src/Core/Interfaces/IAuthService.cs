using SocialHubManager_Backend.src.Core.DTOs;
using SocialHubManager_Backend.src.Core.Entities;

namespace SocialHubManager_Backend.src.Core.Interfaces
{
    public interface IAuthService
    {
        Task<User> Register(RegisterDto dto);
        Task<User?> Login(LoginDto dto);
        Task<User?> GetUserByEmail(string email);
        string GenerateJwtToken(User user);
    }
}
