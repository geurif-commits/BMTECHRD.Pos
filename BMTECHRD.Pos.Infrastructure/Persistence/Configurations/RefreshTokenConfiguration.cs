using BMTECHRD.Pos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BMTECHRD.Pos.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.BusinessId)
            .IsRequired();

        builder.Property(rt => rt.UserId)
            .IsRequired();

        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.RevokedAt)
            .IsRequired(false);

        builder.Property(rt => rt.ReplacedByTokenId)
            .IsRequired(false);

        builder.Property(rt => rt.DeviceId)
            .IsRequired(false)
            .HasMaxLength(80);

        builder.Property(rt => rt.IpAddress)
            .IsRequired(false)
            .HasMaxLength(64);

        // Índices para búsquedas rápidas
        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => rt.ExpiresAt);
        builder.HasIndex(rt => rt.BusinessId);

        // Único en TokenHash (un hash no puede repetirse)
        builder.HasIndex(rt => rt.TokenHash)
            .IsUnique();

        builder.ToTable("RefreshTokens");
    }
}
