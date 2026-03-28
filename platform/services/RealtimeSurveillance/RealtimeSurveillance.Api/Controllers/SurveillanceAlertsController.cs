using System.Security.Claims;

using Asp.Versioning;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using RealtimeSurveillance.Application.Commands.AcknowledgeSurveillanceAlert;
using RealtimeSurveillance.Application.Commands.EscalateSurveillanceAlert;
using RealtimeSurveillance.Application.Commands.RaiseSurveillanceAlert;
using RealtimeSurveillance.Application.Commands.ResolveSurveillanceAlert;
using RealtimeSurveillance.Application.Queries.GetSurveillanceAlertById;
using RealtimeSurveillance.Application.Queries.ListSurveillanceAlerts;

namespace RealtimeSurveillance.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/surveillance/alerts")]
public sealed class SurveillanceAlertsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public SurveillanceAlertsController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost]
    [Authorize(Policy = PlatformAuthorizationPolicies.SurveillanceWrite)]
    [ProducesResponseType(typeof(RaiseAlertResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RaiseAlertResponse>> RaiseAsync(
        [FromBody] RaiseSurveillanceAlertRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        Ulid alertId = await _sender
            .SendAsync(
                new RaiseSurveillanceAlertCommand(
                    correlationId,
                    request.TreatmentSessionId,
                    request.AlertType,
                    request.Severity,
                    request.Detail,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return Ok(new RaiseAlertResponse(alertId.ToString()));
    }

    [HttpGet("{alertId}")]
    [Authorize(Policy = PlatformAuthorizationPolicies.SurveillanceRead)]
    [ProducesResponseType(typeof(SurveillanceAlertReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SurveillanceAlertReadDto>> GetByIdAsync(
        string alertId,
        CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(alertId.Trim(), out Ulid id))
            return BadRequest();
        SurveillanceAlertReadDto? row = await _sender
            .SendAsync(new GetSurveillanceAlertByIdQuery(id), cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return NotFound();
        return Ok(row);
    }

    [HttpGet]
    [Authorize(Policy = PlatformAuthorizationPolicies.SurveillanceRead)]
    [ProducesResponseType(typeof(IReadOnlyList<SurveillanceAlertListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SurveillanceAlertListItemDto>>> ListAsync(
        [FromQuery] string? sessionId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<SurveillanceAlertListItemDto> rows = await _sender
            .SendAsync(new ListSurveillanceAlertsQuery(sessionId), cancellationToken)
            .ConfigureAwait(false);
        return Ok(rows);
    }

    [HttpPost("{alertId}/acknowledge")]
    [Authorize(Policy = PlatformAuthorizationPolicies.SurveillanceWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AcknowledgeAsync(
        string alertId,
        [FromBody] AcknowledgeAlertRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!Ulid.TryParse(alertId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        try
        {
            await _sender
                .SendAsync(
                    new AcknowledgeSurveillanceAlertCommand(
                        correlationId,
                        id,
                        request.AcknowledgedByUserId,
                        principalId),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }

        return NoContent();
    }

    [HttpPost("{alertId}/escalate")]
    [Authorize(Policy = PlatformAuthorizationPolicies.SurveillanceWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EscalateAsync(
        string alertId,
        [FromBody] EscalateAlertRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!Ulid.TryParse(alertId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        try
        {
            await _sender
                .SendAsync(
                    new EscalateSurveillanceAlertCommand(correlationId, id, request.EscalationDetail, principalId),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }

        return NoContent();
    }

    [HttpPost("{alertId}/resolve")]
    [Authorize(Policy = PlatformAuthorizationPolicies.SurveillanceWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ResolveAsync(
        string alertId,
        [FromBody] ResolveAlertRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!Ulid.TryParse(alertId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        try
        {
            await _sender
                .SendAsync(
                    new ResolveSurveillanceAlertCommand(correlationId, id, request.ResolutionNote, principalId),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }

        return NoContent();
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record RaiseSurveillanceAlertRequest(
    string TreatmentSessionId,
    string AlertType,
    string Severity,
    string? Detail);

public sealed record RaiseAlertResponse(string AlertId);

public sealed record AcknowledgeAlertRequest(string AcknowledgedByUserId);

public sealed record EscalateAlertRequest(string EscalationDetail);

public sealed record ResolveAlertRequest(string? ResolutionNote);
