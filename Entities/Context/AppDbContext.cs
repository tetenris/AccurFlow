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
        public DbSet<SupplierEntity> Suppliers { get; set; }
        public DbSet<InvoiceEntity> Invoices { get; set; }
        public DbSet<InvoiceLineEntity> InvoiceLines { get; set; }
        public DbSet<PaymentEntity> Payments { get; set; }
        public DbSet<PaymentAllocationEntity> PaymentAllocations { get; set; }
        public DbSet<PurchaseOrderEntity> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderLineEntity> PurchaseOrderLines { get; set; }
        public DbSet<PurchaseRequestEntity> PurchaseRequests { get; set; }
        public DbSet<PurchaseRequestLineEntity> PurchaseRequestLines { get; set; }
        public DbSet<GoodsReceiptEntity> GoodsReceipts { get; set; }
        public DbSet<GoodsReceiptLineEntity> GoodsReceiptLines { get; set; }
        public DbSet<GoodsReturnEntity> GoodsReturns { get; set; }
        public DbSet<GoodsReturnLineEntity> GoodsReturnLines { get; set; }
        public DbSet<SalesQuotationEntity> SalesQuotations { get; set; }
        public DbSet<SalesQuotationLineEntity> SalesQuotationLines { get; set; }
        public DbSet<SalesOrderEntity> SalesOrders { get; set; }
        public DbSet<SalesOrderLineEntity> SalesOrderLines { get; set; }
        public DbSet<DeliveryOrderEntity> DeliveryOrders { get; set; }
        public DbSet<DeliveryOrderLineEntity> DeliveryOrderLines { get; set; }
        public DbSet<ItemEntity> Items { get; set; }
        public DbSet<ItemGroupEntity> ItemGroups { get; set; }
        public DbSet<UnitEntity> Units { get; set; }
        public DbSet<WarehouseEntity> Warehouses { get; set; }
        public DbSet<StockMovementEntity> StockMovements { get; set; }
        public DbSet<StockBatchEntity> StockBatches { get; set; }
        public DbSet<StockTransferEntity> StockTransfers { get; set; }
        public DbSet<StockTransferLineEntity> StockTransferLines { get; set; }
        public DbSet<StockOpnameEntity> StockOpnames { get; set; }
        public DbSet<StockOpnameLineEntity> StockOpnameLines { get; set; }
        public DbSet<CashBankTransferEntity> CashBankTransfers { get; set; }
        public DbSet<BankReconciliationEntity> BankReconciliations { get; set; }
        public DbSet<BankReconciliationLineEntity> BankReconciliationLines { get; set; }
        public DbSet<ApprovalRequestEntity> ApprovalRequests { get; set; }
        public DbSet<ApprovalHistoryEntity> ApprovalHistories { get; set; }
        public DbSet<DocumentAttachmentEntity> DocumentAttachments { get; set; }
        public DbSet<TaxEntity> Taxes { get; set; }
        public DbSet<FixedAssetEntity> FixedAssets { get; set; }
        public DbSet<FixedAssetDepreciationEntity> FixedAssetDepreciations { get; set; }
        public DbSet<EmployeeEntity> Employees { get; set; }
        public DbSet<PayrollEntity> Payrolls { get; set; }
        public DbSet<PayrollLineEntity> PayrollLines { get; set; }
        public DbSet<BillOfMaterialEntity> BillOfMaterials { get; set; }
        public DbSet<BillOfMaterialLineEntity> BillOfMaterialLines { get; set; }
        public DbSet<ProductionOrderEntity> ProductionOrders { get; set; }
        public DbSet<ProductionOrderLineEntity> ProductionOrderLines { get; set; }
        public DbSet<YearEndClosingEntity> YearEndClosings { get; set; }

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
