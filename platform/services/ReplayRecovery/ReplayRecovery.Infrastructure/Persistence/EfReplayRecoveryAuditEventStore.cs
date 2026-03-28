using BuildingBlocks.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace ReplayRecovery.Infrastructure.Persistence;

public sealed class EfReplayRecoveryAuditEventStore : IAuditEventStore
{
    private readonly ReplayRecoveryDbContext _db;

    public EfReplayRecoveryAuditEventStore(ReplayRecoveryDbContext db) =>
        _db = db ?? throw new ArgumentNullException(nameof(db));

    public Task AppendAsync(AuditRecordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        string resourceType = request.ResourceType.Trim();
        if (resourceType.Length > ReplayRecoverySecurityAuditLogConfiguration.MaxResourceTypeLength)
            resourceType = resourceType[..ReplayRecoverySecurityAuditLogConfiguration.MaxResourceTypeLength];

        static string? Truncate(string? value, int max)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            string t = value.Trim();
            return t.Length <= max ? t : t[..max];
        }

        var row = new ReplayRecoverySecurityAuditLogEntry
        {
            Id = Guid.NewGuid(),
            OccurredAtUtc = DateTimeOffset.UtcNow,
            Action = (int)request.Action,
            ResourceType = resourceType,
            ResourceId = Truncate(request.ResourceId, ReplayRecoverySecurityAuditLogConfiguration.MaxResourceIdLength),
            UserId = Truncate(request.UserId, ReplayRecoverySecurityAuditLogConfiguration.MaxUserIdLength),
            Outcome = (int)request.Outcome,
            Description = Truncate(request.Description, ReplayRecoverySecurityAuditLogConfiguration.MaxDescriptionLength),
            TenantId = Truncate(request.TenantId, ReplayRecoverySecurityAuditLogConfiguration.MaxTenantIdLength),
            CorrelationId = Truncate(request.CorrelationId, ReplayRecoverySecurityAuditLogConfiguration.MaxCorrelationIdLength),
        };
        _ = _db.ReplayRecoverySecurityAuditLogEntries.Add(row);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<AuditRecordRequest>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        int take = Math.Clamp(count, 1, 10_000);
        List<ReplayRecoverySecurityAuditLogEntry> rows = await _db.ReplayRecoverySecurityAuditLogEntries
            .AsNoTracking()
            .OrderByDescending(e => e.OccurredAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<AuditRecordRequest> result = new(rows.Count);
        foreach (ReplayRecoverySecurityAuditLogEntry e in rows)
            result.Add(new AuditRecordRequest(
                (AuditAction)e.Action,
                e.ResourceType,
                e.ResourceId,
                e.UserId,
                (AuditOutcome)e.Outcome,
                e.Description,
                e.TenantId,
                e.CorrelationId));

        return result;
    }
}
