using Intercessor.Abstractions;

namespace ReplayRecovery.Application.Commands.AdvanceReplayCheckpoint;

public sealed record AdvanceReplayCheckpointCommand(
    Ulid CorrelationId,
    Ulid ReplayJobId,
    string? AuthenticatedUserId = null) : ICommand;
