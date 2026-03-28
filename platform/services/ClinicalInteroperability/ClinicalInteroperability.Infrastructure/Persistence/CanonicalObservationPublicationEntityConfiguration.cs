using ClinicalInteroperability.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicalInteroperability.Infrastructure.Persistence;

internal sealed class CanonicalObservationPublicationEntityConfiguration : IEntityTypeConfiguration<CanonicalObservationPublication>
{
    public void Configure(EntityTypeBuilder<CanonicalObservationPublication> entity)
    {
        _ = entity.ToTable("canonical_observation_publications");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.MeasurementId).HasMaxLength(CanonicalObservationPublication.MaxMeasurementIdLength).IsRequired();
        _ = entity.Property(e => e.ObservationId).HasMaxLength(CanonicalObservationPublication.MaxObservationIdLength).IsRequired();
        _ = entity.Property(e => e.FhirProfileUrl).HasMaxLength(CanonicalObservationPublication.MaxFhirProfileUrlLength);
        _ = entity.Property(e => e.State).HasConversion<int>().IsRequired();
        _ = entity.Property(e => e.FhirResourceReference).HasMaxLength(CanonicalObservationPublication.MaxFhirResourceReferenceLength);
        _ = entity.Property(e => e.LastFailureReason).HasMaxLength(CanonicalObservationPublication.MaxFailureReasonLength);
        _ = entity.Property(e => e.AttemptCount).IsRequired();
        _ = entity.Property(e => e.LastAttemptAtUtc).IsRequired();
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.HasIndex(e => new { e.MeasurementId, e.LastAttemptAtUtc });
    }
}
