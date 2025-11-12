

using SocialHubManager_Backend.src.Core.Entities;
using SocialHubManager_Backend.src.Core.Interfaces;
using System;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using OtpNet;
using QRCoder;

namespace SocialHubManager_Backend.src.Infrastructure.Services
{
    public class TwoFactorService : ITwoFactorService
    {
        public async Task<(string SecretKey, string QrCodeDataUrl)> GenerateTwoFactorSetupAsync(User user)
        {
            // Implementation for generating a secret key and QR code data URL
            var key = KeyGeneration.GenerateRandomKey(20);
            var secretKey = Base32Encoding.ToString(key);

            // Create OTP URI compatible with authenticator apps
            var otpUri = new OtpUri(OtpType.Totp, secretKey, user.Email, "SocialHubManager");

            // generate QR code
            using var qrGenerator = new QRCoder.QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(otpUri.ToString(), QRCoder.QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCoder.QRCode(qrData);
            using var bitmap = qrCode.GetGraphic(20);

            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            var base64 = Convert.ToBase64String(ms.ToArray());
            var dataUrl = $"data:image/png;base64,{base64}";

            return await Task.FromResult((secretKey, dataUrl));

        }
        // used only first time for setup verification
        public async Task<bool> VerifySetupCodeAsync(User user, string token)
        {
            if (string.IsNullOrEmpty(user.OtpSecret))
                return false;
            
            // Implementation for verifying the setup code
            var totp = new Totp(Base32Encoding.ToBytes(user.OtpSecret));
            return await Task.FromResult(totp.VerifyTotp(token, out _, new VerificationWindow(1, 1)));
        }
        // used on each login when 2FA is enabled
        public async Task<bool> VerifyLoginCodeAsync(User user, string token)
        {
            // Implementation for verifying the login code
            var totp = new Totp(Base32Encoding.ToBytes(user.OtpSecret));
            return await Task.FromResult(totp.VerifyTotp(token, out _, new VerificationWindow(1, 1)));
        }
    }
}
