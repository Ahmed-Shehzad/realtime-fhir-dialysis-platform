using System.Security.Claims;

using Asp.Versioning;

using SignalConditioning.Application.Commands.ConditionSignal;
using SignalConditioning.Application.Queries.GetLatestConditioning;
using SignalConditioning.Domain.Abstractions;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SignalConditioning.Api.Controllers;

/// <summary>Signal conditioning API (v1).</summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/measurements")]
public sealed class ConditioningController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public ConditioningController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost("{measurementId}/conditioning")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ConditioningWrite)]
    [ProducesResponseType(typeof(ConditionSignalResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ConditionSignalResponse>> ConditionAsync(
        string measurementId,
        [FromBody] ConditionSignalRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(measurementId);
        ArgumentNullException.ThrowIfNull(request);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        ConditionSignalResult result = await _sender
            .SendAsync(
                new ConditionSignalCommand(
                    correlationId,
                    measurementId.Trim(),
                    request.ChannelId,
                    request.SampleValue,
                    request.PreviousSampleValue,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return StatusCode(
            StatusCodes.Status201Created,
            new ConditionSignalResponse(
                result.ConditioningResultId.ToString(),
                result.IsDropout,
                result.QualityScorePercent));
    }

    [HttpGet("{measurementId}/conditioning/latest")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ConditioningRead)]
    [ProducesResponseType(typeof(ConditioningLatestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConditioningLatestResponse>> GetLatestAsync(
        string measurementId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(measurementId);
        ConditioningSummary? row = await _sender
            .SendAsync(new GetLatestConditioningQuery(measurementId.Trim()), cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return NotFound();
        return Ok(
            new ConditioningLatestResponse(
                row.Id.ToString(),
                row.MeasurementId,
                row.ChannelId,
                row.IsDropout,
                row.DriftDetected,
                row.QualityScorePercent,
                row.ConditioningMethodVersion,
                row.ConditionedSignalKind,
                row.EvaluatedAtUtc));
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record ConditionSignalRequest(
    string ChannelId,
    double? SampleValue,
    double? PreviousSampleValue);

public sealed record ConditionSignalResponse(string ConditioningResultId, bool IsDropout, int QualityScorePercent);

public sealed record ConditioningLatestResponse(
    string Id,
    string MeasurementId,
    string ChannelId,
    bool IsDropout,
    bool DriftDetected,
    int QualityScorePercent,
    string ConditioningMethodVersion,
    string? ConditionedSignalKind,
    DateTimeOffset EvaluatedAtUtc);
