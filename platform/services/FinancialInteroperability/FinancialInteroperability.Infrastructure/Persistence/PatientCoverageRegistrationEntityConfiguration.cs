using FinancialInteroperability.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialInteroperability.Infrastructure.Persistence;

internal sealed class PatientCoverageRegistrationEntityConfiguration : IEntityTypeConfiguration<PatientCoverageRegistration>
{
    public void Configure(EntityTypeBuilder<PatientCoverageRegistration> entity)
    {
        _ = entity.ToTable("patient_coverage_registrations");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.PatientId).HasMaxLength(PatientCoverageRegistration.MaxPatientIdLength).IsRequired();
        _ = entity.Property(e => e.MemberIdentifier).HasMaxLength(PatientCoverageRegistration.MaxMemberIdentifierLength).IsRequired();
        _ = entity.Property(e => e.PayorDisplayName).HasMaxLength(PatientCoverageRegistration.MaxPayorDisplayNameLength).IsRequired();
        _ = entity.Property(e => e.PlanDisplayName).HasMaxLength(PatientCoverageRegistration.MaxPlanDisplayNameLength).IsRequired();
        _ = entity.Property(e => e.PeriodStart).IsRequired();
        _ = entity.Property(e => e.PeriodEnd);
        _ = entity.Property(e => e.FhirCoverageResourceId).HasMaxLength(PatientCoverageRegistration.MaxFhirCoverageIdLength);
        _ = entity.Property(e => e.TenantId).HasMaxLength(PatientCoverageRegistration.MaxTenantIdLength);
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.Property(e => e.IsDeleted).IsRequired();
        _ = entity.HasIndex(e => new { e.PatientId, e.TenantId });
    }
}
