using AccuFlow.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuFlow.Entities.EntityConfigurations
{
    public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.ToTable("Users");
            
            builder.HasKey(u => u.UserId);
            
            builder.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.Property(u => u.FailedLoginAttempts)
                .HasDefaultValue(0);

            builder.Property(u => u.IsLocked)
                .HasDefaultValue(false);

            builder.Property(u => u.LockedReason)
                .HasMaxLength(255);
            
            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(u => u.IsActive)
                .IsRequired();
            
            // Configure relationship with Role
            builder.HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Create unique index on Email
            builder.HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
