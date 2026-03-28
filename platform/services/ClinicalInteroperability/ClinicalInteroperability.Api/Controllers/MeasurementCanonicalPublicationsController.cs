using System.Security.Claims;

using Asp.Versioning;

using ClinicalInteroperability.Application.Commands.PublishCanonicalObservation;
using ClinicalInteroperability.Application.Queries.GetLatestCanonicalObservationPublication;
using ClinicalInteroperability.Domain.Abstractions;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicalInteroperability.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/measurements")]
public sealed class MeasurementCanonicalPublicationsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public MeasurementCanonicalPublicationsController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost("{measurementId}/canonical-observation/publications")]
    [Authorize(Policy = PlatformAuthorizationPolicies.InteroperabilityWrite)]
    [ProducesResponseType(typeof(PublishCanonicalObservationResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<PublishCanonicalObservationResponse>> PublishAsync(
        string measurementId,
        [FromBody] PublishCanonicalObservationRequest? request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(measurementId);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        PublishCanonicalObservationResult result = await _sender
            .SendAsync(
                new PublishCanonicalObservationCommand(
                    correlationId,
                    measurementId.Trim(),
                    request?.FhirProfileUrl,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return StatusCode(
            StatusCodes.Status201Created,
            new PublishCanonicalObservationResponse(
                result.PublicationId.ToString(),
                result.State.ToString(),
                result.ObservationId,
                result.FhirResourceReference));
    }

    [HttpGet("{measurementId}/canonical-observation/publications/latest")]
    [Authorize(Policy = PlatformAuthorizationPolicies.InteroperabilityRead)]
    [ProducesResponseType(typeof(CanonicalObservationPublicationLatestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CanonicalObservationPublicationLatestResponse>> GetLatestAsync(
        string measurementId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(measurementId);
        CanonicalObservationPublicationSummary? row = await _sender
            .SendAsync(
                new GetLatestCanonicalObservationPublicationQuery(measurementId.Trim()),
                cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return NotFound();
        return Ok(
            new CanonicalObservationPublicationLatestResponse(
                row.Id.ToString(),
                row.MeasurementId,
                row.ObservationId,
                row.State.ToString(),
                row.FhirResourceReference,
                row.LastFailureReason,
                row.AttemptCount,
                row.LastAttemptAtUtc));
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record PublishCanonicalObservationRequest(string? FhirProfileUrl);

public sealed record PublishCanonicalObservationResponse(
    string PublicationId,
    string State,
    string ObservationId,
    string? FhirResourceReference);

public sealed record CanonicalObservationPublicationLatestResponse(
    string Id,
    string MeasurementId,
    string ObservationId,
    string State,
    string? FhirResourceReference,
    string? LastFailureReason,
    int AttemptCount,
    DateTimeOffset LastAttemptAtUtc);
