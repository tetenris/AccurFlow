using AccuFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuFlow.Entities.EntityConfigurations
{
    public class ChartOfAccountEntityConfiguration : IEntityTypeConfiguration<ChartOfAccountEntity>
    {
        public void Configure(EntityTypeBuilder<ChartOfAccountEntity> builder)
        {
            builder.ToTable("ChartOfAccounts");
            
            builder.HasKey(c => c.AccountId);
            
            builder.Property(c => c.AccountId)
                .IsRequired();
            
            builder.Property(c => c.AccountCode)
                .IsRequired()
                .HasMaxLength(20);
            
            builder.Property(c => c.AccountName)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(c => c.AccountType)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(c => c.Description)
                .HasMaxLength(500);
            
            builder.Property(c => c.IsHeader)
                .IsRequired()
                .HasDefaultValue(false);
            
            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            builder.Property(c => c.OpeningBalance)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);
            
            builder.Property(c => c.NormalBalance)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("Debit");
            
            builder.Property(c => c.Currency)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("IDR");
            
            builder.Property(c => c.Level)
                .IsRequired()
                .HasDefaultValue(0);
            
            // Configure self-referencing relationship for hierarchical structure
            builder.HasOne(c => c.ParentAccount)
                .WithMany(c => c.ChildAccounts)
                .HasForeignKey(c => c.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Create unique index on AccountCode
            builder.HasIndex(c => c.AccountCode)
                .IsUnique();
            
            // Create indexes for common queries
            builder.HasIndex(c => c.AccountType);
            builder.HasIndex(c => c.ParentAccountId);
            builder.HasIndex(c => c.IsActive);
            builder.HasIndex(c => c.IsDeleted);
        }
    }
}

