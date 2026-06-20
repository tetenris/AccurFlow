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
        public DbSet<MenuEntity> Menus { get; set; }
        public DbSet<ChartOfAccountEntity> ChartOfAccounts { get; set; }
        public DbSet<JournalEntryEntity> JournalEntries { get; set; }
        public DbSet<JournalLineEntity> JournalLines { get; set; }
        public DbSet<CustomerEntity> Customers { get; set; }
        public DbSet<SupplierEntity> Suppliers { get; set; }
        public DbSet<InvoiceEntity> Invoices { get; set; }
        public DbSet<InvoiceLineEntity> InvoiceLines { get; set; }
        public DbSet<PaymentEntity> Payments { get; set; }
        public DbSet<PaymentAllocationEntity> PaymentAllocations { get; set; }
        public DbSet<PurchaseOrderEntity> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderLineEntity> PurchaseOrderLines { get; set; }
        public DbSet<ItemEntity> Items { get; set; }
        public DbSet<WarehouseEntity> Warehouses { get; set; }
        public DbSet<StockMovementEntity> StockMovements { get; set; }
        public DbSet<StockOpnameEntity> StockOpnames { get; set; }
        public DbSet<StockOpnameLineEntity> StockOpnameLines { get; set; }
        public DbSet<ApprovalRequestEntity> ApprovalRequests { get; set; }
        public DbSet<ApprovalHistoryEntity> ApprovalHistories { get; set; }
        public DbSet<DocumentAttachmentEntity> DocumentAttachments { get; set; }
        public DbSet<TaxEntity> Taxes { get; set; }

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
            
            modelBuilder.ApplyConfiguration(new EntityConfigurations.UserEntityConfiguration());
            modelBuilder.ApplyConfiguration(new EntityConfigurations.RoleEntityConfiguration());
            modelBuilder.ApplyConfiguration(new EntityConfigurations.MenuEntityConfiguration());

            modelBuilder.Ignore<ChartOfAccountEntity>();
            modelBuilder.Ignore<JournalEntryEntity>();
            modelBuilder.Ignore<JournalLineEntity>();
            modelBuilder.Ignore<CustomerEntity>();
            modelBuilder.Ignore<SupplierEntity>();
            modelBuilder.Ignore<InvoiceEntity>();
            modelBuilder.Ignore<InvoiceLineEntity>();
            modelBuilder.Ignore<PaymentEntity>();
            modelBuilder.Ignore<PaymentAllocationEntity>();
            modelBuilder.Ignore<PurchaseOrderEntity>();
            modelBuilder.Ignore<PurchaseOrderLineEntity>();
            modelBuilder.Ignore<ItemEntity>();
            modelBuilder.Ignore<WarehouseEntity>();
            modelBuilder.Ignore<StockMovementEntity>();
            modelBuilder.Ignore<StockOpnameEntity>();
            modelBuilder.Ignore<StockOpnameLineEntity>();
            modelBuilder.Ignore<ApprovalRequestEntity>();
            modelBuilder.Ignore<ApprovalHistoryEntity>();
            modelBuilder.Ignore<DocumentAttachmentEntity>();
            modelBuilder.Ignore<TaxEntity>();
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
