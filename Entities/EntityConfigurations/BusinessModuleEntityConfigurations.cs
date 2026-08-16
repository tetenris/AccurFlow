using AccuFlow.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuFlow.Entities.EntityConfigurations
{
    public class InvoiceEntityConfiguration : IEntityTypeConfiguration<InvoiceEntity>
    {
        public void Configure(EntityTypeBuilder<InvoiceEntity> builder)
        {
            builder.ToTable("Invoices");
            builder.HasKey(e => e.InvoiceId);
            builder.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
            builder.Property(e => e.InvoiceType).IsRequired().HasMaxLength(20);
            builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
            builder.Property(e => e.Notes).HasMaxLength(1000);
            builder.Property(e => e.SubTotal).HasPrecision(18, 2);
            builder.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            builder.Property(e => e.TaxAmount).HasPrecision(18, 2);
            builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
            builder.Property(e => e.PaidAmount).HasPrecision(18, 2);
            builder.HasIndex(e => e.InvoiceNumber).IsUnique();
            builder.HasIndex(e => new { e.InvoiceType, e.Status, e.IsDeleted });
            builder.HasMany(e => e.Lines).WithOne(e => e.Invoice).HasForeignKey(e => e.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Supplier).WithMany().HasForeignKey(e => e.SupplierId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.JournalEntry).WithMany().HasForeignKey(e => e.JournalId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class InvoiceLineEntityConfiguration : IEntityTypeConfiguration<InvoiceLineEntity>
    {
        public void Configure(EntityTypeBuilder<InvoiceLineEntity> builder)
        {
            builder.ToTable("InvoiceLines");
            builder.HasKey(e => e.InvoiceLineId);
            builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
            builder.Property(e => e.Quantity).HasPrecision(18, 4);
            builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
            builder.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            builder.Property(e => e.TaxAmount).HasPrecision(18, 2);
            builder.Property(e => e.LineTotal).HasPrecision(18, 2);
            builder.HasOne(e => e.Item).WithMany().HasForeignKey(e => e.ItemId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class PaymentEntityConfiguration : IEntityTypeConfiguration<PaymentEntity>
    {
        public void Configure(EntityTypeBuilder<PaymentEntity> builder)
        {
            builder.ToTable("Payments");
            builder.HasKey(e => e.PaymentId);
            builder.Property(e => e.PaymentNumber).IsRequired().HasMaxLength(50);
            builder.Property(e => e.PaymentType).IsRequired().HasMaxLength(20);
            builder.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
            builder.Property(e => e.ReferenceNumber).HasMaxLength(100);
            builder.Property(e => e.Notes).HasMaxLength(1000);
            builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
            builder.HasIndex(e => e.PaymentNumber).IsUnique();
            builder.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Supplier).WithMany().HasForeignKey(e => e.SupplierId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.CashBankAccount).WithMany().HasForeignKey(e => e.CashBankAccountId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.JournalEntry).WithMany().HasForeignKey(e => e.JournalId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(e => e.Allocations).WithOne(e => e.Payment).HasForeignKey(e => e.PaymentId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class PaymentAllocationEntityConfiguration : IEntityTypeConfiguration<PaymentAllocationEntity>
    {
        public void Configure(EntityTypeBuilder<PaymentAllocationEntity> builder)
        {
            builder.ToTable("PaymentAllocations");
            builder.HasKey(e => e.PaymentAllocationId);
            builder.Property(e => e.AllocatedAmount).HasPrecision(18, 2);
            builder.HasOne(e => e.Invoice).WithMany().HasForeignKey(e => e.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class PurchaseOrderEntityConfiguration : IEntityTypeConfiguration<PurchaseOrderEntity>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderEntity> builder)
        {
            builder.ToTable("PurchaseOrders");
            builder.HasKey(e => e.PurchaseOrderId);
            builder.Property(e => e.PurchaseOrderNumber).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
            builder.Property(e => e.Notes).HasMaxLength(1000);
            builder.Property(e => e.SubTotal).HasPrecision(18, 2);
            builder.Property(e => e.TaxAmount).HasPrecision(18, 2);
            builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
            builder.HasIndex(e => e.PurchaseOrderNumber).IsUnique();
            builder.HasOne(e => e.Supplier).WithMany().HasForeignKey(e => e.SupplierId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.PurchaseInvoice).WithMany().HasForeignKey(e => e.PurchaseInvoiceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(e => e.Lines).WithOne(e => e.PurchaseOrder).HasForeignKey(e => e.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class PurchaseOrderLineEntityConfiguration : IEntityTypeConfiguration<PurchaseOrderLineEntity>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderLineEntity> builder)
        {
            builder.ToTable("PurchaseOrderLines");
            builder.HasKey(e => e.PurchaseOrderLineId);
            builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
            builder.Property(e => e.Quantity).HasPrecision(18, 4);
            builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
            builder.Property(e => e.TaxAmount).HasPrecision(18, 2);
            builder.Property(e => e.LineTotal).HasPrecision(18, 2);
            builder.HasOne(e => e.Item).WithMany().HasForeignKey(e => e.ItemId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class GoodsReceiptEntityConfiguration : IEntityTypeConfiguration<GoodsReceiptEntity>
    {
        public void Configure(EntityTypeBuilder<GoodsReceiptEntity> builder)
        {
            builder.ToTable("GoodsReceipts");
            builder.HasKey(e => e.GoodsReceiptId);
            builder.Property(e => e.GoodsReceiptNumber).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
            builder.Property(e => e.Notes).HasMaxLength(1000);
            builder.Property(e => e.SubTotal).HasPrecision(18, 2);
            builder.Property(e => e.TaxAmount).HasPrecision(18, 2);
            builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
            builder.HasIndex(e => e.GoodsReceiptNumber).IsUnique();
            builder.HasIndex(e => new { e.Status, e.IsDeleted });
            builder.HasOne(e => e.PurchaseOrder).WithMany().HasForeignKey(e => e.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Supplier).WithMany().HasForeignKey(e => e.SupplierId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Warehouse).WithMany().HasForeignKey(e => e.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.JournalEntry).WithMany().HasForeignKey(e => e.JournalId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(e => e.Lines).WithOne(e => e.GoodsReceipt).HasForeignKey(e => e.GoodsReceiptId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class GoodsReceiptLineEntityConfiguration : IEntityTypeConfiguration<GoodsReceiptLineEntity>
    {
        public void Configure(EntityTypeBuilder<GoodsReceiptLineEntity> builder)
        {
            builder.ToTable("GoodsReceiptLines");
            builder.HasKey(e => e.GoodsReceiptLineId);
            builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
            builder.Property(e => e.Quantity).HasPrecision(18, 4);
            builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
            builder.Property(e => e.TaxAmount).HasPrecision(18, 2);
            builder.Property(e => e.LineTotal).HasPrecision(18, 2);
            builder.HasOne(e => e.Item).WithMany().HasForeignKey(e => e.ItemId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.PurchaseOrderLine).WithMany().HasForeignKey(e => e.PurchaseOrderLineId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ItemEntityConfiguration : IEntityTypeConfiguration<ItemEntity>
    {
        public void Configure(EntityTypeBuilder<ItemEntity> builder)
        {
            builder.ToTable("Items");
            builder.HasKey(e => e.ItemId);
            builder.Property(e => e.ItemCode).IsRequired().HasMaxLength(50);
            builder.Property(e => e.ItemName).IsRequired().HasMaxLength(255);
            builder.Property(e => e.ItemType).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Unit).IsRequired().HasMaxLength(20);
            builder.Property(e => e.Description).HasMaxLength(1000);
            builder.Property(e => e.SalesPrice).HasPrecision(18, 2);
            builder.Property(e => e.PurchasePrice).HasPrecision(18, 2);
            builder.HasIndex(e => e.ItemCode).IsUnique();
            builder.HasOne(e => e.InventoryAccount).WithMany().HasForeignKey(e => e.InventoryAccountId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.SalesAccount).WithMany().HasForeignKey(e => e.SalesAccountId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.CostOfGoodsSoldAccount).WithMany().HasForeignKey(e => e.CostOfGoodsSoldAccountId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class WarehouseEntityConfiguration : IEntityTypeConfiguration<WarehouseEntity>
    {
        public void Configure(EntityTypeBuilder<WarehouseEntity> builder)
        {
            builder.ToTable("Warehouses");
            builder.HasKey(e => e.WarehouseId);
            builder.Property(e => e.WarehouseCode).IsRequired().HasMaxLength(50);
            builder.Property(e => e.WarehouseName).IsRequired().HasMaxLength(255);
            builder.Property(e => e.Address).HasMaxLength(500);
            builder.HasIndex(e => e.WarehouseCode).IsUnique();
        }
    }

    public class StockMovementEntityConfiguration : IEntityTypeConfiguration<StockMovementEntity>
    {
        public void Configure(EntityTypeBuilder<StockMovementEntity> builder)
        {
            builder.ToTable("StockMovements");
            builder.HasKey(e => e.StockMovementId);
            builder.Property(e => e.MovementType).IsRequired().HasMaxLength(50);
            builder.Property(e => e.SourceDocumentType).IsRequired().HasMaxLength(50);
            builder.Property(e => e.QuantityIn).HasPrecision(18, 4);
            builder.Property(e => e.QuantityOut).HasPrecision(18, 4);
            builder.Property(e => e.UnitCost).HasPrecision(18, 2);
            builder.Property(e => e.Notes).HasMaxLength(1000);
            builder.HasOne(e => e.Item).WithMany().HasForeignKey(e => e.ItemId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Warehouse).WithMany().HasForeignKey(e => e.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class StockOpnameEntityConfiguration : IEntityTypeConfiguration<StockOpnameEntity>
    {
        public void Configure(EntityTypeBuilder<StockOpnameEntity> builder)
        {
            builder.ToTable("StockOpnames");
            builder.HasKey(e => e.StockOpnameId);
            builder.Property(e => e.StockOpnameNumber).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
            builder.Property(e => e.Notes).HasMaxLength(1000);
            builder.HasIndex(e => e.StockOpnameNumber).IsUnique();
            builder.HasOne(e => e.Warehouse).WithMany().HasForeignKey(e => e.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(e => e.Lines).WithOne(e => e.StockOpname).HasForeignKey(e => e.StockOpnameId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class StockOpnameLineEntityConfiguration : IEntityTypeConfiguration<StockOpnameLineEntity>
    {
        public void Configure(EntityTypeBuilder<StockOpnameLineEntity> builder)
        {
            builder.ToTable("StockOpnameLines");
            builder.HasKey(e => e.StockOpnameLineId);
            builder.Property(e => e.SystemQuantity).HasPrecision(18, 4);
            builder.Property(e => e.ActualQuantity).HasPrecision(18, 4);
            builder.Property(e => e.DifferenceQuantity).HasPrecision(18, 4);
            builder.Property(e => e.Notes).HasMaxLength(1000);
            builder.HasOne(e => e.Item).WithMany().HasForeignKey(e => e.ItemId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ApprovalRequestEntityConfiguration : IEntityTypeConfiguration<ApprovalRequestEntity>
    {
        public void Configure(EntityTypeBuilder<ApprovalRequestEntity> builder)
        {
            builder.ToTable("ApprovalRequests");
            builder.HasKey(e => e.ApprovalRequestId);
            builder.Property(e => e.DocumentType).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
            builder.Property(e => e.Notes).HasMaxLength(1000);
            builder.HasOne(e => e.RequestedByUser).WithMany().HasForeignKey(e => e.RequestedBy).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.CurrentApprover).WithMany().HasForeignKey(e => e.CurrentApproverId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(e => e.Histories).WithOne(e => e.ApprovalRequest).HasForeignKey(e => e.ApprovalRequestId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ApprovalHistoryEntityConfiguration : IEntityTypeConfiguration<ApprovalHistoryEntity>
    {
        public void Configure(EntityTypeBuilder<ApprovalHistoryEntity> builder)
        {
            builder.ToTable("ApprovalHistories");
            builder.HasKey(e => e.ApprovalHistoryId);
            builder.Property(e => e.Action).IsRequired().HasMaxLength(20);
            builder.Property(e => e.Notes).HasMaxLength(1000);
            builder.HasOne(e => e.Approver).WithMany().HasForeignKey(e => e.ApproverId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class DocumentAttachmentEntityConfiguration : IEntityTypeConfiguration<DocumentAttachmentEntity>
    {
        public void Configure(EntityTypeBuilder<DocumentAttachmentEntity> builder)
        {
            builder.ToTable("DocumentAttachments");
            builder.HasKey(e => e.DocumentAttachmentId);
            builder.Property(e => e.DocumentType).IsRequired().HasMaxLength(50);
            builder.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            builder.Property(e => e.StoredFileName).IsRequired().HasMaxLength(255);
            builder.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            builder.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Description).HasMaxLength(500);
            builder.HasIndex(e => new { e.DocumentType, e.DocumentId });
        }
    }

    public class TaxEntityConfiguration : IEntityTypeConfiguration<TaxEntity>
    {
        public void Configure(EntityTypeBuilder<TaxEntity> builder)
        {
            builder.ToTable("Taxes");
            builder.HasKey(e => e.TaxId);
            builder.Property(e => e.TaxCode).IsRequired().HasMaxLength(50);
            builder.Property(e => e.TaxName).IsRequired().HasMaxLength(255);
            builder.Property(e => e.TaxType).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Rate).HasPrecision(9, 4);
            builder.HasIndex(e => e.TaxCode).IsUnique();
            builder.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
