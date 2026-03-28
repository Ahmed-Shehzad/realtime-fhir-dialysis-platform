using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QueryReadModel.Domain;

namespace QueryReadModel.Infrastructure.Persistence;

internal sealed class SessionOverviewProjectionEntityConfiguration : IEntityTypeConfiguration<SessionOverviewProjection>
{
    public void Configure(EntityTypeBuilder<SessionOverviewProjection> entity)
    {
        _ = entity.ToTable("session_overview_projections");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.TreatmentSessionId).HasMaxLength(SessionOverviewProjection.MaxTreatmentSessionIdLength).IsRequired();
        _ = entity.HasIndex(e => e.TreatmentSessionId).IsUnique();
        _ = entity.Property(e => e.SessionState).HasMaxLength(SessionOverviewProjection.MaxSessionStateLength).IsRequired();
        _ = entity.Property(e => e.PatientDisplayLabel).HasMaxLength(SessionOverviewProjection.MaxPatientDisplayLength);
        _ = entity.Property(e => e.LinkedDeviceId).HasMaxLength(SessionOverviewProjection.MaxDeviceIdLength);
        _ = entity.Property(e => e.SessionStartedAtUtc).IsRequired();
        _ = entity.Property(e => e.ProjectionUpdatedAtUtc).IsRequired();
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
    }
}
