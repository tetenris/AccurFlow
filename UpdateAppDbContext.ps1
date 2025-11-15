$targetPath = "D:\ASP.NET\2025\AKURAT\AccuFlow"
$dbContextPath = Join-Path $targetPath "Data\AppDbContext.cs"

$updatedContent = @'
using Microsoft.EntityFrameworkCore;
using AccuFlow.Entities;

namespace AccuFlow.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<RoleEntity> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure RoleEntity
            modelBuilder.Entity<RoleEntity>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.RoleName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.HasIndex(e => e.RoleName);
            });
        }
    }
}
'@

Set-Content -Path $dbContextPath -Value $updatedContent
Write-Host "✅ AppDbContext.cs updated successfully!" -ForegroundColor Green
