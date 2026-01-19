using AccuFlow.Entities.Abstractions;
using AccuFlow.Entities.Entity;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Entities.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets will be added here
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<RoleEntity> Roles { get; set; }
        public DbSet<RoleMenuEntity> RoleMenus { get; set; }
        public DbSet<ChartOfAccountEntity> ChartOfAccounts { get; set; }
        public DbSet<MenuEntity> Menus { get; set; }
        public DbSet<JournalEntryEntity> JournalEntries { get; set; }
        public DbSet<JournalLineEntity> JournalLines { get; set; }
        public DbSet<CustomerEntity> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure RoleMenuEntity
            modelBuilder.Entity<RoleMenuEntity>(entity =>
            {
                entity.HasKey(e => e.RoleMenuId);
                entity.ToTable("RoleMenus");
                
                entity.HasOne(e => e.Role)
                    .WithMany()
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Menu)
                    .WithMany()
                    .HasForeignKey(e => e.MenuId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Apply configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
