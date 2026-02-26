using BMTECHRD.Pos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BMTECHRD.Pos.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
            .HasMaxLength(60)
            .IsRequired();

        builder.HasIndex(x => new { x.BusinessId, x.Username }).IsUnique();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.PinHash)
            .HasMaxLength(250);

        builder.Property(x => x.Role).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
    }
}
