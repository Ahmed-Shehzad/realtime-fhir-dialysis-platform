using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Persistence;

/// <summary>
/// Applies MassTransit EF Core transactional outbox + inbox entity mappings (strict MassTransit schema).
/// </summary>
public static class MassTransitTransactionalOutboxModelExtensions
{
    /// <summary>
    /// Registers <c>InboxState</c>, <c>OutboxMessage</c>, and <c>OutboxState</c> via MassTransit.
    /// </summary>
    public static void AddMassTransitTransactionalOutboxAndInbox(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddTransactionalOutboxEntities();
    }
}
