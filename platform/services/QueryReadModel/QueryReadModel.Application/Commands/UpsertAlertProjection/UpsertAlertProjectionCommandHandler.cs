using QueryReadModel.Domain;
using QueryReadModel.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

namespace QueryReadModel.Application.Commands.UpsertAlertProjection;

public sealed class UpsertAlertProjectionCommandHandler
    : ICommandHandler<UpsertAlertProjectionCommand, bool>
{
    private readonly IAlertProjectionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public UpsertAlertProjectionCommandHandler(
        IAlertProjectionRepository repository,
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
        UpsertAlertProjectionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        AlertProjection? existing = await _repository
            .GetByAlertRowKeyForUpdateAsync(command.AlertRowKey, cancellationToken)
            .ConfigureAwait(false);
        if (existing is null)
        {
            AlertProjection row = AlertProjection.Create(
                command.AlertRowKey,
                command.AlertType,
                command.Severity,
                command.AlertState,
                command.TreatmentSessionId,
                command.RaisedAtUtc);
            await _repository.AddAsync(row, cancellationToken).ConfigureAwait(false);
            await RecordAuditAsync(row.Id.ToString(), command.AuthenticatedUserId, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            existing.UpdateAlert(
                command.AlertType,
                command.Severity,
                command.AlertState,
                command.TreatmentSessionId);
            await RecordAuditAsync(existing.Id.ToString(), command.AuthenticatedUserId, cancellationToken).ConfigureAwait(false);
        }

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private Task RecordAuditAsync(string resourceId, string? userId, CancellationToken cancellationToken) =>
        _audit.RecordAsync(
            new AuditRecordRequest(
                AuditAction.Execute,
                "AlertProjection",
                resourceId,
                userId,
                AuditOutcome.Success,
                "Upsert alert projection.",
                TenantId: _tenant.TenantId,
                CorrelationId: null),
            cancellationToken);
}
