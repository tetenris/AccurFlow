using AccuFlow.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuFlow.Entities.EntityConfigurations
{
    public class SupplierEntityConfiguration : IEntityTypeConfiguration<SupplierEntity>
    {
        public void Configure(EntityTypeBuilder<SupplierEntity> builder)
        {
            builder.ToTable("Suppliers");
            
            // Primary Key
            builder.HasKey(e => e.SupplierId);
            
            // Required Fields
            builder.Property(e => e.SupplierCode)
                .IsRequired()
                .HasMaxLength(20);
            
            builder.Property(e => e.SupplierName)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(e => e.SupplierType)
                .IsRequired()
                .HasMaxLength(50);
            
            // Optional Fields with Max Length
            builder.Property(e => e.ContactPerson)
                .HasMaxLength(255);
            
            builder.Property(e => e.Phone)
                .HasMaxLength(50);
            
            builder.Property(e => e.Email)
                .HasMaxLength(255);
            
            builder.Property(e => e.Website)
                .HasMaxLength(255);
            
            builder.Property(e => e.Address)
                .HasMaxLength(500);
            
            builder.Property(e => e.City)
                .HasMaxLength(100);
            
            builder.Property(e => e.State)
                .HasMaxLength(100);
            
            builder.Property(e => e.PostalCode)
                .HasMaxLength(20);
            
            builder.Property(e => e.Country)
                .HasMaxLength(100);
            
            builder.Property(e => e.TaxId)
                .HasMaxLength(50);
            
            builder.Property(e => e.Notes)
                .HasMaxLength(1000);
            
            // Decimal Precision
            builder.Property(e => e.CreditLimit)
                .HasPrecision(18, 2);
            
            builder.Property(e => e.CurrentBalance)
                .HasPrecision(18, 2);
            
            // Default Values
            builder.Property(e => e.IsActive)
                .HasDefaultValue(true);
            
            builder.Property(e => e.CreditLimit)
                .HasDefaultValue(0);
            
            builder.Property(e => e.PaymentTerms)
                .HasDefaultValue(30);
            
            builder.Property(e => e.CurrentBalance)
                .HasDefaultValue(0);
            
            // Indexes
            builder.HasIndex(e => e.SupplierCode)
                .IsUnique()
                .HasDatabaseName("IX_Suppliers_SupplierCode");
            
            builder.HasIndex(e => e.SupplierName)
                .HasDatabaseName("IX_Suppliers_SupplierName");
            
            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Suppliers_IsActive");
            
            builder.HasIndex(e => new { e.IsActive, e.IsDeleted })
                .HasDatabaseName("IX_Suppliers_IsActive_IsDeleted");
            
            // Audit Fields (inherited from BaseEntity)
            builder.Property(e => e.CreatedBy)
                .HasMaxLength(255);
            
            builder.Property(e => e.UpdatedBy)
                .HasMaxLength(255);
        }
    }
}
