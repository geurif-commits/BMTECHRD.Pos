using BMTECHRD.Pos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BMTECHRD.Pos.Infrastructure.Persistence.Configurations;

public sealed class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.ToTable("licenses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ActivationKey)
            .HasMaxLength(80)
            .IsRequired();

        builder.HasIndex(x => x.ActivationKey).IsUnique();

        builder.Property(x => x.Plan).IsRequired();
        builder.Property(x => x.Status).IsRequired();
    }
}
