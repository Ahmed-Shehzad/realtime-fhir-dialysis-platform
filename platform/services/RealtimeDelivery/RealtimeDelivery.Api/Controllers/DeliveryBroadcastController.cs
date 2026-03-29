using System.Security.Claims;
using System.Text.Json.Serialization;

using Asp.Versioning;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using RealtimeDelivery.Application.Commands.BroadcastAlertFeed;
using RealtimeDelivery.Application.Commands.BroadcastSessionFeed;

namespace RealtimeDelivery.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/delivery/broadcast")]
public sealed class DeliveryBroadcastController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public DeliveryBroadcastController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost("session")]
    [Authorize(Policy = PlatformAuthorizationPolicies.DeliveryWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> BroadcastSessionAsync(
        [FromBody] BroadcastSessionFeedRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(
                new BroadcastSessionFeedCommand(
                    correlationId,
                    request.TreatmentSessionId,
                    request.EventType,
                    request.Summary,
                    request.OccurredAtUtc,
                    principalId,
                    request.VitalsByChannel,
                    request.PatientDisplayLabel,
                    request.SessionStateHint,
                    request.LinkedDeviceIdHint),
                cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("alert")]
    [Authorize(Policy = PlatformAuthorizationPolicies.DeliveryWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> BroadcastAlertAsync(
        [FromBody] BroadcastAlertFeedRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(
                new BroadcastAlertFeedCommand(
                    correlationId,
                    request.EventType,
                    request.TreatmentSessionId,
                    request.AlertId,
                    request.Severity,
                    request.LifecycleState,
                    request.OccurredAtUtc,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record BroadcastSessionFeedRequest(
    string TreatmentSessionId,
    string EventType,
    string Summary,
    [property: JsonRequired] DateTimeOffset OccurredAtUtc,
    Dictionary<string, double>? VitalsByChannel = null,
    string? PatientDisplayLabel = null,
    string? SessionStateHint = null,
    string? LinkedDeviceIdHint = null);

public sealed record BroadcastAlertFeedRequest(
    string EventType,
    string? TreatmentSessionId,
    string AlertId,
    string Severity,
    string LifecycleState,
    [property: JsonRequired] DateTimeOffset OccurredAtUtc);
