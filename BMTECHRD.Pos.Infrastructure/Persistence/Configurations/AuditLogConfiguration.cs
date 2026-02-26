using BMTECHRD.Pos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BMTECHRD.Pos.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(al => al.Id);

        builder.Property(al => al.BusinessId)
            .IsRequired();

        builder.Property(al => al.ActorUserId)
            .IsRequired(false);

        builder.Property(al => al.Action)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(al => al.EntityType)
            .IsRequired(false)
            .HasMaxLength(60);

        builder.Property(al => al.EntityId)
            .IsRequired(false);

        builder.Property(al => al.DataJson)
            .IsRequired(false)
            .HasMaxLength(2000);

        builder.Property(al => al.CreatedAt)
            .IsRequired();

        // Índices para consultas y limpieza
        builder.HasIndex(al => al.BusinessId);
        builder.HasIndex(al => new { al.BusinessId, al.CreatedAt });
        builder.HasIndex(al => al.ActorUserId);
        builder.HasIndex(al => al.Action);

        builder.ToTable("AuditLogs");
    }
}
