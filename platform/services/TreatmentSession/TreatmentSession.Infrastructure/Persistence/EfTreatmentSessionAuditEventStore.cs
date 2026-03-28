using BuildingBlocks.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace TreatmentSession.Infrastructure.Persistence;

/// <summary><see cref="IAuditEventStore"/> on <see cref="TreatmentSessionDbContext"/>; commit via <see cref="IUnitOfWork"/>.</summary>
public sealed class EfTreatmentSessionAuditEventStore : IAuditEventStore
{
    private readonly TreatmentSessionDbContext _db;

    public EfTreatmentSessionAuditEventStore(TreatmentSessionDbContext db) =>
        _db = db ?? throw new ArgumentNullException(nameof(db));

    /// <inheritdoc />
    public Task AppendAsync(AuditRecordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        string resourceType = request.ResourceType.Trim();
        if (resourceType.Length > SecurityAuditLogConfiguration.MaxResourceTypeLength)
            resourceType = resourceType[..SecurityAuditLogConfiguration.MaxResourceTypeLength];

        static string? Truncate(string? value, int max)
        {
            if (string.IsNullOrEmpty(value)) return value;
            string t = value.Trim();
            return t.Length <= max ? t : t[..max];
        }

        var row = new SecurityAuditLogEntry
        {
            Id = Guid.NewGuid(),
            OccurredAtUtc = DateTimeOffset.UtcNow,
            Action = (int)request.Action,
            ResourceType = resourceType,
            ResourceId = Truncate(request.ResourceId, SecurityAuditLogConfiguration.MaxResourceIdLength),
            UserId = Truncate(request.UserId, SecurityAuditLogConfiguration.MaxUserIdLength),
            Outcome = (int)request.Outcome,
            Description = Truncate(request.Description, SecurityAuditLogConfiguration.MaxDescriptionLength),
            TenantId = Truncate(request.TenantId, SecurityAuditLogConfiguration.MaxTenantIdLength),
            CorrelationId = Truncate(request.CorrelationId, SecurityAuditLogConfiguration.MaxCorrelationIdLength)
        };

        _ = _db.SecurityAuditLogEntries.Add(row);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AuditRecordRequest>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        int take = Math.Clamp(count, 1, 10_000);
        List<SecurityAuditLogEntry> rows = await _db.SecurityAuditLogEntries
            .AsNoTracking()
            .OrderByDescending(e => e.OccurredAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<AuditRecordRequest> result = new(rows.Count);
        foreach (SecurityAuditLogEntry e in rows)
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
