using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.ValueObjects;

namespace AdministrationConfiguration.Infrastructure.Persistence;

internal sealed class RuleSetEntityConfiguration : IEntityTypeConfiguration<RuleSet>
{
    public void Configure(EntityTypeBuilder<RuleSet> entity)
    {
        _ = entity.ToTable("rule_sets");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity
            .Property(e => e.Version)
            .HasConversion(v => v.Value, v => new RuleVersion(v))
            .HasMaxLength(RuleVersion.MaxLength)
            .IsRequired();
        _ = entity
            .Property(e => e.RulesDocument)
            .HasColumnName("RulesDocument")
            .HasConversion(v => v.Raw, v => new RulesDocumentPayload(v))
            .IsRequired();
        _ = entity.Property(e => e.IsPublished).IsRequired();
        _ = entity.Property(e => e.PublishedAtUtc);
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.Property(e => e.DeletedAtUtc);
        _ = entity.Property(e => e.IsDeleted).IsRequired();
    }
}
