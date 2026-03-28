using System.Security.Claims;

using Asp.Versioning;

using AdministrationConfiguration.Application.Commands.ChangeThresholdProfile;
using AdministrationConfiguration.Application.Commands.CreateThresholdProfile;
using AdministrationConfiguration.Application.Queries.GetThresholdProfileById;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdministrationConfiguration.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/administration-configuration/threshold-profiles")]
public sealed class ThresholdProfilesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public ThresholdProfilesController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost]
    [Authorize(Policy = PlatformAuthorizationPolicies.ConfigurationWrite)]
    [ProducesResponseType(typeof(CreateThresholdProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateThresholdProfileResponse>> CreateAsync(
        [FromBody] CreateThresholdProfileRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        Ulid id = await _sender
            .SendAsync(
                new CreateThresholdProfileCommand(
                    correlationId,
                    request.ProfileCode.Trim(),
                    request.PayloadJson,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return Ok(new CreateThresholdProfileResponse(id.ToString()));
    }

    [HttpPut("{profileId}")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ConfigurationWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeAsync(
        string profileId,
        [FromBody] ChangeThresholdProfileRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!Ulid.TryParse(profileId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(
                new ChangeThresholdProfileCommand(correlationId, id, request.PayloadJson, principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpGet("{profileId}")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ConfigurationRead)]
    [ProducesResponseType(typeof(ThresholdProfileReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ThresholdProfileReadDto>> GetByIdAsync(
        string profileId,
        CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(profileId.Trim(), out Ulid id))
            return BadRequest();
        ThresholdProfileReadDto? row =
            await _sender.SendAsync(new GetThresholdProfileByIdQuery(id), cancellationToken).ConfigureAwait(false);
        if (row is null)
            return NotFound();
        return Ok(row);
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record CreateThresholdProfileRequest(string ProfileCode, string PayloadJson);

public sealed record CreateThresholdProfileResponse(string ProfileId);

public sealed record ChangeThresholdProfileRequest(string PayloadJson);
