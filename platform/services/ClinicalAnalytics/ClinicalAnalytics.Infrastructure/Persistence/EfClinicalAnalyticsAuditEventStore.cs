using BuildingBlocks.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace ClinicalAnalytics.Infrastructure.Persistence;

public sealed class EfClinicalAnalyticsAuditEventStore : IAuditEventStore
{
    private readonly ClinicalAnalyticsDbContext _db;

    public EfClinicalAnalyticsAuditEventStore(ClinicalAnalyticsDbContext db) =>
        _db = db ?? throw new ArgumentNullException(nameof(db));

    public Task AppendAsync(AuditRecordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        string resourceType = request.ResourceType.Trim();
        if (resourceType.Length > ClinicalAnalyticsSecurityAuditLogConfiguration.MaxResourceTypeLength)
            resourceType = resourceType[..ClinicalAnalyticsSecurityAuditLogConfiguration.MaxResourceTypeLength];

        static string? Truncate(string? value, int max)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            string t = value.Trim();
            return t.Length <= max ? t : t[..max];
        }

        var row = new ClinicalAnalyticsSecurityAuditLogEntry
        {
            Id = Guid.NewGuid(),
            OccurredAtUtc = DateTimeOffset.UtcNow,
            Action = (int)request.Action,
            ResourceType = resourceType,
            ResourceId = Truncate(request.ResourceId, ClinicalAnalyticsSecurityAuditLogConfiguration.MaxResourceIdLength),
            UserId = Truncate(request.UserId, ClinicalAnalyticsSecurityAuditLogConfiguration.MaxUserIdLength),
            Outcome = (int)request.Outcome,
            Description = Truncate(request.Description, ClinicalAnalyticsSecurityAuditLogConfiguration.MaxDescriptionLength),
            TenantId = Truncate(request.TenantId, ClinicalAnalyticsSecurityAuditLogConfiguration.MaxTenantIdLength),
            CorrelationId = Truncate(
                request.CorrelationId,
                ClinicalAnalyticsSecurityAuditLogConfiguration.MaxCorrelationIdLength),
        };
        _ = _db.ClinicalAnalyticsSecurityAuditLogEntries.Add(row);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<AuditRecordRequest>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        int take = Math.Clamp(count, 1, 10_000);
        List<ClinicalAnalyticsSecurityAuditLogEntry> rows = await _db.ClinicalAnalyticsSecurityAuditLogEntries
            .AsNoTracking()
            .OrderByDescending(e => e.OccurredAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<AuditRecordRequest> result = new(rows.Count);
        foreach (ClinicalAnalyticsSecurityAuditLogEntry e in rows)
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
