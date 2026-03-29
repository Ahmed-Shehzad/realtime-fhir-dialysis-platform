using Asp.Versioning;

using BuildingBlocks.Authorization;

using DeviceRegistry.Domain.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeviceRegistry.Api.Controllers;

/// <summary>
/// Device trust read API (v1).
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/devices")]
public sealed class DeviceTrustController : ControllerBase
{
    private readonly ITrustLookup _trustLookup;

    /// <summary>
    /// Creates the controller.
    /// </summary>
    public DeviceTrustController(ITrustLookup trustLookup) =>
        _trustLookup = trustLookup ?? throw new ArgumentNullException(nameof(trustLookup));

    /// <summary>
    /// Returns whether the device exists and is trusted (active).
    /// </summary>
    [HttpGet("{deviceId}/trust")]
    [Authorize(Policy = PlatformAuthorizationPolicies.DevicesRead)]
    [ProducesResponseType(typeof(DeviceTrustResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DeviceTrustResponse>> GetTrustAsync(
        string deviceId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        bool trusted = await _trustLookup
            .IsDeviceTrustedAsync(new BuildingBlocks.ValueObjects.DeviceId(deviceId), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new DeviceTrustResponse(deviceId, trusted));
    }
}
