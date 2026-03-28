using MeasurementAcquisition.Domain;

namespace MeasurementAcquisition.Api.Controllers;

/// <summary>HTTP request body for raw measurement ingest.</summary>
public sealed record IngestMeasurementRequest(
    string DeviceIdentifier,
    string Channel,
    string MeasurementType,
    string SchemaVersion,
    string RawPayloadJson);

/// <summary>HTTP response after ingest is persisted.</summary>
public sealed record IngestMeasurementResponse(
    string MeasurementId,
    string DeviceId,
    AcquisitionStatus Status,
    string PayloadHash,
    string? RejectionReason);
