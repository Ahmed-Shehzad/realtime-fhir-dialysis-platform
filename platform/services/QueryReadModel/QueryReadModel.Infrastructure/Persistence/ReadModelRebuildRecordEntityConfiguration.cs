using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QueryReadModel.Domain;

namespace QueryReadModel.Infrastructure.Persistence;

internal sealed class ReadModelRebuildRecordEntityConfiguration : IEntityTypeConfiguration<ReadModelRebuildRecord>
{
    public void Configure(EntityTypeBuilder<ReadModelRebuildRecord> entity)
    {
        _ = entity.ToTable("read_model_rebuild_records");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
    }
}
