using BMTECHRD.Pos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BMTECHRD.Pos.Infrastructure.Persistence.Configurations;

public sealed class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.ToTable("businesses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(140)
            .IsRequired();

        builder.Property(x => x.LogoPath)
            .HasMaxLength(400);

        builder.Property(x => x.CurrencyCode)
            .HasMaxLength(8);

        builder.HasOne(x => x.License)
            .WithOne(x => x.Business)
            .HasForeignKey<License>(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
