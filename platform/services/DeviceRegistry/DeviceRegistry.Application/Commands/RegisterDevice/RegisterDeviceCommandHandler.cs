using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;
using BuildingBlocks.ValueObjects;

using DeviceRegistry.Domain;
using DeviceRegistry.Domain.Abstractions;

using Intercessor.Abstractions;

namespace DeviceRegistry.Application.Commands.RegisterDevice;

/// <summary>
/// Handles <see cref="RegisterDeviceCommand"/>.
/// </summary>
public sealed class RegisterDeviceCommandHandler : ICommandHandler<RegisterDeviceCommand, RegisterDeviceResult>
{
    private readonly IDeviceRepository _devices;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    /// <summary>
    /// Creates a handler.
    /// </summary>
    public RegisterDeviceCommandHandler(
        IDeviceRepository devices,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _devices = devices ?? throw new ArgumentNullException(nameof(devices));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    /// <inheritdoc />
    public async Task<RegisterDeviceResult> HandleAsync(RegisterDeviceCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var deviceId = new DeviceId(command.DeviceIdentifier);
        Device? existing = await _devices
            .GetByDeviceIdAsync(deviceId, cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
            throw new InvalidOperationException($"Device '{command.DeviceIdentifier}' is already registered.");

        string? manufacturer = string.IsNullOrWhiteSpace(command.Manufacturer)
            ? null
            : command.Manufacturer.Trim();
        if (manufacturer is not null && manufacturer.Length > Device.MaxManufacturerLength)
            manufacturer = manufacturer[..Device.MaxManufacturerLength];

        TrustState trust = TrustState.From(command.InitialTrustState);
        Device device = Device.Register(command.CorrelationId, deviceId, trust, manufacturer, _tenant.TenantId);
        await _devices.AddAsync(device, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Create,
                    "Device",
                    deviceId.Value,
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    "Device registered.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new RegisterDeviceResult(device.Id, deviceId.Value, trust.Value);
    }
}
