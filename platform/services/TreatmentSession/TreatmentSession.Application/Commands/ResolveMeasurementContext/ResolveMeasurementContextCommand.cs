using Intercessor.Abstractions;

namespace TreatmentSession.Application.Commands.ResolveMeasurementContext;

public sealed record ResolveMeasurementContextCommand(
    Ulid CorrelationId,
    Ulid SessionId,
    string MeasurementId,
    string? AuthenticatedUserId = null) : ICommand<bool>;
