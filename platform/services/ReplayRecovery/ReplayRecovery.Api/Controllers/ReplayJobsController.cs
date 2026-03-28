using System.Security.Claims;

using Asp.Versioning;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using ReplayRecovery.Application.Commands.AdvanceReplayCheckpoint;
using ReplayRecovery.Application.Commands.CancelReplayJob;
using ReplayRecovery.Application.Commands.CompleteReplayJob;
using ReplayRecovery.Application.Commands.FailReplayJob;
using ReplayRecovery.Application.Commands.PauseReplayJob;
using ReplayRecovery.Application.Commands.ResumeReplayJob;
using ReplayRecovery.Application.Commands.StartReplayJob;
using ReplayRecovery.Application.Queries.GetReplayJobById;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ReplayRecovery.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/replay-recovery/replay-jobs")]
public sealed class ReplayJobsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public ReplayJobsController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost("start")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReplayWrite)]
    [ProducesResponseType(typeof(StartReplayJobResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StartReplayJobResponse>> StartAsync(
        [FromBody] StartReplayJobRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        Ulid id = await _sender
            .SendAsync(
                new StartReplayJobCommand(
                    correlationId,
                    request.ReplayMode.Trim(),
                    request.ProjectionSetName.Trim(),
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return Ok(new StartReplayJobResponse(id.ToString()));
    }

    [HttpPost("{replayJobId}/checkpoints")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReplayWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AdvanceCheckpointAsync(
        string replayJobId,
        CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(replayJobId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(new AdvanceReplayCheckpointCommand(correlationId, id, principalId), cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("{replayJobId}/pause")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReplayWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PauseAsync(string replayJobId, CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(replayJobId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(new PauseReplayJobCommand(correlationId, id, principalId), cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("{replayJobId}/resume")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReplayWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResumeAsync(string replayJobId, CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(replayJobId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(new ResumeReplayJobCommand(correlationId, id, principalId), cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("{replayJobId}/cancel")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReplayWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CancelAsync(string replayJobId, CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(replayJobId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(new CancelReplayJobCommand(correlationId, id, principalId), cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("{replayJobId}/complete")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReplayWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CompleteAsync(string replayJobId, CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(replayJobId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(new CompleteReplayJobCommand(correlationId, id, principalId), cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("{replayJobId}/fail")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReplayWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> FailAsync(
        string replayJobId,
        [FromBody] FailReplayJobRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!Ulid.TryParse(replayJobId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(
                new FailReplayJobCommand(correlationId, id, request.Reason.Trim(), principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpGet("{replayJobId}")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReplayRead)]
    [ProducesResponseType(typeof(ReplayJobReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReplayJobReadDto>> GetByIdAsync(
        string replayJobId,
        CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(replayJobId.Trim(), out Ulid id))
            return BadRequest();
        ReplayJobReadDto? row = await _sender
            .SendAsync(new GetReplayJobByIdQuery(id), cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return NotFound();
        return Ok(row);
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record StartReplayJobRequest(string ReplayMode, string ProjectionSetName);

public sealed record StartReplayJobResponse(string ReplayJobId);

public sealed record FailReplayJobRequest(string Reason);
