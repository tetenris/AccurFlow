using AccuFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuFlow.Entities.EntityConfigurations
{
    public class CashBankTransferEntityConfiguration : IEntityTypeConfiguration<CashBankTransferEntity>
    {
        public void Configure(EntityTypeBuilder<CashBankTransferEntity> builder)
        {
            builder.ToTable("CashBankTransfers");
            builder.HasKey(e => e.TransferId);
            builder.Property(e => e.TransferNumber).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
            builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
            builder.Property(e => e.ReferenceNumber).HasMaxLength(100);
            builder.Property(e => e.Amount).HasPrecision(18, 2);
            builder.HasIndex(e => e.TransferNumber).IsUnique();
            builder.HasOne(e => e.FromAccount).WithMany().HasForeignKey(e => e.FromAccountId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.ToAccount).WithMany().HasForeignKey(e => e.ToAccountId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.JournalEntry).WithMany().HasForeignKey(e => e.JournalId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class BankReconciliationEntityConfiguration : IEntityTypeConfiguration<BankReconciliationEntity>
    {
        public void Configure(EntityTypeBuilder<BankReconciliationEntity> builder)
        {
            builder.ToTable("BankReconciliations");
            builder.HasKey(e => e.ReconciliationId);
            builder.Property(e => e.ReconciliationNumber).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
            builder.Property(e => e.Notes).HasMaxLength(1000);
            builder.Property(e => e.StatementEndingBalance).HasPrecision(18, 2);
            builder.Property(e => e.GlEndingBalance).HasPrecision(18, 2);
            builder.HasIndex(e => e.ReconciliationNumber).IsUnique();
            builder.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(e => e.Lines).WithOne(e => e.Reconciliation).HasForeignKey(e => e.ReconciliationId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class BankReconciliationLineEntityConfiguration : IEntityTypeConfiguration<BankReconciliationLineEntity>
    {
        public void Configure(EntityTypeBuilder<BankReconciliationLineEntity> builder)
        {
            builder.ToTable("BankReconciliationLines");
            builder.HasKey(e => e.ReconciliationLineId);
            builder.Property(e => e.DocumentNumber).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
            builder.Property(e => e.Amount).HasPrecision(18, 2);
        }
    }
}
