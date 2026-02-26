using BMTECHRD.Pos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BMTECHRD.Pos.Infrastructure.Persistence.Configurations;

public sealed class TableConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.ToTable("tables");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Number).IsRequired();
        builder.HasIndex(x => new { x.BusinessId, x.Number }).IsUnique();

        builder.Property(x => x.Status).IsRequired();

        builder.Property(x => x.PosX).IsRequired();
        builder.Property(x => x.PosY).IsRequired();

        builder.HasOne(x => x.OpenedByWaiter)
            .WithMany()
            .HasForeignKey(x => x.OpenedByWaiterId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
