using Microsoft.EntityFrameworkCore;

using WorkflowOrchestrator.Domain;
using WorkflowOrchestrator.Domain.Abstractions;

using BuildingBlocks;

namespace WorkflowOrchestrator.Infrastructure.Persistence;

public sealed class WorkflowInstanceRepository : Repository<WorkflowInstance>, IWorkflowInstanceRepository
{
    private readonly WorkflowOrchestratorDbContext _db;

    public WorkflowInstanceRepository(WorkflowOrchestratorDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<WorkflowInstance?> GetByIdAsync(Ulid workflowInstanceId, CancellationToken cancellationToken = default) =>
        _db.WorkflowInstances.AsNoTracking().FirstOrDefaultAsync(w => w.Id == workflowInstanceId, cancellationToken);

    public Task<WorkflowInstance?> GetByIdForUpdateAsync(Ulid workflowInstanceId, CancellationToken cancellationToken = default) =>
        _db.WorkflowInstances.FirstOrDefaultAsync(w => w.Id == workflowInstanceId, cancellationToken);
}
