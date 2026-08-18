using AccuFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuFlow.Entities.EntityConfigurations
{
    public class MenuEntityConfiguration : IEntityTypeConfiguration<MenuEntity>
    {
        public void Configure(EntityTypeBuilder<MenuEntity> builder)
        {
            builder.ToTable("Menus");

            builder.HasKey(m => m.MenuId);

            builder.Property(m => m.MenuId)
                .IsRequired();

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.Controller)
                .HasMaxLength(100);

            builder.Property(m => m.Action)
                .HasMaxLength(500);

            builder.Property(m => m.Icon)
                .HasMaxLength(5000);

            builder.Property(m => m.Sequence)
                .IsRequired();

            // Self-referencing relationship
            builder.HasOne(m => m.ParentMenu)
                .WithMany(m => m.ChildMenus)
                .HasForeignKey(m => m.MenuParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(m => m.Sequence);
        }
    }
}

