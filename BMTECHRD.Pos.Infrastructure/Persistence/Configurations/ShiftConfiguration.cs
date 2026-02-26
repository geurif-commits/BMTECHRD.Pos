using BMTECHRD.Pos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BMTECHRD.Pos.Infrastructure.Persistence.Configurations;

public sealed class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("shifts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BusinessId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.OpenedAt).IsRequired();
        builder.Property(x => x.OpeningCash).HasPrecision(18,2).IsRequired();
        builder.Property(x => x.ClosingCash).HasPrecision(18,2);
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasIndex(x => new { x.UserId, x.Status });
        builder.HasIndex(x => new { x.BusinessId });
    }
}
