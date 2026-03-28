using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.Abstractions;
using AdministrationConfiguration.Domain.ValueObjects;

using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Commands.UpsertFacilityConfiguration;

public sealed class UpsertFacilityConfigurationCommandHandler : ICommandHandler<UpsertFacilityConfigurationCommand>
{
    private readonly IFacilityConfigurationRepository _configurations;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public UpsertFacilityConfigurationCommandHandler(
        IFacilityConfigurationRepository configurations,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _configurations = configurations ?? throw new ArgumentNullException(nameof(configurations));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task HandleAsync(
        UpsertFacilityConfigurationCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var facilityId = new FacilityId(command.FacilityId);
        var payload = new ConfigurationPayload(command.ConfigurationJson);
        EffectiveDateRange? range = command.EffectiveFromUtc.HasValue || command.EffectiveToUtc.HasValue
            ? new EffectiveDateRange(command.EffectiveFromUtc, command.EffectiveToUtc)
            : null;
        range?.ThrowIfInvalid();

        FacilityConfiguration? existing =
            await _configurations.GetByFacilityIdForUpdateAsync(facilityId, cancellationToken).ConfigureAwait(false);
        if (existing is null)
        {
            FacilityConfiguration created = FacilityConfiguration.Create(
                command.CorrelationId,
                facilityId,
                payload,
                range,
                _tenant.TenantId);
            await _configurations.AddAsync(created, cancellationToken).ConfigureAwait(false);
            await RecordAuditAsync(
                    command,
                    AuditAction.Create,
                    "FacilityConfiguration",
                    created.Id.ToString(),
                    "Facility configuration created.")
                .ConfigureAwait(false);
        }
        else
        {
            existing.Update(command.CorrelationId, payload, range, _tenant.TenantId);
            await RecordAuditAsync(
                    command,
                    AuditAction.Update,
                    "FacilityConfiguration",
                    existing.Id.ToString(),
                    "Facility configuration updated.")
                .ConfigureAwait(false);
        }

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task RecordAuditAsync(
        UpsertFacilityConfigurationCommand command,
        AuditAction action,
        string resourceType,
        string resourceId,
        string description)
    {
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    action,
                    resourceType,
                    resourceId,
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    description,
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()))
            .ConfigureAwait(false);
    }
}
