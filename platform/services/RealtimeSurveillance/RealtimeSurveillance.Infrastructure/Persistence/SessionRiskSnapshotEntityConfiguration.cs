using BuildingBlocks.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RealtimeSurveillance.Domain;
using RealtimeSurveillance.Domain.ValueObjects;

namespace RealtimeSurveillance.Infrastructure.Persistence;

internal sealed class SessionRiskSnapshotEntityConfiguration : IEntityTypeConfiguration<SessionRiskSnapshot>
{
    public const int MaxTreatmentSessionIdLength = 256;

    public void Configure(EntityTypeBuilder<SessionRiskSnapshot> entity)
    {
        _ = entity.ToTable("session_risk_snapshots");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity
            .Property(e => e.TreatmentSessionId)
            .HasConversion(v => v.Value, v => new SessionId(v))
            .HasMaxLength(MaxTreatmentSessionIdLength)
            .IsRequired();
        _ = entity.HasIndex(e => e.TreatmentSessionId).IsUnique();
        _ = entity
            .Property(e => e.RiskLevel)
            .HasConversion(v => v.Value, v => SessionRiskLevel.FromStored(v))
            .HasMaxLength(SessionRiskLevel.MaxLength)
            .IsRequired();
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
    }
}
