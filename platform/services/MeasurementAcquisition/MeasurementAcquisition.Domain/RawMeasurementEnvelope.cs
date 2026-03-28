using BuildingBlocks;

using BuildingBlocks.ValueObjects;

using MeasurementAcquisition.Domain.Events;
using MeasurementAcquisition.Domain.IntegrationEvents;

using RealtimePlatform.IntegrationEventCatalog;

namespace MeasurementAcquisition.Domain;

/// <summary>
/// Aggregate for a single ingested raw measurement payload (MVP envelope).
/// </summary>
public sealed class RawMeasurementEnvelope : AggregateRoot
{
    /// <summary>Maximum length for <see cref="Channel"/>.</summary>
    public const int MaxChannelLength = 128;

    /// <summary>Maximum length for <see cref="MeasurementType"/>.</summary>
    public const int MaxMeasurementTypeLength = 128;

    /// <summary>Maximum length for <see cref="SchemaVersion"/>.</summary>
    public const int MaxSchemaVersionLength = 64;

    private RawMeasurementEnvelope()
    {
        DeviceId = new DeviceId("__uninitialized__");
    }

    public DeviceId DeviceId { get; private set; }

    public string Channel { get; private set; } = string.Empty;

    public string MeasurementType { get; private set; } = string.Empty;

    public string SchemaVersion { get; private set; } = string.Empty;

    public string PayloadHash { get; private set; } = string.Empty;

    public string RawPayloadJson { get; private set; } = string.Empty;

    public AcquisitionStatus Status { get; private set; }

    public string? RejectionReason { get; private set; }

    /// <summary>
    /// Creates an envelope, records received, then accepts or rejects in the same unit of work.
    /// </summary>
    /// <param name="correlationId">Correlation for tracing and integration events.</param>
    /// <param name="input">Channel, type, schema, payload.</param>
    /// <param name="payloadIsValidJson">When false, envelope is rejected with <paramref name="invalidReason"/>.</param>
    /// <param name="invalidReason">Validation message when <paramref name="payloadIsValidJson"/> is false.</param>
    /// <param name="tenantId">Resolved tenant for integration envelope metadata; may be null for tests or non-HTTP callers.</param>
    public static RawMeasurementEnvelope Ingest(
        Ulid correlationId,
        RawIngestInput input,
        bool payloadIsValidJson,
        string? invalidReason,
        string? tenantId)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.Channel);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.MeasurementType);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.SchemaVersion);
        ArgumentNullException.ThrowIfNull(input.RawPayloadJson);

        string channelTrimmed = input.Channel.Trim();
        string typeTrimmed = input.MeasurementType.Trim();
        string schemaTrimmed = input.SchemaVersion.Trim();
        if (channelTrimmed.Length > MaxChannelLength
            || typeTrimmed.Length > MaxMeasurementTypeLength
            || schemaTrimmed.Length > MaxSchemaVersionLength)
            throw new ArgumentException("Channel, measurement type, or schema version exceeds maximum length.");

        DeviceId deviceId = input.DeviceId;
        string rawPayloadJson = input.RawPayloadJson;
        string hash = Sha256PayloadFingerprint.ComputeHex(rawPayloadJson);

        var envelope = new RawMeasurementEnvelope
        {
            DeviceId = deviceId,
            Channel = channelTrimmed,
            MeasurementType = typeTrimmed,
            SchemaVersion = schemaTrimmed,
            PayloadHash = hash,
            RawPayloadJson = rawPayloadJson
        };

        Ulid measurementId = envelope.Id;

        envelope.ApplyEvent(new MeasurementReceivedDomainEvent(measurementId, deviceId));
        envelope.ApplyEvent(
            new MeasurementReceivedIntegrationEvent(
                correlationId,
                measurementId.ToString(),
                deviceId.Value,
                envelope.Channel,
                envelope.MeasurementType)
            {
                RoutingDeviceId = deviceId.Value,
                TenantId = tenantId
            });

        if (payloadIsValidJson)
        {
            envelope.Status = AcquisitionStatus.Accepted;
            envelope.ApplyEvent(new MeasurementAcceptedDomainEvent(measurementId));
            envelope.ApplyEvent(
                new MeasurementAcceptedIntegrationEvent(
                    correlationId,
                    measurementId.ToString(),
                    deviceId.Value,
                    envelope.SchemaVersion,
                    hash)
                {
                    RoutingDeviceId = deviceId.Value
                });
        }
        else
        {
            envelope.Status = AcquisitionStatus.Rejected;
            string reason = string.IsNullOrWhiteSpace(invalidReason) ? "Invalid JSON payload." : invalidReason.Trim();
            envelope.RejectionReason = reason;
            envelope.ApplyEvent(new MeasurementRejectedDomainEvent(measurementId, reason));
            envelope.ApplyEvent(
                new MeasurementRejectedIntegrationEvent(
                    correlationId,
                    measurementId.ToString(),
                    deviceId.Value,
                    reason)
                {
                    RoutingDeviceId = deviceId.Value,
                    TenantId = tenantId
                });
        }

        return envelope;
    }
}
