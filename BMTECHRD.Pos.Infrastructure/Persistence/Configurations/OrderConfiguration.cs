using BMTECHRD.Pos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BMTECHRD.Pos.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => new { x.BusinessId, x.TableId, x.CreatedAt });

        builder.HasOne<Table>()
            .WithMany()
            .HasForeignKey(x => x.TableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
