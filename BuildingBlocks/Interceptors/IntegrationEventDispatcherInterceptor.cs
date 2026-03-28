using BuildingBlocks.Abstractions;
using BuildingBlocks.Persistence;

using Intercessor.Abstractions;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

using RealtimePlatform.MassTransit;

namespace BuildingBlocks.Interceptors;

/// <summary>
/// Stages catalog integration events into the MassTransit EF transactional outbox before save, then after a successful save clears aggregate buffers and performs best-effort in-process <see cref="IPublisher"/> dispatch.
/// Register after <see cref="DomainEventDispatcherInterceptor"/> so domain handlers can enqueue integration events first.
/// </summary>
/// <remarks>
/// Scoped registration aligns with <see cref="IPublishEndpoint"/> and <see cref="IPublisher"/> lifetimes.
/// </remarks>
public sealed class IntegrationEventDispatcherInterceptor : SaveChangesInterceptor
{
    private static readonly AsyncLocal<PendingIntegrationWork?> PendingCommit = new();

    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IPublisher _publisher;
    private readonly RealtimePlatform.Messaging.IMessageSerializer _serializer;
    private readonly OutboxPublisherOptions _publisherOptions;

    public IntegrationEventDispatcherInterceptor(
        IPublishEndpoint publishEndpoint,
        IPublisher publisher,
        RealtimePlatform.Messaging.IMessageSerializer serializer,
        IOptions<OutboxPublisherOptions> publisherOptions)
    {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        ArgumentNullException.ThrowIfNull(publisherOptions);
        _publisherOptions = publisherOptions.Value;
    }

    public async override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is DbContext context)
        {
            PendingIntegrationWork? pending = await TryStageMassTransitOutboxAsync(context, cancellationToken)
                .ConfigureAwait(false);
            if (pending is not null)
                PendingCommit.Value = pending;
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    public async override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        int rowsAffected = await base.SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
        if (PendingCommit.Value is not { } pending)
            return rowsAffected;

        try
        {
            foreach (AggregateRoot aggregate in pending.Aggregates)
                aggregate.ClearIntegrationEvents();

            foreach (IIntegrationEvent integrationEvent in pending.Events)
                try
                {
                    await _publisher.PublishAsync((dynamic)integrationEvent, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // In-process dispatch is best-effort; broker delivery uses MassTransit outbox.
                }
        }
        finally
        {
            PendingCommit.Value = null;
        }

        return rowsAffected;
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        PendingCommit.Value = null;
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private async Task<PendingIntegrationWork?> TryStageMassTransitOutboxAsync(
        DbContext dbContext,
        CancellationToken cancellationToken)
    {
        List<AggregateRoot> aggregates = dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(static e => e.State is EntityState.Added or EntityState.Modified)
            .Select(static e => e.Entity)
            .Where(static e => e.IntegrationEvents.Count != 0)
            .ToList();

        if (aggregates.Count == 0)
            return null;

        List<IIntegrationEvent> integrationEvents = [.. aggregates.SelectMany(static a => a.IntegrationEvents)];
        var messageIds = new HashSet<Ulid>();

        foreach (IIntegrationEvent integrationEvent in integrationEvents)
        {
            Ulid messageId = integrationEvent is global::BuildingBlocks.IntegrationEvent integration
                ? integration.EventId
                : Ulid.NewUlid();
            if (!messageIds.Add(messageId))
                continue;

            CatalogIntegrationEventTransport transport =
                IntegrationEventCatalogTransportMapper.ToTransport(integrationEvent, _serializer);

            await _publishEndpoint
                .Publish(
                    transport,
                    ctx =>
                    {
                        ctx.MessageId = Ulid.Parse(transport.MessageId).ToGuid();
                        if (_publisherOptions.SourceAddress is not null)
                            ctx.Headers.Set("Source-Address", _publisherOptions.SourceAddress.ToString());
                    },
                    cancellationToken)
                .ConfigureAwait(false);
        }

        return new PendingIntegrationWork(aggregates, integrationEvents);
    }

    private sealed record PendingIntegrationWork(List<AggregateRoot> Aggregates, List<IIntegrationEvent> Events);
}
