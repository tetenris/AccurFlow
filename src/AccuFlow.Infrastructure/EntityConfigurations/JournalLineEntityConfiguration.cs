using AccuFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuFlow.Entities.EntityConfigurations
{
    public class JournalLineEntityConfiguration : IEntityTypeConfiguration<JournalLineEntity>
    {
        public void Configure(EntityTypeBuilder<JournalLineEntity> builder)
        {
            builder.ToTable("JournalLines");

            builder.HasKey(x => x.JournalLineId);

            // Indexes
            builder.HasIndex(x => x.JournalId);
            builder.HasIndex(x => x.AccountId);
            builder.HasIndex(x => new { x.AccountId, x.JournalId }).HasDatabaseName("IX_JournalLines_AccountId_JournalId");

            // Relationships
            builder.HasOne(x => x.JournalEntry)
                .WithMany(x => x.JournalLines)
                .HasForeignKey(x => x.JournalId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Account)
                .WithMany()
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Default values
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        }
    }
}

