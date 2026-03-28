using FinancialInteroperability.Domain;
using FinancialInteroperability.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

namespace FinancialInteroperability.Application.Commands.RecordClaimAdjudication;

public sealed class RecordClaimAdjudicationCommandHandler
    : ICommandHandler<RecordClaimAdjudicationCommand, RecordClaimAdjudicationResult>
{
    private readonly IDialysisFinancialClaimRepository _claims;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public RecordClaimAdjudicationCommandHandler(
        IDialysisFinancialClaimRepository claims,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _claims = claims ?? throw new ArgumentNullException(nameof(claims));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<RecordClaimAdjudicationResult> HandleAsync(
        RecordClaimAdjudicationCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        DialysisFinancialClaim? claim = await _claims
            .GetByIdAsync(command.FinancialClaimId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Financial claim was not found.");

        string responseId = command.ExternalClaimResponseId.Trim();
        if (claim.Status == FinancialClaimLifecycleStatus.Adjudicated
            && string.Equals(claim.ExternalClaimResponseId, responseId, StringComparison.Ordinal)) return new RecordClaimAdjudicationResult(false);

        claim.RecordAdjudication(command.CorrelationId, command.ExternalClaimResponseId, command.OutcomeDisplay);

        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Update,
                    "DialysisFinancialClaim",
                    claim.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Claim adjudicated with response {claim.ExternalClaimResponseId}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new RecordClaimAdjudicationResult(true);
    }
}
