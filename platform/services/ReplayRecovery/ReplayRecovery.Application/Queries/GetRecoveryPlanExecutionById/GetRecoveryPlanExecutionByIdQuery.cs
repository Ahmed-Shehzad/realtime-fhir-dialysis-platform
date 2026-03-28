using Intercessor.Abstractions;

namespace ReplayRecovery.Application.Queries.GetRecoveryPlanExecutionById;

public sealed record GetRecoveryPlanExecutionByIdQuery(Ulid ExecutionId)
    : IQuery<RecoveryPlanExecutionReadDto?>;
