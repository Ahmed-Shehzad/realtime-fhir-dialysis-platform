using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using ReplayRecovery.Domain;
using ReplayRecovery.Domain.Abstractions;

using Intercessor.Abstractions;

namespace ReplayRecovery.Application.Commands.CompleteReplayJob;

public sealed class CompleteReplayJobCommandHandler : ICommandHandler<CompleteReplayJobCommand>
{
    private readonly IReplayJobRepository _jobs;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public CompleteReplayJobCommandHandler(
        IReplayJobRepository jobs,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _jobs = jobs ?? throw new ArgumentNullException(nameof(jobs));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task HandleAsync(
        CompleteReplayJobCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ReplayJob? job = await _jobs
            .GetByIdForUpdateAsync(command.ReplayJobId, cancellationToken)
            .ConfigureAwait(false);
        if (job is null)
            throw new InvalidOperationException($"Replay job {command.ReplayJobId} was not found.");

        job.Complete(command.CorrelationId, _tenant.TenantId);

        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Update,
                    "ReplayJob",
                    job.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    "Replay job completed.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
