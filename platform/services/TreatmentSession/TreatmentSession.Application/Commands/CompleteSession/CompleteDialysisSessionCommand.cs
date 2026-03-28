using Intercessor.Abstractions;

namespace TreatmentSession.Application.Commands.CompleteSession;

public sealed record CompleteDialysisSessionCommand(
    Ulid CorrelationId,
    Ulid SessionId,
    string? AuthenticatedUserId = null) : ICommand<bool>;
