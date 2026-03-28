using Intercessor.Abstractions;

namespace ReplayRecovery.Application.Commands.CompleteReplayJob;

public sealed record CompleteReplayJobCommand(
    Ulid CorrelationId,
    Ulid ReplayJobId,
    string? AuthenticatedUserId = null) : ICommand;
