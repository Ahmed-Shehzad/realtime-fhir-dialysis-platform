using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QueryReadModel.Domain;

namespace QueryReadModel.Infrastructure.Persistence;

internal sealed class AlertProjectionEntityConfiguration : IEntityTypeConfiguration<AlertProjection>
{
    public void Configure(EntityTypeBuilder<AlertProjection> entity)
    {
        _ = entity.ToTable("alert_projections");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.AlertRowKey).HasMaxLength(AlertProjection.MaxAlertIdLength).IsRequired();
        _ = entity.HasIndex(e => e.AlertRowKey).IsUnique();
        _ = entity.Property(e => e.AlertType).HasMaxLength(AlertProjection.MaxAlertTypeLength).IsRequired();
        _ = entity.Property(e => e.Severity).HasMaxLength(AlertProjection.MaxSeverityLength).IsRequired();
        _ = entity.Property(e => e.AlertState).HasMaxLength(AlertProjection.MaxAlertStateLength).IsRequired();
        _ = entity.Property(e => e.TreatmentSessionId).HasMaxLength(AlertProjection.MaxTreatmentSessionIdLength);
        _ = entity.Property(e => e.RaisedAtUtc).IsRequired();
        _ = entity.Property(e => e.ProjectionUpdatedAtUtc).IsRequired();
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
    }
}
