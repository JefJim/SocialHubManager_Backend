using System.ComponentModel.DataAnnotations;

namespace SocialHubManager_Backend.src.Core.Entities
{
    public class SocialNetwork
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = null!;

        public ICollection<UserSocialNetwork> UserSocialNetworks { get; set; } = new List<UserSocialNetwork>();
    }
}
