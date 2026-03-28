using Intercessor.Abstractions;

namespace RealtimeSurveillance.Application.Commands.ResolveSurveillanceAlert;

public sealed record ResolveSurveillanceAlertCommand(
    Ulid CorrelationId,
    Ulid AlertId,
    string? ResolutionNote,
    string? AuthenticatedUserId = null) : ICommand;
