using FinancialInteroperability.Domain;
using FinancialInteroperability.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

namespace FinancialInteroperability.Application.Commands.AttachExplanationOfBenefit;

public sealed class AttachExplanationOfBenefitCommandHandler
    : ICommandHandler<AttachExplanationOfBenefitCommand, AttachExplanationOfBenefitResult>
{
    private readonly IDialysisFinancialClaimRepository _claims;
    private readonly IExplanationOfBenefitRecordRepository _eobRecords;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public AttachExplanationOfBenefitCommandHandler(
        IDialysisFinancialClaimRepository claims,
        IExplanationOfBenefitRecordRepository eobRecords,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _claims = claims ?? throw new ArgumentNullException(nameof(claims));
        _eobRecords = eobRecords ?? throw new ArgumentNullException(nameof(eobRecords));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<AttachExplanationOfBenefitResult> HandleAsync(
        AttachExplanationOfBenefitCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        DialysisFinancialClaim? claim = await _claims
            .GetByIdAsync(command.DialysisFinancialClaimId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Financial claim was not found.");

        if (claim.Status != FinancialClaimLifecycleStatus.Adjudicated)
            throw new InvalidOperationException("Explanation of benefit can only be linked after adjudication.");

        string sessionId = command.TreatmentSessionId.Trim();
        if (!string.Equals(claim.TreatmentSessionId, sessionId, StringComparison.Ordinal))
            throw new InvalidOperationException("Treatment session id does not match the claim.");

        string eobRef = command.FhirExplanationOfBenefitReference.Trim();
        ExplanationOfBenefitRecord? existing = await _eobRecords
            .GetByClaimIdAsync(command.DialysisFinancialClaimId, cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
        {
            if (string.Equals(existing.FhirExplanationOfBenefitReference, eobRef, StringComparison.Ordinal))
                return new AttachExplanationOfBenefitResult(existing.Id, Created: false);
            throw new InvalidOperationException("A different explanation of benefit is already linked to this claim.");
        }

        ExplanationOfBenefitRecord row = ExplanationOfBenefitRecord.Attach(
            command.CorrelationId,
            command.DialysisFinancialClaimId,
            sessionId,
            command.FhirExplanationOfBenefitReference,
            command.PatientResponsibilityAmount,
            _tenant.TenantId);
        await _eobRecords.AddAsync(row, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Create,
                    "ExplanationOfBenefitRecord",
                    row.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"EOB linked to session {row.TreatmentSessionId} for claim {claim.Id}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new AttachExplanationOfBenefitResult(row.Id, Created: true);
    }
}
