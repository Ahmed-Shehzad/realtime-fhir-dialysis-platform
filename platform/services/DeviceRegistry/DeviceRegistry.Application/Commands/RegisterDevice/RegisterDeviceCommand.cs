using Intercessor.Abstractions;

namespace DeviceRegistry.Application.Commands.RegisterDevice;

/// <summary>
/// Registers a device in the registry (idempotent on duplicate business id — fails if exists).
/// </summary>
/// <param name="CorrelationId">Correlation for tracing and audit.</param>
/// <param name="DeviceIdentifier">Business device id.</param>
/// <param name="Manufacturer">Optional manufacturer.</param>
/// <param name="InitialTrustState">Initial trust string.</param>
/// <param name="AuthenticatedUserId">Entra <c>oid</c> or name identifier for audit (optional when bypass is used).</param>
public sealed record RegisterDeviceCommand(
    Ulid CorrelationId,
    string DeviceIdentifier,
    string? Manufacturer,
    string InitialTrustState,
    string? AuthenticatedUserId = null) : ICommand<RegisterDeviceResult>;
