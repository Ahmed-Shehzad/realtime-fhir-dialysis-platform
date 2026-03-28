using System.Security.Claims;

using Asp.Versioning;

using TerminologyConformance.Application.Commands.ValidateSemanticConformance;
using TerminologyConformance.Application.Queries.GetLatestConformanceAssessment;
using TerminologyConformance.Domain.Abstractions;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TerminologyConformance.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/resources")]
public sealed class SemanticConformanceController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public SemanticConformanceController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost("{resourceId}/semantic-conformance")]
    [Authorize(Policy = PlatformAuthorizationPolicies.TerminologyWrite)]
    [ProducesResponseType(typeof(ValidateSemanticConformanceResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ValidateSemanticConformanceResponse>> ValidateAsync(
        string resourceId,
        [FromBody] ValidateSemanticConformanceRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);
        ArgumentNullException.ThrowIfNull(request);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        ValidateSemanticConformanceResult result = await _sender
            .SendAsync(
                new ValidateSemanticConformanceCommand(
                    correlationId,
                    resourceId.Trim(),
                    request.CodeSystemUri,
                    request.CodeValue,
                    request.UnitCode,
                    request.ProfileUrl,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return StatusCode(
            StatusCodes.Status201Created,
            new ValidateSemanticConformanceResponse(
                result.AssessmentId.ToString(),
                result.TerminologyOutcome.ToString(),
                result.ProfileOutcome.ToString()));
    }

    [HttpGet("{resourceId}/semantic-conformance/latest")]
    [Authorize(Policy = PlatformAuthorizationPolicies.TerminologyRead)]
    [ProducesResponseType(typeof(ConformanceAssessmentLatestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConformanceAssessmentLatestResponse>> GetLatestAsync(
        string resourceId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);
        ConformanceAssessmentSummary? row = await _sender
            .SendAsync(
                new GetLatestConformanceAssessmentQuery(resourceId.Trim()),
                cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return NotFound();
        return Ok(
            new ConformanceAssessmentLatestResponse(
                row.Id.ToString(),
                row.ResourceId,
                row.TerminologyOutcome.ToString(),
                row.ProfileOutcome.ToString(),
                row.TerminologyReason,
                row.ProfileReason,
                row.AssessedProfileUrl,
                row.ProfileRuleRegistryVersion,
                row.EvaluatedAtUtc));
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record ValidateSemanticConformanceRequest(
    string? CodeSystemUri,
    string? CodeValue,
    string? UnitCode,
    string? ProfileUrl);

public sealed record ValidateSemanticConformanceResponse(
    string AssessmentId,
    string TerminologyOutcome,
    string ProfileOutcome);

public sealed record ConformanceAssessmentLatestResponse(
    string Id,
    string ResourceId,
    string TerminologyOutcome,
    string ProfileOutcome,
    string? TerminologyReason,
    string? ProfileReason,
    string? AssessedProfileUrl,
    string ProfileRuleRegistryVersion,
    DateTimeOffset EvaluatedAtUtc);
