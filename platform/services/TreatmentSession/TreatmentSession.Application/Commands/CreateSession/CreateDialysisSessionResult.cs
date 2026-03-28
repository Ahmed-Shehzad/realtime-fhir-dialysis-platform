namespace TreatmentSession.Application.Commands.CreateSession;

/// <summary>Result of <see cref="CreateDialysisSessionCommand"/>.</summary>
public sealed record CreateDialysisSessionResult(Ulid SessionId);
