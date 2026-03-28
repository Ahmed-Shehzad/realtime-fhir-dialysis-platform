using System.Security.Claims;

using Asp.Versioning;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Reporting.Application.Commands.FinalizeSessionReport;
using Reporting.Application.Commands.GenerateSessionReport;
using Reporting.Application.Commands.PublishDiagnosticReport;
using Reporting.Application.Queries.GetSessionReportById;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Reporting.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/reporting")]
public sealed class SessionReportController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public SessionReportController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost("sessions/{treatmentSessionId}/reports")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReportingWrite)]
    [ProducesResponseType(typeof(GenerateSessionReportResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GenerateSessionReportResponse>> GenerateAsync(
        string treatmentSessionId,
        [FromBody] GenerateSessionReportRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(treatmentSessionId);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        List<SessionReportEvidenceDto>? evidence = request.Evidence is { Count: > 0 }
            ? request.Evidence.Select(e => new SessionReportEvidenceDto(e.Kind, e.Locator)).ToList()
            : null;
        Ulid reportId = await _sender
            .SendAsync(
                new GenerateSessionReportCommand(
                    correlationId,
                    treatmentSessionId.Trim(),
                    request.NarrativeVersion.Trim(),
                    evidence,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return Ok(new GenerateSessionReportResponse(reportId.ToString()));
    }

    [HttpPost("reports/{reportId}/finalize")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReportingWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> FinalizeAsync(string reportId, CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(reportId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(new FinalizeSessionReportCommand(correlationId, id, principalId), cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("reports/{reportId}/publication")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReportingWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PublishAsync(
        string reportId,
        [FromBody] PublishDiagnosticReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(reportId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(
                new PublishDiagnosticReportCommand(
                    correlationId,
                    id,
                    request?.PublicationTargetHint,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpGet("reports/{reportId}")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReportingRead)]
    [ProducesResponseType(typeof(SessionReportReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionReportReadDto>> GetByIdAsync(
        string reportId,
        CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(reportId.Trim(), out Ulid id))
            return BadRequest();
        SessionReportReadDto? row = await _sender
            .SendAsync(new GetSessionReportByIdQuery(id), cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return NotFound();
        return Ok(row);
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record EvidenceItemRequest(string Kind, string Locator);

public sealed record GenerateSessionReportRequest(string NarrativeVersion, IReadOnlyList<EvidenceItemRequest>? Evidence);

public sealed record GenerateSessionReportResponse(string ReportId);

public sealed record PublishDiagnosticReportRequest(string? PublicationTargetHint);
