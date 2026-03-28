using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;
using BuildingBlocks.ValueObjects;

using Intercessor.Abstractions;

using RealtimeSurveillance.Domain;
using RealtimeSurveillance.Domain.Abstractions;
using RealtimeSurveillance.Domain.ValueObjects;

namespace RealtimeSurveillance.Application.Commands.UpdateSessionRiskSnapshot;

public sealed class UpdateSessionRiskSnapshotCommandHandler : ICommandHandler<UpdateSessionRiskSnapshotCommand>
{
    private readonly ISessionRiskSnapshotRepository _snapshots;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public UpdateSessionRiskSnapshotCommandHandler(
        ISessionRiskSnapshotRepository snapshots,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _snapshots = snapshots ?? throw new ArgumentNullException(nameof(snapshots));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task HandleAsync(
        UpdateSessionRiskSnapshotCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var sessionId = new SessionId(command.TreatmentSessionId);
        SessionRiskLevel level = SessionRiskLevel.FromRequiredString(command.RiskLevel);

        SessionRiskSnapshot? existing = await _snapshots
            .GetBySessionIdForUpdateAsync(command.TreatmentSessionId, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            SessionRiskSnapshot snapshot = SessionRiskSnapshot.Start(
                command.CorrelationId,
                sessionId,
                level,
                _tenant.TenantId);
            await _snapshots.AddAsync(snapshot, cancellationToken).ConfigureAwait(false);
        }
        else
            existing.UpdateRisk(command.CorrelationId, level, _tenant.TenantId);

        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Update,
                    "SessionRiskSnapshot",
                    command.TreatmentSessionId,
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Session risk updated to {level.Value}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
