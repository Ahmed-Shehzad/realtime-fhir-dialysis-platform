using BuildingBlocks.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TreatmentSession.Domain;

namespace TreatmentSession.Infrastructure.Persistence;

internal sealed class DialysisSessionEntityConfiguration : IEntityTypeConfiguration<DialysisSession>
{
    public const int MaxMrnLength = 128;

    public const int MaxDeviceIdLength = 128;

    public void Configure(EntityTypeBuilder<DialysisSession> entity)
    {
        _ = entity.ToTable("dialysis_sessions");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.State).HasConversion<int>().IsRequired();
        _ = entity.Property(e => e.AssignedPatientMrn)
            .HasConversion(
                v => v.HasValue ? v.Value.Value : null,
                v => v == null ? (MedicalRecordNumber?)null : new MedicalRecordNumber(v))
            .HasMaxLength(MaxMrnLength);
        _ = entity.Property(e => e.LinkedDeviceId)
            .HasConversion(
                v => v.HasValue ? v.Value.Value : null,
                v => v == null ? (DeviceId?)null : new DeviceId(v))
            .HasMaxLength(MaxDeviceIdLength);
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
    }
}
