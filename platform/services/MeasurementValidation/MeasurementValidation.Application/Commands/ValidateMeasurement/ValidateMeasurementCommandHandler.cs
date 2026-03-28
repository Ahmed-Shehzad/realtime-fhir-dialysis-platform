using MeasurementValidation.Domain;
using MeasurementValidation.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

namespace MeasurementValidation.Application.Commands.ValidateMeasurement;

public sealed class ValidateMeasurementCommandHandler : ICommandHandler<ValidateMeasurementCommand, ValidateMeasurementResult>
{
    private readonly IMeasurementValidationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public ValidateMeasurementCommandHandler(
        IMeasurementValidationRepository repository,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<ValidateMeasurementResult> HandleAsync(
        ValidateMeasurementCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidatedMeasurement validation = ValidatedMeasurement.Run(
            command.CorrelationId,
            command.MeasurementId,
            command.ValidationProfileId,
            command.SampleValue,
            _tenant.TenantId);
        await _repository.AddAsync(validation, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Execute,
                    "ValidatedMeasurement",
                    validation.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Measurement validation outcome: {validation.Outcome}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new ValidateMeasurementResult(validation.Id, validation.Outcome);
    }
}
