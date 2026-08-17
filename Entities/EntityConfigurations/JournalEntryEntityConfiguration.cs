using AccuFlow.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuFlow.Entities.EntityConfigurations
{
    public class JournalEntryEntityConfiguration : IEntityTypeConfiguration<JournalEntryEntity>
    {
        public void Configure(EntityTypeBuilder<JournalEntryEntity> builder)
        {
            builder.ToTable("JournalEntries");

            builder.HasKey(x => x.JournalId);

            // Indexes
            builder.HasIndex(x => x.JournalNumber).IsUnique();
            builder.HasIndex(x => x.JournalDate);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => new { x.JournalDate, x.Status });

            // Relationships
            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.UpdatedByUser)
                .WithMany()
                .HasForeignKey(x => x.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PostedByUser)
                .WithMany()
                .HasForeignKey(x => x.PostedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DeletedByUser)
                .WithMany()
                .HasForeignKey(x => x.DeletedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReversalJournal)
                .WithOne(x => x.OriginalJournal)
                .HasForeignKey<JournalEntryEntity>(x => x.ReversalJournalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.JournalLines)
                .WithOne(x => x.JournalEntry)
                .HasForeignKey(x => x.JournalId)
                .OnDelete(DeleteBehavior.Cascade);

            // Default values
            builder.Property(x => x.Status).HasDefaultValue("Draft");
            builder.Property(x => x.JournalType).HasDefaultValue("General");
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
