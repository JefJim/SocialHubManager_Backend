using Microsoft.EntityFrameworkCore;
using SocialHubManager_Backend.src.Core.Entities;

namespace SocialHubManager_Backend.src.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserSocialNetwork> UserSocialNetworks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar la clave primaria compuesta (UserId + Provider)
            modelBuilder.Entity<UserSocialNetwork>()
                .HasKey(usn => new { usn.UserId, usn.Provider });

            // Relación User 1:N UserSocialNetworks
            modelBuilder.Entity<UserSocialNetwork>()
                .HasOne(usn => usn.User)
                .WithMany(u => u.UserSocialNetworks)
                .HasForeignKey(usn => usn.UserId);
        }
    }
}
