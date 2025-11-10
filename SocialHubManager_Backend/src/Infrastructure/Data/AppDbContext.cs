using Microsoft.EntityFrameworkCore;
using SocialHubManager_Backend.src.Core.Entities;

namespace SocialHubManager_Backend.src.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<SocialNetwork> SocialNetworks { get; set; } = null!;
        public DbSet<UserSocialNetwork> UserSocialNetworks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserSocialNetwork>()
                .HasKey(usn => new { usn.UserId, usn.SocialNetworkId });

            modelBuilder.Entity<UserSocialNetwork>()
                .HasOne(usn => usn.User)
                .WithMany(u => u.UserSocialNetworks)
                .HasForeignKey(usn => usn.UserId);

            modelBuilder.Entity<UserSocialNetwork>()
                .HasOne(usn => usn.SocialNetwork)
                .WithMany(sn => sn.UserSocialNetworks)
                .HasForeignKey(usn => usn.SocialNetworkId);
        }
    }
}
