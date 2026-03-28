using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace RealtimePlatform.Persistence;

/// <summary>
/// Design-time <see cref="DbContext"/> containing only MassTransit transactional outbox + inbox tables.
/// </summary>
public sealed class SchemaOnlyDbContext : DbContext
{
    /// <summary>Creates the context for migrations.</summary>
    public SchemaOnlyDbContext(DbContextOptions<SchemaOnlyDbContext> options)
        : base(options)
    {
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddTransactionalOutboxEntities();
    }
}
