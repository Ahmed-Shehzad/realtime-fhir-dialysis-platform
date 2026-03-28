using System.Security.Claims;

using Asp.Versioning;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using WorkflowOrchestrator.Application.Commands.AdvanceWorkflowStep;
using WorkflowOrchestrator.Application.Commands.CompleteWorkflowInstance;
using WorkflowOrchestrator.Application.Commands.FailWorkflowInstance;
using WorkflowOrchestrator.Application.Commands.RequestWorkflowManualIntervention;
using WorkflowOrchestrator.Application.Commands.SignalWorkflowTimeout;
using WorkflowOrchestrator.Application.Commands.StartWorkflowInstance;
using WorkflowOrchestrator.Application.Commands.TriggerWorkflowCompensation;
using WorkflowOrchestrator.Application.Queries.GetWorkflowInstanceById;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WorkflowOrchestrator.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/workflow-orchestrator/workflows")]
public sealed class WorkflowInstanceController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public WorkflowInstanceController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost("start")]
    [Authorize(Policy = PlatformAuthorizationPolicies.WorkflowWrite)]
    [ProducesResponseType(typeof(StartWorkflowResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StartWorkflowResponse>> StartAsync(
        [FromBody] StartWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        Ulid id = await _sender
            .SendAsync(
                new StartWorkflowInstanceCommand(
                    correlationId,
                    request.WorkflowKind.Trim(),
                    request.TreatmentSessionId.Trim(),
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return Ok(new StartWorkflowResponse(id.ToString()));
    }

    [HttpPost("{workflowInstanceId}/advance")]
    [Authorize(Policy = PlatformAuthorizationPolicies.WorkflowWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AdvanceAsync(
        string workflowInstanceId,
        [FromBody] AdvanceWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!Ulid.TryParse(workflowInstanceId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(
                new AdvanceWorkflowStepCommand(correlationId, id, request.NextStepName.Trim(), principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("{workflowInstanceId}/complete")]
    [Authorize(Policy = PlatformAuthorizationPolicies.WorkflowWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CompleteAsync(string workflowInstanceId, CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(workflowInstanceId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(new CompleteWorkflowInstanceCommand(correlationId, id, principalId), cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("{workflowInstanceId}/fail")]
    [Authorize(Policy = PlatformAuthorizationPolicies.WorkflowWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> FailAsync(
        string workflowInstanceId,
        [FromBody] FailWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!Ulid.TryParse(workflowInstanceId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(
                new FailWorkflowInstanceCommand(correlationId, id, request.Reason.Trim(), principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("{workflowInstanceId}/compensation")]
    [Authorize(Policy = PlatformAuthorizationPolicies.WorkflowWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CompensationAsync(
        string workflowInstanceId,
        [FromBody] TriggerCompensationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!Ulid.TryParse(workflowInstanceId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(
                new TriggerWorkflowCompensationCommand(correlationId, id, request.Reason.Trim(), principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("{workflowInstanceId}/manual-intervention")]
    [Authorize(Policy = PlatformAuthorizationPolicies.WorkflowWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ManualInterventionAsync(
        string workflowInstanceId,
        [FromBody] ManualInterventionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!Ulid.TryParse(workflowInstanceId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(
                new RequestWorkflowManualInterventionCommand(correlationId, id, request.Detail.Trim(), principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("{workflowInstanceId}/timeout")]
    [Authorize(Policy = PlatformAuthorizationPolicies.WorkflowWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> TimeoutAsync(string workflowInstanceId, CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(workflowInstanceId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(new SignalWorkflowTimeoutCommand(correlationId, id, principalId), cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpGet("{workflowInstanceId}")]
    [Authorize(Policy = PlatformAuthorizationPolicies.WorkflowRead)]
    [ProducesResponseType(typeof(WorkflowInstanceReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowInstanceReadDto>> GetByIdAsync(
        string workflowInstanceId,
        CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(workflowInstanceId.Trim(), out Ulid id))
            return BadRequest();
        WorkflowInstanceReadDto? row = await _sender
            .SendAsync(new GetWorkflowInstanceByIdQuery(id), cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return NotFound();
        return Ok(row);
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record StartWorkflowRequest(string WorkflowKind, string TreatmentSessionId);

public sealed record StartWorkflowResponse(string WorkflowInstanceId);

public sealed record AdvanceWorkflowRequest(string NextStepName);

public sealed record FailWorkflowRequest(string Reason);

public sealed record TriggerCompensationRequest(string Reason);

public sealed record ManualInterventionRequest(string Detail);
