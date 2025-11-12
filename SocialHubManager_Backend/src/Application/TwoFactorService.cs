using SocialHubManager_Backend.src.Core.Entities;
using OtpNet;

namespace SocialHubManager_Backend.src.Application
{
    public class TwoFactorService
    {
        public bool VerifyCode(User user, string token)
        {
            if (string.IsNullOrEmpty(user.OtpSecret))
                return false;

            var secretBytes = Base32Encoding.ToBytes(user.OtpSecret);
            var totp = new Totp(secretBytes);
            return totp.VerifyTotp(token, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
        }
    }
}
