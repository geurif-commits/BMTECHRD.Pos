using BMTECHRD.Pos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BMTECHRD.Pos.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.HasIndex(x => new { x.BusinessId, x.Name }).IsUnique();

        builder.Property(x => x.Price).HasPrecision(18,2).IsRequired();

        builder.Property(x => x.Stock).IsRequired();
        builder.Property(x => x.TrackInventory).IsRequired();
        builder.Property(x => x.Area).IsRequired();
        builder.Property(x => x.ImageUrl).HasMaxLength(400);
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}