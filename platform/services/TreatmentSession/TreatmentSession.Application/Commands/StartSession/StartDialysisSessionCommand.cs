using Intercessor.Abstractions;

namespace TreatmentSession.Application.Commands.StartSession;

public sealed record StartDialysisSessionCommand(
    Ulid CorrelationId,
    Ulid SessionId,
    string? AuthenticatedUserId = null) : ICommand<bool>;
