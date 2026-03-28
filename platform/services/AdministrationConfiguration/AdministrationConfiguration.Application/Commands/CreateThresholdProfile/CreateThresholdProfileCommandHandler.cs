using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.Abstractions;
using AdministrationConfiguration.Domain.ValueObjects;

using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Commands.CreateThresholdProfile;

public sealed class CreateThresholdProfileCommandHandler : ICommandHandler<CreateThresholdProfileCommand, Ulid>
{
    private readonly IThresholdProfileRepository _profiles;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public CreateThresholdProfileCommandHandler(
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

    public async Task<Ulid> HandleAsync(
        CreateThresholdProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var code = new ThresholdProfileCode(command.ProfileCode);
        var payload = new ThresholdProfilePayload(command.PayloadJson);
        ThresholdProfile profile = ThresholdProfile.Create(command.CorrelationId, code, payload, _tenant.TenantId);
        await _profiles.AddAsync(profile, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Create,
                    "ThresholdProfile",
                    profile.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Threshold profile created ({code.Value}).",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return profile.Id;
    }
}
