using System.Security.Claims;

using Asp.Versioning;

using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Mvc;

using TreatmentSession.Application.Commands.AssignPatient;
using TreatmentSession.Application.Commands.CompleteSession;
using TreatmentSession.Application.Commands.CreateSession;
using TreatmentSession.Application.Commands.LinkDevice;
using TreatmentSession.Application.Commands.MarkMeasurementUnresolved;
using TreatmentSession.Application.Commands.ResolveMeasurementContext;
using TreatmentSession.Application.Commands.StartSession;

namespace TreatmentSession.Api.Controllers;

/// <summary>Dialysis treatment session lifecycle API (v1).</summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/sessions")]
public sealed class SessionsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public SessionsController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    /// <summary>Creates a session in Created state.</summary>
    [HttpPost]
    // [Authorize(Policy = PlatformAuthorizationPolicies.SessionsWrite)]
    [ProducesResponseType(typeof(CreateSessionResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateSessionResponse>> CreateAsync(CancellationToken cancellationToken)
    {
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        var command = new CreateDialysisSessionCommand(correlationId, principalId);
        CreateDialysisSessionResult result = await _sender.SendAsync(command, cancellationToken).ConfigureAwait(false);
        return StatusCode(StatusCodes.Status201Created, new CreateSessionResponse(result.SessionId.ToString()));
    }

    /// <summary>Assigns patient (MRN) to a created session.</summary>
    [HttpPost("{sessionId}/patient")]
    // [Authorize(Policy = PlatformAuthorizationPolicies.SessionsWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignPatientAsync(
        string sessionId,
        [FromBody] AssignPatientRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseSessionId(sessionId, out Ulid sid))
            return BadRequest();
        ArgumentNullException.ThrowIfNull(request);
        return await ExecuteMutationAsync(
            async () =>
            {
                Ulid correlationId = _correlation.GetOrCreate();
                string? principalId = GetPrincipalObjectId();
                _ = await _sender
                    .SendAsync(
                        new AssignPatientToSessionCommand(correlationId, sid, request.MedicalRecordNumber, principalId),
                        cancellationToken)
                    .ConfigureAwait(false);
            }).ConfigureAwait(false);
    }

    /// <summary>Links a device to a created session.</summary>
    [HttpPost("{sessionId}/device")]
    // [Authorize(Policy = PlatformAuthorizationPolicies.SessionsWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LinkDeviceAsync(
        string sessionId,
        [FromBody] LinkDeviceRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseSessionId(sessionId, out Ulid sid))
            return BadRequest();
        ArgumentNullException.ThrowIfNull(request);
        return await ExecuteMutationAsync(
            async () =>
            {
                Ulid correlationId = _correlation.GetOrCreate();
                string? principalId = GetPrincipalObjectId();
                _ = await _sender
                    .SendAsync(
                        new LinkDeviceToSessionCommand(correlationId, sid, request.DeviceIdentifier, principalId),
                        cancellationToken)
                    .ConfigureAwait(false);
            }).ConfigureAwait(false);
    }

    /// <summary>Starts the session (Created → Active).</summary>
    [HttpPost("{sessionId}/start")]
    // [Authorize(Policy = PlatformAuthorizationPolicies.SessionsWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> StartAsync(string sessionId, CancellationToken cancellationToken)
    {
        if (!TryParseSessionId(sessionId, out Ulid sid))
            return BadRequest();
        return await ExecuteMutationAsync(
            async () =>
            {
                Ulid correlationId = _correlation.GetOrCreate();
                string? principalId = GetPrincipalObjectId();
                _ = await _sender
                    .SendAsync(new StartDialysisSessionCommand(correlationId, sid, principalId), cancellationToken)
                    .ConfigureAwait(false);
            }).ConfigureAwait(false);
    }

    /// <summary>Completes an active session.</summary>
    [HttpPost("{sessionId}/complete")]
    // [Authorize(Policy = PlatformAuthorizationPolicies.SessionsWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CompleteAsync(string sessionId, CancellationToken cancellationToken)
    {
        if (!TryParseSessionId(sessionId, out Ulid sid))
            return BadRequest();
        return await ExecuteMutationAsync(
            async () =>
            {
                Ulid correlationId = _correlation.GetOrCreate();
                string? principalId = GetPrincipalObjectId();
                _ = await _sender
                    .SendAsync(new CompleteDialysisSessionCommand(correlationId, sid, principalId), cancellationToken)
                    .ConfigureAwait(false);
            }).ConfigureAwait(false);
    }

    /// <summary>Records measurement context resolved.</summary>
    [HttpPost("{sessionId}/measurements/{measurementId}/context/resolved")]
    // [Authorize(Policy = PlatformAuthorizationPolicies.SessionsWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResolveContextAsync(
        string sessionId,
        string measurementId,
        CancellationToken cancellationToken)
    {
        if (!TryParseSessionId(sessionId, out Ulid sid))
            return BadRequest();
        ArgumentException.ThrowIfNullOrWhiteSpace(measurementId);
        return await ExecuteMutationAsync(
            async () =>
            {
                Ulid correlationId = _correlation.GetOrCreate();
                string? principalId = GetPrincipalObjectId();
                _ = await _sender
                    .SendAsync(
                        new ResolveMeasurementContextCommand(correlationId, sid, measurementId, principalId),
                        cancellationToken)
                    .ConfigureAwait(false);
            }).ConfigureAwait(false);
    }

    /// <summary>Records measurement context unresolved.</summary>
    [HttpPost("{sessionId}/measurements/{measurementId}/context/unresolved")]
    // [Authorize(Policy = PlatformAuthorizationPolicies.SessionsWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkUnresolvedAsync(
        string sessionId,
        string measurementId,
        [FromBody] MarkUnresolvedRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseSessionId(sessionId, out Ulid sid))
            return BadRequest();
        ArgumentException.ThrowIfNullOrWhiteSpace(measurementId);
        ArgumentNullException.ThrowIfNull(request);
        return await ExecuteMutationAsync(
            async () =>
            {
                Ulid correlationId = _correlation.GetOrCreate();
                string? principalId = GetPrincipalObjectId();
                _ = await _sender
                    .SendAsync(
                        new MarkMeasurementContextUnresolvedCommand(
                            correlationId,
                            sid,
                            measurementId,
                            request.Reason,
                            principalId),
                        cancellationToken)
                    .ConfigureAwait(false);
            }).ConfigureAwait(false);
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

    private static bool TryParseSessionId(string sessionId, out Ulid id)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            id = default;
            return false;
        }

        try
        {
            id = Ulid.Parse(sessionId.Trim());
            return true;
        }
        catch (FormatException)
        {
            id = default;
            return false;
        }
    }

    private static bool IsNotFoundMessage(string message) =>
        message.Contains("was not found", StringComparison.Ordinal);

    private async Task<IActionResult> ExecuteMutationAsync(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (IsNotFoundMessage(ex.Message))
        {
            return NotFound();
        }
        catch (InvalidOperationException)
        {
            return Conflict();
        }
    }
}

/// <summary>Response body for session creation.</summary>
public sealed record CreateSessionResponse(string SessionId);

public sealed record AssignPatientRequest(string MedicalRecordNumber);

public sealed record LinkDeviceRequest(string DeviceIdentifier);

public sealed record MarkUnresolvedRequest(string Reason);
