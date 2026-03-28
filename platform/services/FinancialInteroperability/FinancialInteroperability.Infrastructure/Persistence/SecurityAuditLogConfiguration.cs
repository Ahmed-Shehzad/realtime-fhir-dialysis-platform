using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialInteroperability.Infrastructure.Persistence;

internal sealed class SecurityAuditLogConfiguration : IEntityTypeConfiguration<SecurityAuditLogEntry>
{
    public const int MaxResourceTypeLength = 128;
    public const int MaxResourceIdLength = 512;
    public const int MaxUserIdLength = 256;
    public const int MaxDescriptionLength = 2000;
    public const int MaxTenantIdLength = 128;
    public const int MaxCorrelationIdLength = 128;

    public void Configure(EntityTypeBuilder<SecurityAuditLogEntry> entity)
    {
        _ = entity.ToTable("financial_security_audit_log");
        _ = entity.HasKey(e => e.Id);
        _ = entity.HasIndex(e => e.OccurredAtUtc);
        _ = entity.Property(e => e.OccurredAtUtc).IsRequired();
        _ = entity.Property(e => e.Action).IsRequired();
        _ = entity.Property(e => e.ResourceType).HasMaxLength(MaxResourceTypeLength).IsRequired();
        _ = entity.Property(e => e.ResourceId).HasMaxLength(MaxResourceIdLength);
        _ = entity.Property(e => e.UserId).HasMaxLength(MaxUserIdLength);
        _ = entity.Property(e => e.Outcome).IsRequired();
        _ = entity.Property(e => e.Description).HasMaxLength(MaxDescriptionLength);
        _ = entity.Property(e => e.TenantId).HasMaxLength(MaxTenantIdLength);
        _ = entity.Property(e => e.CorrelationId).HasMaxLength(MaxCorrelationIdLength);
    }
}
