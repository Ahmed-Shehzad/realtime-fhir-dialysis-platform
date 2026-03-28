using Intercessor.Abstractions;

namespace ReplayRecovery.Application.Commands.PauseReplayJob;

public sealed record PauseReplayJobCommand(
    Ulid CorrelationId,
    Ulid ReplayJobId,
    string? AuthenticatedUserId = null) : ICommand;
