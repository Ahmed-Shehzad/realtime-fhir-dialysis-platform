using Intercessor.Abstractions;

namespace ReplayRecovery.Application.Commands.ResumeReplayJob;

public sealed record ResumeReplayJobCommand(
    Ulid CorrelationId,
    Ulid ReplayJobId,
    string? AuthenticatedUserId = null) : ICommand;
