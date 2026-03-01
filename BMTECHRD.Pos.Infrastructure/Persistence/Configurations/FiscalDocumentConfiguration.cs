using BMTECHRD.Pos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BMTECHRD.Pos.Infrastructure.Persistence.Configurations;

public sealed class FiscalDocumentConfiguration : IEntityTypeConfiguration<FiscalDocument>
{
    public void Configure(EntityTypeBuilder<FiscalDocument> b)
    {
        b.ToTable("fiscal_documents");
        b.HasKey(x => x.Id);

        b.Property(x => x.Type).HasMaxLength(16).IsRequired();
        b.Property(x => x.Number).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(32).IsRequired();

        b.Property(x => x.Subtotal).HasPrecision(18, 2);
        b.Property(x => x.Tax).HasPrecision(18, 2);
        b.Property(x => x.Tip).HasPrecision(18, 2);
        b.Property(x => x.Total).HasPrecision(18, 2);

        b.HasIndex(x => new { x.BusinessId, x.Number }).IsUnique();
        b.HasIndex(x => x.TableId);
    }
}
