using BuildingBlocks;

using RealtimePlatform.IntegrationEventCatalog;

namespace QueryReadModel.Domain;

/// <summary>Outbox envelope for rebuild completion (projection set rebuilt).</summary>
public sealed class ReadModelRebuildRecord : AggregateRoot
{
    private ReadModelRebuildRecord()
    {
    }

    public static ReadModelRebuildRecord Emit(
        Ulid correlationId,
        string projectionSet,
        int recordCount,
        string? tenantId)
    {
        string set = (projectionSet ?? string.Empty).Trim();
        if (set.Length == 0)
            set = "all";

        var row = new ReadModelRebuildRecord();
        row.ApplyCreatedDateTime();
        row.ApplyEvent(
            new ReadModelProjectionRebuiltIntegrationEvent(correlationId, set, recordCount)
            {
                TenantId = tenantId,
            });
        return row;
    }
}
