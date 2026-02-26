using BMTECHRD.Pos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BMTECHRD.Pos.Infrastructure.Persistence.Configurations;

public sealed class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.ToTable("bills");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Subtotal).HasPrecision(18,2).IsRequired();
        builder.Property(x => x.Tax).HasPrecision(18,2).IsRequired();
        builder.Property(x => x.Tip).HasPrecision(18,2).IsRequired();
        builder.Property(x => x.Discount).HasPrecision(18,2).IsRequired();
        builder.Property(x => x.Total).HasPrecision(18,2).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => new { x.BusinessId, x.TableId });
    }
}
