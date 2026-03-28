using Intercessor.Abstractions;

namespace TreatmentSession.Application.Commands.LinkDevice;

public sealed record LinkDeviceToSessionCommand(
    Ulid CorrelationId,
    Ulid SessionId,
    string DeviceIdentifier,
    string? AuthenticatedUserId = null) : ICommand<bool>;
