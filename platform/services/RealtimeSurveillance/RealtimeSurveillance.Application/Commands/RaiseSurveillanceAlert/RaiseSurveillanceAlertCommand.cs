using Intercessor.Abstractions;

namespace RealtimeSurveillance.Application.Commands.RaiseSurveillanceAlert;

public sealed record RaiseSurveillanceAlertCommand(
    Ulid CorrelationId,
    string TreatmentSessionId,
    string AlertType,
    string Severity,
    string? Detail,
    string? AuthenticatedUserId = null) : ICommand<Ulid>;
