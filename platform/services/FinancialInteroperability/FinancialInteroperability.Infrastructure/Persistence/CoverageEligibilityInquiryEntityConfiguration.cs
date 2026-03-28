using FinancialInteroperability.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialInteroperability.Infrastructure.Persistence;

internal sealed class CoverageEligibilityInquiryEntityConfiguration : IEntityTypeConfiguration<CoverageEligibilityInquiry>
{
    public void Configure(EntityTypeBuilder<CoverageEligibilityInquiry> entity)
    {
        _ = entity.ToTable("coverage_eligibility_inquiries");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.PatientCoverageRegistrationId)
            .HasConversion(v => v.ToString(), v => Ulid.Parse(v))
            .IsRequired();
        _ = entity.Property(e => e.PatientId).HasMaxLength(PatientCoverageRegistration.MaxPatientIdLength).IsRequired();
        _ = entity.Property(e => e.Status).HasConversion<int>().IsRequired();
        _ = entity.Property(e => e.OutcomeCode).HasMaxLength(CoverageEligibilityInquiry.MaxOutcomeCodeLength).IsRequired();
        _ = entity.Property(e => e.Notes).HasMaxLength(CoverageEligibilityInquiry.MaxNotesLength);
        _ = entity.Property(e => e.TenantId).HasMaxLength(CoverageEligibilityInquiry.MaxTenantIdLength);
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.Property(e => e.IsDeleted).IsRequired();
        _ = entity.HasIndex(e => new { e.PatientId, e.TenantId });
    }
}
