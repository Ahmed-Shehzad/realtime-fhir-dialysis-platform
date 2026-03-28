using QueryReadModel.Domain;
using QueryReadModel.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

namespace QueryReadModel.Application.Commands.UpsertSessionOverviewProjection;

public sealed class UpsertSessionOverviewProjectionCommandHandler
    : ICommandHandler<UpsertSessionOverviewProjectionCommand, bool>
{
    private readonly ISessionOverviewProjectionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public UpsertSessionOverviewProjectionCommandHandler(
        ISessionOverviewProjectionRepository repository,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<bool> HandleAsync(
        UpsertSessionOverviewProjectionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        SessionOverviewProjection? existing = await _repository
            .GetByTreatmentSessionIdForUpdateAsync(command.TreatmentSessionId, cancellationToken)
            .ConfigureAwait(false);
        if (existing is null)
        {
            SessionOverviewProjection row = SessionOverviewProjection.Create(
                command.TreatmentSessionId,
                command.SessionState,
                command.PatientDisplayLabel,
                command.LinkedDeviceId,
                command.SessionStartedAtUtc);
            await _repository.AddAsync(row, cancellationToken).ConfigureAwait(false);
            await RecordAuditAsync(row.Id.ToString(), command.AuthenticatedUserId, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            existing.UpdateOverview(
                command.SessionState,
                command.PatientDisplayLabel,
                command.LinkedDeviceId,
                command.SessionStartedAtUtc);
            await RecordAuditAsync(existing.Id.ToString(), command.AuthenticatedUserId, cancellationToken).ConfigureAwait(false);
        }

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private Task RecordAuditAsync(string resourceId, string? userId, CancellationToken cancellationToken) =>
        _audit.RecordAsync(
            new AuditRecordRequest(
                AuditAction.Execute,
                "SessionOverviewProjection",
                resourceId,
                userId,
                AuditOutcome.Success,
                "Upsert session overview projection.",
                TenantId: _tenant.TenantId,
                CorrelationId: null),
            cancellationToken);
}
