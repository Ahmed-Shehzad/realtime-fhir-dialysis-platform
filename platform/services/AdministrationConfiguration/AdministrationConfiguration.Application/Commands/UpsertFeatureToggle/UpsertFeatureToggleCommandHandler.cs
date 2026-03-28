using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.Abstractions;
using AdministrationConfiguration.Domain.ValueObjects;

using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Commands.UpsertFeatureToggle;

public sealed class UpsertFeatureToggleCommandHandler : ICommandHandler<UpsertFeatureToggleCommand>
{
    private readonly IFeatureToggleRepository _toggles;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public UpsertFeatureToggleCommandHandler(
        IFeatureToggleRepository toggles,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _toggles = toggles ?? throw new ArgumentNullException(nameof(toggles));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task HandleAsync(
        UpsertFeatureToggleCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var key = new FeatureFlagKey(command.FeatureKey);
        FeatureToggle? existing =
            await _toggles.GetByFeatureKeyForUpdateAsync(key, cancellationToken).ConfigureAwait(false);
        if (existing is null)
        {
            FeatureToggle created = FeatureToggle.Create(command.CorrelationId, key, command.IsEnabled, _tenant.TenantId);
            await _toggles.AddAsync(created, cancellationToken).ConfigureAwait(false);
            await RecordAuditAsync(
                    command,
                    AuditAction.Create,
                    created.Id.ToString(),
                    "Feature toggle created.")
                .ConfigureAwait(false);
        }
        else
        {
            bool willEmit = existing.IsEnabled != command.IsEnabled;
            existing.SetEnabled(command.CorrelationId, command.IsEnabled, _tenant.TenantId);
            string msg = willEmit ? "Feature toggle updated." : "Feature toggle unchanged (no publication).";
            await RecordAuditAsync(command, AuditAction.Update, existing.Id.ToString(), msg).ConfigureAwait(false);
        }

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task RecordAuditAsync(
        UpsertFeatureToggleCommand command,
        AuditAction action,
        string resourceId,
        string description)
    {
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    action,
                    "FeatureToggle",
                    resourceId,
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    description,
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()))
            .ConfigureAwait(false);
    }
}
