using System.Security.Claims;

using Asp.Versioning;

using ClinicalInteroperability.Application.Commands.RetryCanonicalObservationPublication;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicalInteroperability.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/publications")]
public sealed class PublicationRetryController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public PublicationRetryController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost("{publicationId}/retry")]
    [Authorize(Policy = PlatformAuthorizationPolicies.InteroperabilityWrite)]
    [ProducesResponseType(typeof(RetryPublicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RetryPublicationResponse>> RetryAsync(
        string publicationId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(publicationId);
        if (!Ulid.TryParse(publicationId.Trim(), out Ulid pid))
            return BadRequest("publicationId must be a valid ULID.");

        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        try
        {
            RetryCanonicalObservationPublicationResult result = await _sender
                .SendAsync(
                    new RetryCanonicalObservationPublicationCommand(correlationId, pid, principalId),
                    cancellationToken)
                .ConfigureAwait(false);
            if (!result.Found)
                return NotFound();
            return Ok(
                new RetryPublicationResponse(
                    result.State!.Value.ToString(),
                    result.ObservationId!,
                    result.FhirResourceReference));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record RetryPublicationResponse(
    string State,
    string ObservationId,
    string? FhirResourceReference);
