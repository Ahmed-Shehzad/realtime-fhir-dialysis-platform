using BuildingBlocks.ValueObjects;

namespace MeasurementAcquisition.Domain;

/// <summary>
/// Domain input slice for <see cref="RawMeasurementEnvelope.Ingest"/>.
/// </summary>
public sealed record RawIngestInput(
    DeviceId DeviceId,
    string Channel,
    string MeasurementType,
    string SchemaVersion,
    string RawPayloadJson);
