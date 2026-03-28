using System.Security.Claims;

using Asp.Versioning;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using DeviceRegistry.Application.Commands.RegisterDevice;

using DeviceRegistry.Domain.Abstractions;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeviceRegistry.Api.Controllers;

/// <summary>
/// Device onboarding and trust API (v1).
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/devices")]
public sealed class DevicesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITrustLookup _trustLookup;
    private readonly ICorrelationIdAccessor _correlation;

    /// <summary>
    /// Creates the controller.
    /// </summary>
    public DevicesController(ISender sender, ITrustLookup trustLookup, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _trustLookup = trustLookup ?? throw new ArgumentNullException(nameof(trustLookup));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    /// <summary>
    /// Registers a new device.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PlatformAuthorizationPolicies.DevicesWrite)]
    [ProducesResponseType(typeof(RegisterDeviceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterDeviceResponse>> RegisterAsync(
        [FromBody] RegisterDeviceRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        string? principalId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = new RegisterDeviceCommand(
            _correlation.GetOrCreate(),
            request.DeviceIdentifier,
            request.Manufacturer,
            request.InitialTrustState,
            principalId);
        try
        {
            RegisterDeviceResult result = await _sender.SendAsync(command, cancellationToken)
                .ConfigureAwait(false);
            var body = new RegisterDeviceResponse(result.AggregateId, result.DeviceId, result.TrustState);
            return CreatedAtAction(
                nameof(GetTrustAsync),
                new { version = "1", deviceId = result.DeviceId },
                body);
        }
        catch (InvalidOperationException)
        {
            return Conflict();
        }
    }

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
