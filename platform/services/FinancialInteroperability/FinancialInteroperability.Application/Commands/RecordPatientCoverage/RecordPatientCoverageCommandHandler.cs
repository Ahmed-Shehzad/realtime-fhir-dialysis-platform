using FinancialInteroperability.Domain;
using FinancialInteroperability.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

namespace FinancialInteroperability.Application.Commands.RecordPatientCoverage;

public sealed class RecordPatientCoverageCommandHandler
    : ICommandHandler<RecordPatientCoverageCommand, RecordPatientCoverageResult>
{
    private readonly IPatientCoverageRegistrationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public RecordPatientCoverageCommandHandler(
        IPatientCoverageRegistrationRepository repository,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<RecordPatientCoverageResult> HandleAsync(
        RecordPatientCoverageCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        PatientCoverageRegistration registration = PatientCoverageRegistration.Register(
            command.CorrelationId,
            new PatientCoverageRegistrationRegisterPayload
            {
                PatientId = command.PatientId,
                MemberIdentifier = command.MemberIdentifier,
                PayorDisplayName = command.PayorDisplayName,
                PlanDisplayName = command.PlanDisplayName,
                PeriodStart = command.PeriodStart,
                PeriodEnd = command.PeriodEnd,
                FhirCoverageResourceId = command.FhirCoverageResourceId,
                TenantId = _tenant.TenantId,
            });
        await _repository.AddAsync(registration, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Create,
                    "PatientCoverageRegistration",
                    registration.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Coverage snapshot for patient {registration.PatientId}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new RecordPatientCoverageResult(registration.Id);
    }
}
