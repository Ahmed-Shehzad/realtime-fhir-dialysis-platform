using Intercessor.Abstractions;

namespace ReplayRecovery.Application.Commands.StartReplayJob;

public sealed record StartReplayJobCommand(
    Ulid CorrelationId,
    string ReplayMode,
    string ProjectionSetName,
    string? AuthenticatedUserId = null) : ICommand<Ulid>;
