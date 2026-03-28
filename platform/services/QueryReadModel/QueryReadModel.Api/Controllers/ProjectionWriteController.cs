using System.Security.Claims;
using System.Text.Json.Serialization;

using Asp.Versioning;

using QueryReadModel.Application.Commands.RebuildReadModelProjections;
using QueryReadModel.Application.Commands.UpsertAlertProjection;
using QueryReadModel.Application.Commands.UpsertSessionOverviewProjection;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QueryReadModel.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/projections")]
public sealed class ProjectionWriteController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public ProjectionWriteController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost("rebuild")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReadModelWrite)]
    [ProducesResponseType(typeof(RebuildProjectionsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RebuildProjectionsResponse>> RebuildAsync(CancellationToken cancellationToken)
    {
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        int count = await _sender
            .SendAsync(new RebuildReadModelProjectionsCommand(correlationId, principalId), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new RebuildProjectionsResponse(count));
    }

    [HttpPost("session-overview")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReadModelWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpsertSessionOverviewAsync(
        [FromBody] UpsertSessionOverviewRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TreatmentSessionId);
        string? principalId = GetPrincipalObjectId();
        _ = await _sender
            .SendAsync(
                new UpsertSessionOverviewProjectionCommand(
                    request.TreatmentSessionId.Trim(),
                    request.SessionState,
                    request.PatientDisplayLabel,
                    request.LinkedDeviceId,
                    request.SessionStartedAtUtc,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("alerts")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReadModelWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpsertAlertAsync(
        [FromBody] UpsertAlertRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        string? principalId = GetPrincipalObjectId();
        _ = await _sender
            .SendAsync(
                new UpsertAlertProjectionCommand(
                    request.AlertRowKey,
                    request.AlertType,
                    request.Severity,
                    request.AlertState,
                    request.TreatmentSessionId,
                    request.RaisedAtUtc,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record RebuildProjectionsResponse(int ProjectionRowsSeeded);

public sealed record UpsertSessionOverviewRequest(
    string TreatmentSessionId,
    string SessionState,
    string? PatientDisplayLabel,
    string? LinkedDeviceId,
    [property: JsonRequired]
    DateTimeOffset SessionStartedAtUtc);

public sealed record UpsertAlertRequest(
    string AlertRowKey,
    string AlertType,
    string Severity,
    string AlertState,
    string? TreatmentSessionId,
    [property: JsonRequired]
    DateTimeOffset RaisedAtUtc);
