using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TerminologyConformance.Domain;

namespace TerminologyConformance.Infrastructure.Persistence;

internal sealed class ConformanceAssessmentEntityConfiguration : IEntityTypeConfiguration<ConformanceAssessment>
{
    public void Configure(EntityTypeBuilder<ConformanceAssessment> entity)
    {
        _ = entity.ToTable("conformance_assessments");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.ResourceId).HasMaxLength(ConformanceAssessment.MaxResourceIdLength).IsRequired();
        _ = entity.Property(e => e.TerminologySliceOutcome).HasConversion<int>().IsRequired();
        _ = entity.Property(e => e.ProfileSliceOutcome).HasConversion<int>().IsRequired();
        _ = entity.Property(e => e.TerminologyReason).HasMaxLength(ConformanceAssessment.MaxReasonLength);
        _ = entity.Property(e => e.ProfileReason).HasMaxLength(ConformanceAssessment.MaxReasonLength);
        _ = entity.Property(e => e.AssessedProfileUrl).HasMaxLength(ConformanceAssessment.MaxProfileUrlLength);
        _ = entity.Property(e => e.ProfileRuleRegistryVersion).HasMaxLength(128).IsRequired();
        _ = entity.Property(e => e.EvaluatedAtUtc).IsRequired();
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.HasIndex(e => new { e.ResourceId, e.EvaluatedAtUtc });
    }
}
