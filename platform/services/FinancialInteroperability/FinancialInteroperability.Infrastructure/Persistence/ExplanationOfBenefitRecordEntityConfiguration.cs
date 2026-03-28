using FinancialInteroperability.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialInteroperability.Infrastructure.Persistence;

internal sealed class ExplanationOfBenefitRecordEntityConfiguration : IEntityTypeConfiguration<ExplanationOfBenefitRecord>
{
    public void Configure(EntityTypeBuilder<ExplanationOfBenefitRecord> entity)
    {
        _ = entity.ToTable("explanation_of_benefit_records");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.DialysisFinancialClaimId)
            .HasConversion(v => v.ToString(), v => Ulid.Parse(v))
            .IsRequired();
        _ = entity.Property(e => e.TreatmentSessionId).HasMaxLength(ExplanationOfBenefitRecord.MaxTreatmentSessionIdLength).IsRequired();
        _ = entity
            .Property(e => e.FhirExplanationOfBenefitReference)
            .HasMaxLength(ExplanationOfBenefitRecord.MaxFhirEobReferenceLength)
            .IsRequired();
        _ = entity.Property(e => e.PatientResponsibilityAmount).HasPrecision(18, 4);
        _ = entity.Property(e => e.LinkedAtUtc).IsRequired();
        _ = entity.Property(e => e.TenantId).HasMaxLength(ExplanationOfBenefitRecord.MaxTenantIdLength);
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.Property(e => e.IsDeleted).IsRequired();
        _ = entity.HasIndex(e => e.DialysisFinancialClaimId).IsUnique();
    }
}
