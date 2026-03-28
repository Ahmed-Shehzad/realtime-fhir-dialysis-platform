using System.Security.Claims;

using System.Text.Json.Serialization;

using Asp.Versioning;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using RealtimeSurveillance.Application.Commands.EvaluateMapHypotensionRule;

namespace RealtimeSurveillance.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/surveillance/rules")]
public sealed class SurveillanceRulesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public SurveillanceRulesController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost("evaluate")]
    [Authorize(Policy = PlatformAuthorizationPolicies.SurveillanceWrite)]
    [ProducesResponseType(typeof(EvaluateMapHypotensionRuleResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EvaluateMapHypotensionRuleResult>> EvaluateAsync(
        [FromBody] EvaluateRuleRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        try
        {
            EvaluateMapHypotensionRuleResult result = await _sender
                .SendAsync(
                    new EvaluateMapHypotensionRuleCommand(
                        correlationId,
                        request.TreatmentSessionId,
                        request.RuleCode,
                        request.MetricValueMmHg,
                        principalId),
                    cancellationToken)
                .ConfigureAwait(false);
            return Ok(result);
        }
        catch (ArgumentException)
        {
            return BadRequest();
        }
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record EvaluateRuleRequest(
    string TreatmentSessionId,
    string RuleCode,
    [property: JsonRequired]
    double MetricValueMmHg);
