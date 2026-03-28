using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

using TreatmentSession.Domain;
using TreatmentSession.Domain.Abstractions;

namespace TreatmentSession.Application.Commands.MarkMeasurementUnresolved;

public sealed class MarkMeasurementContextUnresolvedCommandHandler
    : ICommandHandler<MarkMeasurementContextUnresolvedCommand, bool>
{
    private readonly ISessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public MarkMeasurementContextUnresolvedCommandHandler(
        ISessionRepository sessions,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    /// <inheritdoc />
    public async Task<bool> HandleAsync(
        MarkMeasurementContextUnresolvedCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        DialysisSession? session = await _sessions
            .GetByIdAsync(command.SessionId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Session '{command.SessionId}' was not found.");
        session.MarkMeasurementContextUnresolved(
            command.CorrelationId,
            command.MeasurementId,
            command.Reason,
            _tenant.TenantId);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Update,
                    "DialysisSession",
                    session.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    "Measurement context marked unresolved for session.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
