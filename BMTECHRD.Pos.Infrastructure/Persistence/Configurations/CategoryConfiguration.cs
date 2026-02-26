using BMTECHRD.Pos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BMTECHRD.Pos.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(60)
            .IsRequired();

        builder.HasIndex(x => new { x.BusinessId, x.Name }).IsUnique();

        builder.Property(x => x.SortOrder).HasDefaultValue(0);
        builder.Property(x => x.IsActive).IsRequired();
    }
}