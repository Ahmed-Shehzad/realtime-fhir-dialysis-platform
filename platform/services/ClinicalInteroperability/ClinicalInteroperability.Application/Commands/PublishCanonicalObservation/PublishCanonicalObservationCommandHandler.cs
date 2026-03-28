using ClinicalInteroperability.Domain;
using ClinicalInteroperability.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

namespace ClinicalInteroperability.Application.Commands.PublishCanonicalObservation;

public sealed class PublishCanonicalObservationCommandHandler
    : ICommandHandler<PublishCanonicalObservationCommand, PublishCanonicalObservationResult>
{
    private readonly ICanonicalObservationPublicationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public PublishCanonicalObservationCommandHandler(
        ICanonicalObservationPublicationRepository repository,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<PublishCanonicalObservationResult> HandleAsync(
        PublishCanonicalObservationCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        CanonicalObservationPublication publication = CanonicalObservationPublication.StartPublication(
            command.CorrelationId,
            command.MeasurementId,
            command.FhirProfileUrl,
            _tenant.TenantId);
        await _repository.AddAsync(publication, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Execute,
                    "CanonicalObservationPublication",
                    publication.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Canonical observation publication state: {publication.State}; attempts={publication.AttemptCount}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new PublishCanonicalObservationResult(
            publication.Id,
            publication.State,
            publication.ObservationId,
            publication.FhirResourceReference);
    }
}
