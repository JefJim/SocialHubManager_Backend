using System.Threading.Tasks;
using SocialHubManager_Backend.src.Core.Entities;

namespace SocialHubManager_Backend.src.Core.Interfaces
{
    public interface ITwoFactorService
    {
        Task<(string SecretKey, string QrCodeDataUrl)> GenerateTwoFactorSetupAsync(User user);
        Task<bool> VerifySetupCodeAsync(User user, string token);
        Task<bool> VerifyLoginCodeAsync(User user, string token);
    }
}
