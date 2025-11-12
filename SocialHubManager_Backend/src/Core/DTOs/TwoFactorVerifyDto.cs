namespace SocialHubManager_Backend.src.Core.DTOs
{
    public class TwoFactorVerifyDto
    {
        public string Secret { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}
