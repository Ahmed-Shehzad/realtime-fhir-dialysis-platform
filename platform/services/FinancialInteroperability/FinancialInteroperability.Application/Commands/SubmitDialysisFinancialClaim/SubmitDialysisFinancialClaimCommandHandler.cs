using FinancialInteroperability.Domain;
using FinancialInteroperability.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

namespace FinancialInteroperability.Application.Commands.SubmitDialysisFinancialClaim;

public sealed class SubmitDialysisFinancialClaimCommandHandler
    : ICommandHandler<SubmitDialysisFinancialClaimCommand, SubmitDialysisFinancialClaimResult>
{
    private readonly IPatientCoverageRegistrationRepository _coverage;
    private readonly IDialysisFinancialClaimRepository _claims;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public SubmitDialysisFinancialClaimCommandHandler(
        IPatientCoverageRegistrationRepository coverage,
        IDialysisFinancialClaimRepository claims,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _coverage = coverage ?? throw new ArgumentNullException(nameof(coverage));
        _claims = claims ?? throw new ArgumentNullException(nameof(claims));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<SubmitDialysisFinancialClaimResult> HandleAsync(
        SubmitDialysisFinancialClaimCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        PatientCoverageRegistration? reg = await _coverage
            .GetByIdAsync(command.PatientCoverageRegistrationId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Coverage registration was not found.");
        if (!string.Equals(reg.PatientId, command.PatientId.Trim(), StringComparison.Ordinal))
            throw new InvalidOperationException("Patient id does not match the coverage registration.");

        DialysisFinancialClaim claim = DialysisFinancialClaim.Submit(
            command.CorrelationId,
            new DialysisFinancialClaimSubmitPayload
            {
                TreatmentSessionId = command.TreatmentSessionId,
                PatientId = command.PatientId,
                PatientCoverageRegistrationId = reg.Id,
                FhirEncounterReference = command.FhirEncounterReference,
                ClaimUse = command.ClaimUse,
                ExternalClaimId = command.ExternalClaimId,
                TenantId = _tenant.TenantId,
            });
        await _claims.AddAsync(claim, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Create,
                    "DialysisFinancialClaim",
                    claim.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Financial claim submitted for session {claim.TreatmentSessionId}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new SubmitDialysisFinancialClaimResult(claim.Id);
    }
}
