using QueryReadModel.Domain;
using QueryReadModel.Domain.Abstractions;

using QueryReadModel.Application.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

namespace QueryReadModel.Application.Commands.RebuildReadModelProjections;

public sealed class RebuildReadModelProjectionsCommandHandler
    : ICommandHandler<RebuildReadModelProjectionsCommand, int>
{
    private const string ProjectionSetName = "sessions+alerts";

    private readonly IReadModelProjectionMaintenance _maintenance;
    private readonly IReadModelRebuildRecordRepository _rebuildRecords;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public RebuildReadModelProjectionsCommandHandler(
        IReadModelProjectionMaintenance maintenance,
        IReadModelRebuildRecordRepository rebuildRecords,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _maintenance = maintenance ?? throw new ArgumentNullException(nameof(maintenance));
        _rebuildRecords = rebuildRecords ?? throw new ArgumentNullException(nameof(rebuildRecords));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<int> HandleAsync(
        RebuildReadModelProjectionsCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        int inserted = await _maintenance.ClearAndSeedStubAsync(cancellationToken).ConfigureAwait(false);

        ReadModelRebuildRecord envelope = ReadModelRebuildRecord.Emit(
            command.CorrelationId,
            ProjectionSetName,
            inserted,
            _tenant.TenantId);
        await _rebuildRecords.AddAsync(envelope, cancellationToken).ConfigureAwait(false);

        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Execute,
                    "ReadModelRebuild",
                    envelope.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Read model rebuild completed; projection rows={inserted}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return inserted;
    }
}
