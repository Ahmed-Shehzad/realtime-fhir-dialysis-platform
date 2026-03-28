using AdministrationConfiguration.Domain;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using AdministrationConfiguration.Domain.Abstractions;
using AdministrationConfiguration.Domain.ValueObjects;

using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Commands.ChangeThresholdProfile;

public sealed class ChangeThresholdProfileCommandHandler : ICommandHandler<ChangeThresholdProfileCommand>
{
    private readonly IThresholdProfileRepository _profiles;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public ChangeThresholdProfileCommandHandler(
        IThresholdProfileRepository profiles,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task HandleAsync(
        ChangeThresholdProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ThresholdProfile profile = await _profiles.GetByIdForUpdateAsync(command.ProfileId, cancellationToken).ConfigureAwait(false)
                                   ?? throw new InvalidOperationException("Threshold profile not found.");
        var payload = new ThresholdProfilePayload(command.PayloadJson);
        profile.ReplacePayload(command.CorrelationId, payload, _tenant.TenantId);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Update,
                    "ThresholdProfile",
                    profile.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Threshold profile changed ({profile.ProfileCode.Value}).",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
