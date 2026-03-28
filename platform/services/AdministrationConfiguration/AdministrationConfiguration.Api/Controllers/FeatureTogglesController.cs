using System.Security.Claims;
using System.Text.Json.Serialization;

using Asp.Versioning;

using AdministrationConfiguration.Application.Commands.UpsertFeatureToggle;
using AdministrationConfiguration.Application.Queries.GetFeatureToggleByKey;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdministrationConfiguration.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/administration-configuration/feature-toggles")]
public sealed class FeatureTogglesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public FeatureTogglesController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPut("{featureKey}")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ConfigurationWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpsertAsync(
        string featureKey,
        [FromBody] UpsertFeatureToggleRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(
                new UpsertFeatureToggleCommand(correlationId, featureKey.Trim(), request.IsEnabled, principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpGet("{featureKey}")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ConfigurationRead)]
    [ProducesResponseType(typeof(FeatureToggleReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeatureToggleReadDto>> GetAsync(
        string featureKey,
        CancellationToken cancellationToken)
    {
        FeatureToggleReadDto? row = await _sender
            .SendAsync(new GetFeatureToggleByKeyQuery(featureKey.Trim()), cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return NotFound();
        return Ok(row);
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record UpsertFeatureToggleRequest([property: JsonRequired] bool IsEnabled);
