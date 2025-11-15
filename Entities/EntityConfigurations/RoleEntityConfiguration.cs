using AccuFlow.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuFlow.Entities.EntityConfigurations
{
    public class RoleEntityConfiguration : IEntityTypeConfiguration<RoleEntity>
    {
        public void Configure(EntityTypeBuilder<RoleEntity> builder)
        {
            builder.ToTable("Roles");
            
            builder.HasKey(r => r.RoleId);
            
            builder.Property(r => r.RoleId)
                .IsRequired();
            
            builder.Property(r => r.RoleName)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(r => r.Description)
                .IsRequired()
                .HasMaxLength(500);
            
            builder.Property(r => r.Permissions)
                .IsRequired();
            
            builder.Property(r => r.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
        }
    }
}
