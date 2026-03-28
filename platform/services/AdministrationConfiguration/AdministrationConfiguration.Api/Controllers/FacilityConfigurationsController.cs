using System.Security.Claims;

using Asp.Versioning;

using AdministrationConfiguration.Application.Commands.UpsertFacilityConfiguration;
using AdministrationConfiguration.Application.Queries.GetFacilityConfigurationByFacilityId;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdministrationConfiguration.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/administration-configuration/facility-configurations")]
public sealed class FacilityConfigurationsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public FacilityConfigurationsController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPut("{facilityId}")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ConfigurationWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpsertAsync(
        string facilityId,
        [FromBody] UpsertFacilityConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(
                new UpsertFacilityConfigurationCommand(
                    correlationId,
                    facilityId.Trim(),
                    request.ConfigurationJson,
                    request.EffectiveFromUtc,
                    request.EffectiveToUtc,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpGet("{facilityId}")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ConfigurationRead)]
    [ProducesResponseType(typeof(FacilityConfigurationReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FacilityConfigurationReadDto>> GetAsync(
        string facilityId,
        CancellationToken cancellationToken)
    {
        FacilityConfigurationReadDto? row = await _sender
            .SendAsync(new GetFacilityConfigurationByFacilityIdQuery(facilityId.Trim()), cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return NotFound();
        return Ok(row);
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record UpsertFacilityConfigurationRequest(
    string ConfigurationJson,
    DateTimeOffset? EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc);
