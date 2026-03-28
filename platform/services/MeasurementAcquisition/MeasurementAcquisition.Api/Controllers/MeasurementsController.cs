using System.Security.Claims;

using Asp.Versioning;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using MeasurementAcquisition.Application.Commands.IngestMeasurement;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeasurementAcquisition.Api.Controllers;

/// <summary>
/// Raw measurement ingest (v1).
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/measurements")]
public sealed class MeasurementsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    /// <summary>
    /// Creates the controller.
    /// </summary>
    public MeasurementsController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    /// <summary>
    /// Accepts a raw measurement JSON payload and records accept/reject in one transaction (<c>POST /api/v1/measurements</c>).
    /// </summary>
    [HttpPost]
    [HttpPost("ingest")]
    [Authorize(Policy = PlatformAuthorizationPolicies.MeasurementsWrite)]
    [ProducesResponseType(typeof(IngestMeasurementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IngestMeasurementResponse>> IngestAsync(
        [FromBody] IngestMeasurementRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = new IngestMeasurementPayloadCommand(
            correlationId,
            request.DeviceIdentifier,
            request.Channel,
            request.MeasurementType,
            request.SchemaVersion,
            request.RawPayloadJson,
            principalId);
        IngestMeasurementPayloadResult result = await _sender.SendAsync(command, cancellationToken)
            .ConfigureAwait(false);
        var body = new IngestMeasurementResponse(
            result.MeasurementId,
            result.DeviceId,
            result.Status,
            result.PayloadHash,
            result.RejectionReason);
        return StatusCode(StatusCodes.Status201Created, body);
    }
}
