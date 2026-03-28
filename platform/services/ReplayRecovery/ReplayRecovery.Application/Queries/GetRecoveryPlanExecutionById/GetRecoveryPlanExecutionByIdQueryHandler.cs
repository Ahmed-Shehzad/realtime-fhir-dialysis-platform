using ReplayRecovery.Domain;
using ReplayRecovery.Domain.Abstractions;

using Intercessor.Abstractions;

namespace ReplayRecovery.Application.Queries.GetRecoveryPlanExecutionById;

public sealed class GetRecoveryPlanExecutionByIdQueryHandler
    : IQueryHandler<GetRecoveryPlanExecutionByIdQuery, RecoveryPlanExecutionReadDto?>
{
    private readonly IRecoveryPlanExecutionRepository _executions;

    public GetRecoveryPlanExecutionByIdQueryHandler(IRecoveryPlanExecutionRepository executions) =>
        _executions = executions ?? throw new ArgumentNullException(nameof(executions));

    public async Task<RecoveryPlanExecutionReadDto?> HandleAsync(
        GetRecoveryPlanExecutionByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        RecoveryPlanExecution? row = await _executions
            .GetByIdAsync(query.ExecutionId, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return null;

        return new RecoveryPlanExecutionReadDto(
            row.Id.ToString(),
            row.PlanCode.Value,
            row.State.Value,
            row.OutcomeSummary,
            row.CreatedAtUtc,
            row.UpdatedAtUtc);
    }
}
