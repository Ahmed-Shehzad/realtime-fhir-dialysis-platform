using System.Security.Claims;

using Asp.Versioning;

using MeasurementValidation.Application.Commands.ValidateMeasurement;
using MeasurementValidation.Application.Queries.GetLatestMeasurementValidation;
using MeasurementValidation.Domain.Abstractions;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeasurementValidation.Api.Controllers;

/// <summary>Measurement validation API (v1).</summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/measurements")]
public sealed class ValidationsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public ValidationsController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost("{measurementId}/validation")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ValidationWrite)]
    [ProducesResponseType(typeof(ValidateMeasurementResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ValidateMeasurementResponse>> ValidateAsync(
        string measurementId,
        [FromBody] ValidateMeasurementRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(measurementId);
        ArgumentNullException.ThrowIfNull(request);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        ValidateMeasurementResult result = await _sender
            .SendAsync(
                new ValidateMeasurementCommand(
                    correlationId,
                    measurementId.Trim(),
                    request.ValidationProfileId,
                    request.SampleValue,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return StatusCode(
            StatusCodes.Status201Created,
            new ValidateMeasurementResponse(result.ValidationId.ToString(), result.Outcome.ToString()));
    }

    [HttpGet("{measurementId}/validation/latest")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ValidationRead)]
    [ProducesResponseType(typeof(MeasurementValidationLatestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MeasurementValidationLatestResponse>> GetLatestAsync(
        string measurementId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(measurementId);
        MeasurementValidationSummary? row = await _sender
            .SendAsync(new GetLatestMeasurementValidationQuery(measurementId.Trim()), cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return NotFound();
        return Ok(
            new MeasurementValidationLatestResponse(
                row.Id.ToString(),
                row.MeasurementId,
                row.ValidationProfileId,
                row.Outcome.ToString(),
                row.Reason,
                row.RuleSetVersion,
                row.EvaluatedAtUtc));
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record ValidateMeasurementRequest(string ValidationProfileId, double? SampleValue);

public sealed record ValidateMeasurementResponse(string ValidationId, string Outcome);

public sealed record MeasurementValidationLatestResponse(
    string Id,
    string MeasurementId,
    string ValidationProfileId,
    string Outcome,
    string? Reason,
    string RuleSetVersion,
    DateTimeOffset EvaluatedAtUtc);
