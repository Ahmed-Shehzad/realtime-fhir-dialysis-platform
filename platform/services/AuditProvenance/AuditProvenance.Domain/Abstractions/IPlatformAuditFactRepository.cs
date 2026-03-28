using BuildingBlocks.Abstractions;

namespace AuditProvenance.Domain.Abstractions;

public interface IPlatformAuditFactRepository : IRepository<PlatformAuditFact>
{
    Task<PlatformAuditFact?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlatformAuditFactSummary>> GetRecentSummariesAsync(int count, CancellationToken cancellationToken = default);
}

/// <summary>Read model for recent audit facts.</summary>
public sealed record PlatformAuditFactSummary(
    Ulid Id,
    DateTimeOffset OccurredAtUtc,
    string EventType,
    string Summary,
    string SourceSystem);
