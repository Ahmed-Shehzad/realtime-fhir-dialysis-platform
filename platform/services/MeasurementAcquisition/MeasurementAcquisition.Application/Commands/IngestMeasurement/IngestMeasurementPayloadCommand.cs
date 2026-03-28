using Intercessor.Abstractions;

namespace MeasurementAcquisition.Application.Commands.IngestMeasurement;

/// <summary>
/// Receives a raw measurement payload and records acceptance or rejection (same transaction).
/// </summary>
public sealed record IngestMeasurementPayloadCommand(
    Ulid CorrelationId,
    string DeviceIdentifier,
    string Channel,
    string MeasurementType,
    string SchemaVersion,
    string RawPayloadJson,
    string? AuthenticatedUserId) : ICommand<IngestMeasurementPayloadResult>;
