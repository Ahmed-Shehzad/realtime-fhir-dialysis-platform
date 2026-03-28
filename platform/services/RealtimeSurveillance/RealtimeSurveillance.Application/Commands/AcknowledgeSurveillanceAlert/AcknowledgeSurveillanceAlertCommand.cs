using Intercessor.Abstractions;

namespace RealtimeSurveillance.Application.Commands.AcknowledgeSurveillanceAlert;

public sealed record AcknowledgeSurveillanceAlertCommand(
    Ulid CorrelationId,
    Ulid AlertId,
    string AcknowledgedByUserId,
    string? AuthenticatedUserId = null) : ICommand;
