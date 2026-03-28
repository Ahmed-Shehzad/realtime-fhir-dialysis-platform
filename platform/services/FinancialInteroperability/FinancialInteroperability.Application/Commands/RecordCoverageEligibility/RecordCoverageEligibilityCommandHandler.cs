using FinancialInteroperability.Domain;
using FinancialInteroperability.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

namespace FinancialInteroperability.Application.Commands.RecordCoverageEligibility;

public sealed class RecordCoverageEligibilityCommandHandler
    : ICommandHandler<RecordCoverageEligibilityCommand, RecordCoverageEligibilityResult>
{
    private readonly IPatientCoverageRegistrationRepository _coverage;
    private readonly ICoverageEligibilityInquiryRepository _inquiries;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public RecordCoverageEligibilityCommandHandler(
        IPatientCoverageRegistrationRepository coverage,
        ICoverageEligibilityInquiryRepository inquiries,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _coverage = coverage ?? throw new ArgumentNullException(nameof(coverage));
        _inquiries = inquiries ?? throw new ArgumentNullException(nameof(inquiries));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<RecordCoverageEligibilityResult> HandleAsync(
        RecordCoverageEligibilityCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        PatientCoverageRegistration? reg = await _coverage
            .GetByIdAsync(command.PatientCoverageRegistrationId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Coverage registration was not found.");
        if (!string.Equals(reg.PatientId, command.PatientId.Trim(), StringComparison.Ordinal))
            throw new InvalidOperationException("Patient id does not match the coverage registration.");

        CoverageEligibilityInquiry inquiry = CoverageEligibilityInquiry.Complete(
            command.CorrelationId,
            reg.Id,
            command.PatientId,
            command.OutcomeCode,
            command.Notes,
            _tenant.TenantId);
        await _inquiries.AddAsync(inquiry, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Create,
                    "CoverageEligibilityInquiry",
                    inquiry.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Eligibility outcome {inquiry.OutcomeCode} for patient {inquiry.PatientId}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new RecordCoverageEligibilityResult(inquiry.Id);
    }
}
