using AccuFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuFlow.Entities.EntityConfigurations
{
    public class CustomerEntityConfiguration : IEntityTypeConfiguration<CustomerEntity>
    {
        public void Configure(EntityTypeBuilder<CustomerEntity> builder)
        {
            builder.ToTable("Customers");
            
            // Primary Key
            builder.HasKey(e => e.CustomerId);
            
            // Required Fields
            builder.Property(e => e.CustomerCode)
                .IsRequired()
                .HasMaxLength(20);
            
            builder.Property(e => e.CustomerName)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(e => e.CustomerType)
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
            builder.HasIndex(e => e.CustomerCode)
                .IsUnique()
                .HasDatabaseName("IX_Customers_CustomerCode");
            
            builder.HasIndex(e => e.CustomerName)
                .HasDatabaseName("IX_Customers_CustomerName");
            
            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Customers_IsActive");
            
            builder.HasIndex(e => new { e.IsActive, e.IsDeleted })
                .HasDatabaseName("IX_Customers_IsActive_IsDeleted");
            
            // Audit Fields (inherited from BaseEntity)
            builder.Property(e => e.CreatedBy)
                .HasMaxLength(255);
            
            builder.Property(e => e.UpdatedBy)
                .HasMaxLength(255);
        }
    }
}

