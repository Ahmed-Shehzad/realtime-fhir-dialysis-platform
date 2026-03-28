using BuildingBlocks.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace WorkflowOrchestrator.Infrastructure.Persistence;

public sealed class EfWorkflowOrchestratorAuditEventStore : IAuditEventStore
{
    private readonly WorkflowOrchestratorDbContext _db;

    public EfWorkflowOrchestratorAuditEventStore(WorkflowOrchestratorDbContext db) =>
        _db = db ?? throw new ArgumentNullException(nameof(db));

    public Task AppendAsync(AuditRecordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        string resourceType = request.ResourceType.Trim();
        if (resourceType.Length > WorkflowOrchestratorSecurityAuditLogConfiguration.MaxResourceTypeLength)
            resourceType = resourceType[..WorkflowOrchestratorSecurityAuditLogConfiguration.MaxResourceTypeLength];

        static string? Truncate(string? value, int max)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            string t = value.Trim();
            return t.Length <= max ? t : t[..max];
        }

        var row = new WorkflowOrchestratorSecurityAuditLogEntry
        {
            Id = Guid.NewGuid(),
            OccurredAtUtc = DateTimeOffset.UtcNow,
            Action = (int)request.Action,
            ResourceType = resourceType,
            ResourceId = Truncate(request.ResourceId, WorkflowOrchestratorSecurityAuditLogConfiguration.MaxResourceIdLength),
            UserId = Truncate(request.UserId, WorkflowOrchestratorSecurityAuditLogConfiguration.MaxUserIdLength),
            Outcome = (int)request.Outcome,
            Description = Truncate(request.Description, WorkflowOrchestratorSecurityAuditLogConfiguration.MaxDescriptionLength),
            TenantId = Truncate(request.TenantId, WorkflowOrchestratorSecurityAuditLogConfiguration.MaxTenantIdLength),
            CorrelationId = Truncate(request.CorrelationId, WorkflowOrchestratorSecurityAuditLogConfiguration.MaxCorrelationIdLength),
        };
        _ = _db.WorkflowOrchestratorSecurityAuditLogEntries.Add(row);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<AuditRecordRequest>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        int take = Math.Clamp(count, 1, 10_000);
        List<WorkflowOrchestratorSecurityAuditLogEntry> rows = await _db.WorkflowOrchestratorSecurityAuditLogEntries
            .AsNoTracking()
            .OrderByDescending(e => e.OccurredAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<AuditRecordRequest> result = new(rows.Count);
        foreach (WorkflowOrchestratorSecurityAuditLogEntry e in rows)
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
