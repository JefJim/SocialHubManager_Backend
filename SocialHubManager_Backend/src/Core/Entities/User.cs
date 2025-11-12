using SocialHubManager_Backend.src.Core.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialHubManager_Backend.src.Core.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [Column("twoFactorEnabled")]
        public bool TwoFactorEnabled { get; set; }

        [Column("otpSecret")]
        public string? OtpSecret { get; set; }

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserSocialNetwork> UserSocialNetworks { get; set; } = new List<UserSocialNetwork>();
    }
}
