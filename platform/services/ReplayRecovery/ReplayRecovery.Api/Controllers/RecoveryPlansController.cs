using System.Security.Claims;

using Asp.Versioning;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using ReplayRecovery.Application.Commands.ExecuteRecoveryPlan;
using ReplayRecovery.Application.Queries.GetRecoveryPlanExecutionById;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ReplayRecovery.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/replay-recovery/recovery-plans")]
public sealed class RecoveryPlansController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public RecoveryPlansController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost("execute")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReplayWrite)]
    [ProducesResponseType(typeof(ExecuteRecoveryPlanResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ExecuteRecoveryPlanResponse>> ExecuteAsync(
        [FromBody] ExecuteRecoveryPlanRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        Ulid id = await _sender
            .SendAsync(
                new ExecuteRecoveryPlanCommand(correlationId, request.PlanCode.Trim(), principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ExecuteRecoveryPlanResponse(id.ToString()));
    }

    [HttpGet("executions/{executionId}")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReplayRead)]
    [ProducesResponseType(typeof(RecoveryPlanExecutionReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecoveryPlanExecutionReadDto>> GetExecutionByIdAsync(
        string executionId,
        CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(executionId.Trim(), out Ulid id))
            return BadRequest();
        RecoveryPlanExecutionReadDto? row = await _sender
            .SendAsync(new GetRecoveryPlanExecutionByIdQuery(id), cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return NotFound();
        return Ok(row);
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record ExecuteRecoveryPlanRequest(string PlanCode);

public sealed record ExecuteRecoveryPlanResponse(string ExecutionId);
