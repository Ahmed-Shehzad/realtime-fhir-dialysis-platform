using ClinicalInteroperability.Domain;
using ClinicalInteroperability.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

namespace ClinicalInteroperability.Application.Commands.RetryCanonicalObservationPublication;

public sealed class RetryCanonicalObservationPublicationCommandHandler
    : ICommandHandler<RetryCanonicalObservationPublicationCommand, RetryCanonicalObservationPublicationResult>
{
    private readonly ICanonicalObservationPublicationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public RetryCanonicalObservationPublicationCommandHandler(
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

    public async Task<RetryCanonicalObservationPublicationResult> HandleAsync(
        RetryCanonicalObservationPublicationCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        CanonicalObservationPublication? publication = await _repository
            .GetByIdForUpdateAsync(command.PublicationId, cancellationToken)
            .ConfigureAwait(false);
        if (publication is null)
            return new RetryCanonicalObservationPublicationResult(false, null, null, null);

        publication.RetryPublication(command.CorrelationId, _tenant.TenantId);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Execute,
                    "CanonicalObservationPublication",
                    publication.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Retry canonical observation publication; state={publication.State}; attempts={publication.AttemptCount}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new RetryCanonicalObservationPublicationResult(
            true,
            publication.State,
            publication.ObservationId,
            publication.FhirResourceReference);
    }
}
