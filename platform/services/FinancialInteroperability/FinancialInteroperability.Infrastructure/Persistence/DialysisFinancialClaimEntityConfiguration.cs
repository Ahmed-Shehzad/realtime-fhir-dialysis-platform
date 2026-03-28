using FinancialInteroperability.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialInteroperability.Infrastructure.Persistence;

internal sealed class DialysisFinancialClaimEntityConfiguration : IEntityTypeConfiguration<DialysisFinancialClaim>
{
    public void Configure(EntityTypeBuilder<DialysisFinancialClaim> entity)
    {
        _ = entity.ToTable("dialysis_financial_claims");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.TreatmentSessionId).HasMaxLength(DialysisFinancialClaim.MaxTreatmentSessionIdLength).IsRequired();
        _ = entity.Property(e => e.PatientId).HasMaxLength(DialysisFinancialClaim.MaxPatientIdLength).IsRequired();
        _ = entity.Property(e => e.PatientCoverageRegistrationId)
            .HasConversion(v => v.ToString(), v => Ulid.Parse(v))
            .IsRequired();
        _ = entity
            .Property(e => e.FhirEncounterReference)
            .HasMaxLength(DialysisFinancialClaim.MaxFhirEncounterReferenceLength);
        _ = entity.Property(e => e.ClaimUse).HasConversion<int>().IsRequired();
        _ = entity.Property(e => e.Status).HasConversion<int>().IsRequired();
        _ = entity.Property(e => e.ExternalClaimId).HasMaxLength(DialysisFinancialClaim.MaxExternalIdLength);
        _ = entity.Property(e => e.ExternalClaimResponseId).HasMaxLength(DialysisFinancialClaim.MaxExternalIdLength);
        _ = entity.Property(e => e.OutcomeDisplay).HasMaxLength(DialysisFinancialClaim.MaxOutcomeDisplayLength);
        _ = entity.Property(e => e.TenantId).HasMaxLength(DialysisFinancialClaim.MaxTenantIdLength);
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.Property(e => e.IsDeleted).IsRequired();
        _ = entity.HasIndex(e => new { e.TreatmentSessionId, e.TenantId });
    }
}
