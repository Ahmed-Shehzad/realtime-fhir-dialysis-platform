using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using ReplayRecovery.Domain;
using ReplayRecovery.Domain.Abstractions;
using ReplayRecovery.Domain.ValueObjects;

using Intercessor.Abstractions;

namespace ReplayRecovery.Application.Commands.StartReplayJob;

public sealed class StartReplayJobCommandHandler : ICommandHandler<StartReplayJobCommand, Ulid>
{
    private readonly IReplayJobRepository _jobs;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public StartReplayJobCommandHandler(
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

    public async Task<Ulid> HandleAsync(
        StartReplayJobCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.ProjectionSetName))
            throw new ArgumentException("ProjectionSetName is required.", nameof(command));

        var mode = ReplayMode.ParseApi(command.ReplayMode);
        var projection = new ProjectionSetName(command.ProjectionSetName);
        ReplayJob job = ReplayJob.Start(command.CorrelationId, mode, projection, _tenant.TenantId);

        await _jobs.AddAsync(job, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Create,
                    "ReplayJob",
                    job.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Replay job started ({mode.Value}, projection {projection.Value}).",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return job.Id;
    }
}
