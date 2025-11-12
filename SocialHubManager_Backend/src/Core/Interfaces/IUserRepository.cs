using SocialHubManager_Backend.src.Core.Entities;
using System.Threading.Tasks;

namespace SocialHubManager_Backend.src.Core.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
    }
}
