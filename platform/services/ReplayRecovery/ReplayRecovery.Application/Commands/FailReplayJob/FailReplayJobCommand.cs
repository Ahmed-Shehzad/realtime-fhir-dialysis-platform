using Intercessor.Abstractions;

namespace ReplayRecovery.Application.Commands.FailReplayJob;

public sealed record FailReplayJobCommand(
    Ulid CorrelationId,
    Ulid ReplayJobId,
    string Reason,
    string? AuthenticatedUserId = null) : ICommand;
