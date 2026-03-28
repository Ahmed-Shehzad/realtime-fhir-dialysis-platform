using System.Security.Claims;

using Asp.Versioning;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using RealtimeSurveillance.Application.Commands.UpdateSessionRiskSnapshot;
using RealtimeSurveillance.Application.Queries.GetSessionRiskSnapshot;

namespace RealtimeSurveillance.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/surveillance/sessions")]
public sealed class SessionRiskController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public SessionRiskController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpGet("{treatmentSessionId}/risk")]
    [Authorize(Policy = PlatformAuthorizationPolicies.SurveillanceRead)]
    [ProducesResponseType(typeof(SessionRiskReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionRiskReadDto>> GetRiskAsync(
        string treatmentSessionId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(treatmentSessionId);
        SessionRiskReadDto? row = await _sender
            .SendAsync(new GetSessionRiskSnapshotQuery(treatmentSessionId.Trim()), cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return NotFound();
        return Ok(row);
    }

    [HttpPut("{treatmentSessionId}/risk")]
    [Authorize(Policy = PlatformAuthorizationPolicies.SurveillanceWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PutRiskAsync(
        string treatmentSessionId,
        [FromBody] UpdateSessionRiskRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(treatmentSessionId);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(
                new UpdateSessionRiskSnapshotCommand(
                    correlationId,
                    treatmentSessionId.Trim(),
                    request.RiskLevel,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record UpdateSessionRiskRequest(string RiskLevel);
