using System.Security.Claims;

using Asp.Versioning;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using ClinicalAnalytics.Application.Commands.RunSessionAnalysis;
using ClinicalAnalytics.Application.Queries.GetSessionAnalysisById;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicalAnalytics.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/clinical-analytics")]
public sealed class SessionAnalysisController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public SessionAnalysisController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost("sessions/{treatmentSessionId}/analyses")]
    [Authorize(Policy = PlatformAuthorizationPolicies.AnalyticsWrite)]
    [ProducesResponseType(typeof(RunSessionAnalysisResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RunSessionAnalysisResponse>> RunAnalysisAsync(
        string treatmentSessionId,
        [FromBody] RunSessionAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(treatmentSessionId);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        Ulid analysisId = await _sender
            .SendAsync(
                new RunSessionAnalysisCommand(
                    correlationId,
                    treatmentSessionId.Trim(),
                    request.ModelVersion.Trim(),
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return Ok(new RunSessionAnalysisResponse(analysisId.ToString()));
    }

    [HttpGet("analyses/{analysisId}")]
    [Authorize(Policy = PlatformAuthorizationPolicies.AnalyticsRead)]
    [ProducesResponseType(typeof(SessionAnalysisReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionAnalysisReadDto>> GetByIdAsync(
        string analysisId,
        CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(analysisId.Trim(), out Ulid id))
            return BadRequest();
        SessionAnalysisReadDto? row = await _sender
            .SendAsync(new GetSessionAnalysisByIdQuery(id), cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return NotFound();
        return Ok(row);
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record RunSessionAnalysisRequest(string ModelVersion);

public sealed record RunSessionAnalysisResponse(string AnalysisId);
