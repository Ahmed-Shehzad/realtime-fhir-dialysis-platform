using FinancialInteroperability.Domain;
using FinancialInteroperability.Domain.Abstractions;

using Intercessor.Abstractions;

namespace FinancialInteroperability.Application.Queries.GetFinancialSessionTimeline;

public sealed class GetFinancialSessionTimelineQueryHandler
    : IQueryHandler<GetFinancialSessionTimelineQuery, FinancialSessionTimelineReadModel>
{
    private readonly IPatientCoverageRegistrationRepository _coverages;
    private readonly ICoverageEligibilityInquiryRepository _eligibility;
    private readonly IDialysisFinancialClaimRepository _claims;
    private readonly IExplanationOfBenefitRecordRepository _eob;

    public GetFinancialSessionTimelineQueryHandler(
        IPatientCoverageRegistrationRepository coverages,
        ICoverageEligibilityInquiryRepository eligibility,
        IDialysisFinancialClaimRepository claims,
        IExplanationOfBenefitRecordRepository eob)
    {
        _coverages = coverages ?? throw new ArgumentNullException(nameof(coverages));
        _eligibility = eligibility ?? throw new ArgumentNullException(nameof(eligibility));
        _claims = claims ?? throw new ArgumentNullException(nameof(claims));
        _eob = eob ?? throw new ArgumentNullException(nameof(eob));
    }

    public async Task<FinancialSessionTimelineReadModel> HandleAsync(
        GetFinancialSessionTimelineQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        string sessionId = query.TreatmentSessionId.Trim();
        IReadOnlyList<DialysisFinancialClaim> claimRows = await _claims
            .ListByTreatmentSessionIdAsync(sessionId, cancellationToken)
            .ConfigureAwait(false);

        string? resolvedPatient = query.PatientId?.Trim();
        if (string.IsNullOrEmpty(resolvedPatient) && claimRows.Count > 0)
            resolvedPatient = claimRows[0].PatientId;

        IReadOnlyList<PatientCoverageRegistrationSummary> coverageSummaries = Array.Empty<PatientCoverageRegistrationSummary>();
        IReadOnlyList<CoverageEligibilityInquirySummary> eligibilitySummaries = Array.Empty<CoverageEligibilityInquirySummary>();
        if (!string.IsNullOrEmpty(resolvedPatient))
        {
            IReadOnlyList<PatientCoverageRegistration> cov = await _coverages
                .ListByPatientIdAsync(resolvedPatient, cancellationToken)
                .ConfigureAwait(false);
            coverageSummaries = cov
                .Select(
                    r => new PatientCoverageRegistrationSummary(
                        r.Id,
                        r.PatientId,
                        r.MemberIdentifier,
                        r.PayorDisplayName,
                        r.PlanDisplayName,
                        r.PeriodStart,
                        r.PeriodEnd,
                        r.FhirCoverageResourceId,
                        r.CreatedAtUtc))
                .ToList();

            IReadOnlyList<CoverageEligibilityInquiry> el = await _eligibility
                .ListByPatientIdAsync(resolvedPatient, cancellationToken)
                .ConfigureAwait(false);
            eligibilitySummaries = el
                .Select(
                    i => new CoverageEligibilityInquirySummary(
                        i.Id,
                        i.PatientCoverageRegistrationId,
                        i.PatientId,
                        i.Status,
                        i.OutcomeCode,
                        i.Notes,
                        i.CreatedAtUtc))
                .ToList();
        }

        IReadOnlyList<DialysisFinancialClaimSummary> claimSummaries = claimRows
            .Select(
                c => new DialysisFinancialClaimSummary(
                    c.Id,
                    c.TreatmentSessionId,
                    c.PatientId,
                    c.PatientCoverageRegistrationId,
                    c.FhirEncounterReference,
                    c.ClaimUse,
                    c.Status,
                    c.ExternalClaimId,
                    c.ExternalClaimResponseId,
                    c.OutcomeDisplay,
                    c.CreatedAtUtc,
                    c.UpdatedAtUtc))
            .ToList();

        var eobList = new List<ExplanationOfBenefitSummary>();
        foreach (DialysisFinancialClaim c in claimRows)
        {
            ExplanationOfBenefitRecord? e = await _eob.GetByClaimIdAsync(c.Id, cancellationToken).ConfigureAwait(false);
            if (e is not null)
                eobList.Add(
                    new ExplanationOfBenefitSummary(
                        e.Id,
                        e.DialysisFinancialClaimId,
                        e.TreatmentSessionId,
                        e.FhirExplanationOfBenefitReference,
                        e.PatientResponsibilityAmount,
                        e.LinkedAtUtc,
                        e.CreatedAtUtc));
        }

        return new FinancialSessionTimelineReadModel(
            sessionId,
            resolvedPatient,
            coverageSummaries,
            eligibilitySummaries,
            claimSummaries,
            eobList);
    }
}
