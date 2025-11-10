namespace SocialHubManager_Backend.src.Core.Entities
{
    public class UserSocialNetwork
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int SocialNetworkId { get; set; }
        public SocialNetwork SocialNetwork { get; set; } = null!;
    }
}
