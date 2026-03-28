using Intercessor.Abstractions;

namespace ReplayRecovery.Application.Commands.ExecuteRecoveryPlan;

public sealed record ExecuteRecoveryPlanCommand(
    Ulid CorrelationId,
    string PlanCode,
    string? AuthenticatedUserId = null) : ICommand<Ulid>;
