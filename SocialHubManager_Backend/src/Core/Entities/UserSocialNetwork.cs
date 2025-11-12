using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialHubManager_Backend.src.Core.Entities
{
    [Table("SocialAccounts")] 
    public class UserSocialNetwork
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("userId")] 
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Column("provider")]
        public string Provider { get; set; } = null!;
        [Column("providerId")]
        public string ProviderId { get; set; } = null!;
        [Column("token")]
        public string Token { get; set; } = null!;
        [Column("refreshToken")]
        public string? RefreshToken { get; set; }
        [Column("displayName")]
        public string? DisplayName { get; set; }
        [Column("clientId")]
        public string? ClientId { get; set; }
        [Column("clientSecret")]
        public string? ClientSecret { get; set; }
        [Column("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
