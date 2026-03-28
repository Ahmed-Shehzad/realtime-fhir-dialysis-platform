using BuildingBlocks.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace AdministrationConfiguration.Infrastructure.Persistence;

public sealed class EfAdministrationConfigurationAuditEventStore : IAuditEventStore
{
    private readonly AdministrationConfigurationDbContext _db;

    public EfAdministrationConfigurationAuditEventStore(AdministrationConfigurationDbContext db) =>
        _db = db ?? throw new ArgumentNullException(nameof(db));

    public Task AppendAsync(AuditRecordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        string resourceType = request.ResourceType.Trim();
        if (resourceType.Length > AdministrationConfigurationSecurityAuditLogConfiguration.MaxResourceTypeLength)
            resourceType = resourceType[..AdministrationConfigurationSecurityAuditLogConfiguration.MaxResourceTypeLength];

        static string? Truncate(string? value, int max)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            string t = value.Trim();
            return t.Length <= max ? t : t[..max];
        }

        var row = new AdministrationConfigurationSecurityAuditLogEntry
        {
            Id = Guid.NewGuid(),
            OccurredAtUtc = DateTimeOffset.UtcNow,
            Action = (int)request.Action,
            ResourceType = resourceType,
            ResourceId = Truncate(request.ResourceId, AdministrationConfigurationSecurityAuditLogConfiguration.MaxResourceIdLength),
            UserId = Truncate(request.UserId, AdministrationConfigurationSecurityAuditLogConfiguration.MaxUserIdLength),
            Outcome = (int)request.Outcome,
            Description = Truncate(request.Description, AdministrationConfigurationSecurityAuditLogConfiguration.MaxDescriptionLength),
            TenantId = Truncate(request.TenantId, AdministrationConfigurationSecurityAuditLogConfiguration.MaxTenantIdLength),
            CorrelationId = Truncate(
                request.CorrelationId,
                AdministrationConfigurationSecurityAuditLogConfiguration.MaxCorrelationIdLength),
        };
        _ = _db.AdministrationConfigurationSecurityAuditLogEntries.Add(row);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<AuditRecordRequest>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        int take = Math.Clamp(count, 1, 10_000);
        List<AdministrationConfigurationSecurityAuditLogEntry> rows =
            await _db.AdministrationConfigurationSecurityAuditLogEntries
                .AsNoTracking()
                .OrderByDescending(e => e.OccurredAtUtc)
                .Take(take)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

        List<AuditRecordRequest> result = new(rows.Count);
        foreach (AdministrationConfigurationSecurityAuditLogEntry e in rows)
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
