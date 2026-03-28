namespace ReplayRecovery.Application.Queries.GetRecoveryPlanExecutionById;

public sealed record RecoveryPlanExecutionReadDto(
    string Id,
    string PlanCode,
    string State,
    string OutcomeSummary,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
