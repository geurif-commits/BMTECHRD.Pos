using BMTECHRD.Pos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BMTECHRD.Pos.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasPrecision(18,2).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.MetaJson).HasMaxLength(1000);
        builder.Property(x => x.ShiftId).IsRequired();

        builder.HasOne<Shift>()
            .WithMany()
            .HasForeignKey("ShiftId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.BusinessId, x.TableId, x.CreatedAt });
    }
}
