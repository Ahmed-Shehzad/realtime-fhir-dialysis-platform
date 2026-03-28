using Intercessor.Abstractions;

namespace ReplayRecovery.Application.Commands.CancelReplayJob;

public sealed record CancelReplayJobCommand(
    Ulid CorrelationId,
    Ulid ReplayJobId,
    string? AuthenticatedUserId = null) : ICommand;
