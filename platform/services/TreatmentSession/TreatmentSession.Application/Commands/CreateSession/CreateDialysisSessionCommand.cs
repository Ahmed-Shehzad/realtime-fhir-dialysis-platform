using Intercessor.Abstractions;

namespace TreatmentSession.Application.Commands.CreateSession;

/// <summary>Creates a dialysis session in Created state.</summary>
public sealed record CreateDialysisSessionCommand(
    Ulid CorrelationId,
    string? AuthenticatedUserId = null) : ICommand<CreateDialysisSessionResult>;
