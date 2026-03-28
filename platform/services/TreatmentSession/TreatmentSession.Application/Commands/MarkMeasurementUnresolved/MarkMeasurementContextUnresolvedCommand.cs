using Intercessor.Abstractions;

namespace TreatmentSession.Application.Commands.MarkMeasurementUnresolved;

public sealed record MarkMeasurementContextUnresolvedCommand(
    Ulid CorrelationId,
    Ulid SessionId,
    string MeasurementId,
    string Reason,
    string? AuthenticatedUserId = null) : ICommand<bool>;
