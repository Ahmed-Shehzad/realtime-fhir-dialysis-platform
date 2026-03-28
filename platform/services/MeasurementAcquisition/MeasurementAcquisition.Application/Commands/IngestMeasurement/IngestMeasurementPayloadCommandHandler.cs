using System.Text.Json;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;
using BuildingBlocks.ValueObjects;

using MeasurementAcquisition.Domain;
using MeasurementAcquisition.Domain.Abstractions;

using Intercessor.Abstractions;

namespace MeasurementAcquisition.Application.Commands.IngestMeasurement;

/// <summary>
/// Handles <see cref="IngestMeasurementPayloadCommand"/>.
/// </summary>
public sealed class IngestMeasurementPayloadCommandHandler
    : ICommandHandler<IngestMeasurementPayloadCommand, IngestMeasurementPayloadResult>
{
    private readonly IAcquisitionRepository _acquisitions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    /// <summary>
    /// Creates a handler.
    /// </summary>
    public IngestMeasurementPayloadCommandHandler(
        IAcquisitionRepository acquisitions,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _acquisitions = acquisitions ?? throw new ArgumentNullException(nameof(acquisitions));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    /// <inheritdoc />
    public async Task<IngestMeasurementPayloadResult> HandleAsync(
        IngestMeasurementPayloadCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var deviceId = new DeviceId(command.DeviceIdentifier);
        (bool isValid, string? invalidReason) = TryValidateJson(command.RawPayloadJson);
        var ingestInput = new RawIngestInput(
            deviceId,
            command.Channel,
            command.MeasurementType,
            command.SchemaVersion,
            command.RawPayloadJson);
        RawMeasurementEnvelope envelope = RawMeasurementEnvelope.Ingest(
            command.CorrelationId,
            ingestInput,
            isValid,
            invalidReason,
            _tenant.TenantId);

        await _acquisitions.AddAsync(envelope, cancellationToken).ConfigureAwait(false);
        string description =
            envelope.Status == AcquisitionStatus.Accepted
                ? "Raw measurement ingested; payload accepted."
                : $"Raw measurement ingested; payload rejected: {envelope.RejectionReason}";
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Create,
                    "RawMeasurementEnvelope",
                    envelope.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    description,
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new IngestMeasurementPayloadResult(
            envelope.Id.ToString(),
            deviceId.Value,
            envelope.Status,
            envelope.PayloadHash,
            envelope.RejectionReason);
    }

    private static (bool Ok, string? Reason) TryValidateJson(string json)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            _ = doc.RootElement;
            return (true, null);
        }
        catch (JsonException ex)
        {
            return (false, ex.Message);
        }
    }
}
