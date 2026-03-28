using Intercessor.Abstractions;

namespace RealtimeSurveillance.Application.Commands.EscalateSurveillanceAlert;

public sealed record EscalateSurveillanceAlertCommand(
    Ulid CorrelationId,
    Ulid AlertId,
    string EscalationDetail,
    string? AuthenticatedUserId = null) : ICommand;
