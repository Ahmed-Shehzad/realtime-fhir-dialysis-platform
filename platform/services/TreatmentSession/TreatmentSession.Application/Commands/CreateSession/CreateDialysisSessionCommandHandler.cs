using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

using TreatmentSession.Domain;
using TreatmentSession.Domain.Abstractions;

namespace TreatmentSession.Application.Commands.CreateSession;

/// <summary>Handles <see cref="CreateDialysisSessionCommand"/>.</summary>
public sealed class CreateDialysisSessionCommandHandler : ICommandHandler<CreateDialysisSessionCommand, CreateDialysisSessionResult>
{
    private readonly ISessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public CreateDialysisSessionCommandHandler(
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
    public async Task<CreateDialysisSessionResult> HandleAsync(
        CreateDialysisSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        DialysisSession session = DialysisSession.Create(command.CorrelationId, _tenant.TenantId);
        await _sessions.AddAsync(session, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Create,
                    "DialysisSession",
                    session.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    "Dialysis session created.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new CreateDialysisSessionResult(session.Id);
    }
}
