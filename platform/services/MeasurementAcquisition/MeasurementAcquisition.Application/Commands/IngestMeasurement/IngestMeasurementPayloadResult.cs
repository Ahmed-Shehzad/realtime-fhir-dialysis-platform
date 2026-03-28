using MeasurementAcquisition.Domain;

namespace MeasurementAcquisition.Application.Commands.IngestMeasurement;

/// <summary>
/// Outcome of ingest persisted to the acquisition store.
/// </summary>
public sealed record IngestMeasurementPayloadResult(
    string MeasurementId,
    string DeviceId,
    AcquisitionStatus Status,
    string PayloadHash,
    string? RejectionReason);
