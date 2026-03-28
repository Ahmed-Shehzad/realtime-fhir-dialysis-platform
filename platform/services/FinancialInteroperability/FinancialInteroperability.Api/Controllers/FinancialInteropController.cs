using System.Security.Claims;
using System.Text.Json.Serialization;

using Asp.Versioning;

using FinancialInteroperability.Application.Commands.AttachExplanationOfBenefit;
using FinancialInteroperability.Application.Commands.RecordClaimAdjudication;
using FinancialInteroperability.Application.Commands.RecordCoverageEligibility;
using FinancialInteroperability.Application.Commands.RecordPatientCoverage;
using FinancialInteroperability.Application.Commands.SubmitDialysisFinancialClaim;
using FinancialInteroperability.Application.Queries.GetFinancialSessionTimeline;
using FinancialInteroperability.Application.Queries.ListPatientCoverageRegistrations;
using FinancialInteroperability.Domain;
using FinancialInteroperability.Domain.Abstractions;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialInteroperability.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/financial")]
public sealed class FinancialInteropController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public FinancialInteropController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost("coverage-registrations")]
    [Authorize(Policy = PlatformAuthorizationPolicies.FinancialWrite)]
    [ProducesResponseType(typeof(RecordCoverageRegistrationResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<RecordCoverageRegistrationResponse>> RegisterCoverageAsync(
        [FromBody] RecordCoverageRegistrationRequest? body,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(body);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        RecordPatientCoverageResult result = await _sender
            .SendAsync(
                new RecordPatientCoverageCommand(
                    correlationId,
                    body.PatientId,
                    body.MemberIdentifier,
                    body.PayorDisplayName,
                    body.PlanDisplayName,
                    body.PeriodStart,
                    body.PeriodEnd,
                    body.FhirCoverageResourceId,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return StatusCode(
            StatusCodes.Status201Created,
            new RecordCoverageRegistrationResponse(result.CoverageRegistrationId.ToString()));
    }

    [HttpPost("coverage-eligibility")]
    [Authorize(Policy = PlatformAuthorizationPolicies.FinancialWrite)]
    [ProducesResponseType(typeof(RecordEligibilityResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<RecordEligibilityResponse>> RecordEligibilityAsync(
        [FromBody] RecordEligibilityRequest? body,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(body);
        if (!Ulid.TryParse(body.PatientCoverageRegistrationId, out Ulid registrationId))
            return BadRequest("PatientCoverageRegistrationId must be a valid ULID.");
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        RecordCoverageEligibilityResult result = await _sender
            .SendAsync(
                new RecordCoverageEligibilityCommand(
                    correlationId,
                    registrationId,
                    body.PatientId,
                    body.OutcomeCode,
                    body.Notes,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return StatusCode(StatusCodes.Status201Created, new RecordEligibilityResponse(result.EligibilityInquiryId.ToString()));
    }

    [HttpPost("claims")]
    [Authorize(Policy = PlatformAuthorizationPolicies.FinancialWrite)]
    [ProducesResponseType(typeof(SubmitClaimResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<SubmitClaimResponse>> SubmitClaimAsync(
        [FromBody] SubmitClaimRequest? body,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(body);
        if (!Ulid.TryParse(body.PatientCoverageRegistrationId, out Ulid registrationId))
            return BadRequest("PatientCoverageRegistrationId must be a valid ULID.");
        FinancialClaimUse use = ParseClaimUse(body.ClaimUse);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        SubmitDialysisFinancialClaimResult result = await _sender
            .SendAsync(
                new SubmitDialysisFinancialClaimCommand(
                    correlationId,
                    body.TreatmentSessionId,
                    body.PatientId,
                    registrationId,
                    body.FhirEncounterReference,
                    use,
                    body.ExternalClaimId,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return StatusCode(StatusCodes.Status201Created, new SubmitClaimResponse(result.FinancialClaimId.ToString()));
    }

    [HttpPost("claims/{claimId}/adjudication")]
    [Authorize(Policy = PlatformAuthorizationPolicies.FinancialWrite)]
    [ProducesResponseType(typeof(RecordAdjudicationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RecordAdjudicationResponse>> AdjudicateClaimAsync(
        string claimId,
        [FromBody] RecordAdjudicationRequest? body,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(claimId);
        ArgumentNullException.ThrowIfNull(body);
        if (!Ulid.TryParse(claimId.Trim(), out Ulid id))
            return BadRequest("claimId must be a valid ULID.");
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        RecordClaimAdjudicationResult result = await _sender
            .SendAsync(
                new RecordClaimAdjudicationCommand(
                    correlationId,
                    id,
                    body.ExternalClaimResponseId,
                    body.OutcomeDisplay,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return Ok(new RecordAdjudicationResponse(result.Updated));
    }

    [HttpPost("explanation-of-benefit")]
    [Authorize(Policy = PlatformAuthorizationPolicies.FinancialWrite)]
    [ProducesResponseType(typeof(AttachEobResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(AttachEobResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AttachEobResponse>> AttachEobAsync(
        [FromBody] AttachEobRequest? body,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(body);
        if (!Ulid.TryParse(body.DialysisFinancialClaimId, out Ulid claimUlid))
            return BadRequest("DialysisFinancialClaimId must be a valid ULID.");
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        AttachExplanationOfBenefitResult result = await _sender
            .SendAsync(
                new AttachExplanationOfBenefitCommand(
                    correlationId,
                    claimUlid,
                    body.TreatmentSessionId,
                    body.FhirExplanationOfBenefitReference,
                    body.PatientResponsibilityAmount,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        if (result.Created)
            return StatusCode(
                StatusCodes.Status201Created,
                new AttachEobResponse(result.ExplanationOfBenefitRecordId.ToString(), result.Created));

        return Ok(new AttachEobResponse(result.ExplanationOfBenefitRecordId.ToString(), result.Created));
    }

    [HttpGet("patients/{patientId}/coverage-registrations")]
    [Authorize(Policy = PlatformAuthorizationPolicies.FinancialRead)]
    [ProducesResponseType(typeof(IReadOnlyList<PatientCoverageRegistrationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PatientCoverageRegistrationDto>>> ListCoveragesAsync(
        string patientId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(patientId);
        IReadOnlyList<PatientCoverageRegistrationSummary> rows = await _sender
            .SendAsync(new ListPatientCoverageRegistrationsQuery(patientId.Trim()), cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<PatientCoverageRegistrationDto> dto = rows
            .Select(
                r => new PatientCoverageRegistrationDto(
                    r.Id.ToString(),
                    r.PatientId,
                    r.MemberIdentifier,
                    r.PayorDisplayName,
                    r.PlanDisplayName,
                    r.PeriodStart,
                    r.PeriodEnd,
                    r.FhirCoverageResourceId,
                    r.CreatedAtUtc))
            .ToList();
        return Ok(dto);
    }

    [HttpGet("sessions/{sessionId}/timeline")]
    [Authorize(Policy = PlatformAuthorizationPolicies.FinancialRead)]
    [ProducesResponseType(typeof(FinancialSessionTimelineDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<FinancialSessionTimelineDto>> GetTimelineAsync(
        string sessionId,
        [FromQuery] string? patientId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        FinancialSessionTimelineReadModel model = await _sender
            .SendAsync(new GetFinancialSessionTimelineQuery(sessionId.Trim(), patientId), cancellationToken)
            .ConfigureAwait(false);
        return Ok(
            new FinancialSessionTimelineDto(
                model.TreatmentSessionId,
                model.ResolvedPatientId,
                model.Coverages.Select(
                        c => new PatientCoverageRegistrationDto(
                            c.Id.ToString(),
                            c.PatientId,
                            c.MemberIdentifier,
                            c.PayorDisplayName,
                            c.PlanDisplayName,
                            c.PeriodStart,
                            c.PeriodEnd,
                            c.FhirCoverageResourceId,
                            c.CreatedAtUtc))
                    .ToList(),
                model.EligibilityInquiries.Select(
                        e => new EligibilityInquiryDto(
                            e.Id.ToString(),
                            e.PatientCoverageRegistrationId.ToString(),
                            e.PatientId,
                            e.Status.ToString(),
                            e.OutcomeCode,
                            e.Notes,
                            e.CreatedAtUtc))
                    .ToList(),
                model.Claims.Select(
                        c => new FinancialClaimDto(
                            c.Id.ToString(),
                            c.TreatmentSessionId,
                            c.PatientId,
                            c.PatientCoverageRegistrationId.ToString(),
                            c.FhirEncounterReference,
                            c.ClaimUse.ToString(),
                            c.Status.ToString(),
                            c.ExternalClaimId,
                            c.ExternalClaimResponseId,
                            c.OutcomeDisplay,
                            c.CreatedAtUtc,
                            c.UpdatedAtUtc))
                    .ToList(),
                model.ExplanationOfBenefits.Select(
                        e => new ExplanationOfBenefitDto(
                            e.Id.ToString(),
                            e.DialysisFinancialClaimId.ToString(),
                            e.TreatmentSessionId,
                            e.FhirExplanationOfBenefitReference,
                            e.PatientResponsibilityAmount,
                            e.LinkedAtUtc,
                            e.CreatedAtUtc))
                    .ToList()));
    }

    private static FinancialClaimUse ParseClaimUse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return FinancialClaimUse.Normal;
        if (Enum.TryParse(value.Trim(), ignoreCase: true, out FinancialClaimUse parsed))
            return parsed;
        throw new ArgumentException("ClaimUse must be Normal or Preauthorization.", nameof(value));
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record RecordCoverageRegistrationRequest(
    string PatientId,
    string MemberIdentifier,
    string PayorDisplayName,
    string PlanDisplayName,
    [property: JsonRequired]
    DateOnly PeriodStart,
    DateOnly? PeriodEnd,
    string? FhirCoverageResourceId);

public sealed record RecordCoverageRegistrationResponse(string RegistrationId);

public sealed record RecordEligibilityRequest(
    string PatientCoverageRegistrationId,
    string PatientId,
    string OutcomeCode,
    string? Notes);

public sealed record RecordEligibilityResponse(string InquiryId);

public sealed record SubmitClaimRequest(
    string TreatmentSessionId,
    string PatientId,
    string PatientCoverageRegistrationId,
    string? FhirEncounterReference,
    string ClaimUse,
    string? ExternalClaimId);

public sealed record SubmitClaimResponse(string FinancialClaimId);

public sealed record RecordAdjudicationRequest(string ExternalClaimResponseId, string? OutcomeDisplay);

public sealed record RecordAdjudicationResponse(bool Updated);

public sealed record AttachEobRequest(
    string DialysisFinancialClaimId,
    string TreatmentSessionId,
    string FhirExplanationOfBenefitReference,
    decimal? PatientResponsibilityAmount);

public sealed record AttachEobResponse(string ExplanationOfBenefitRecordId, bool Created);

public sealed record PatientCoverageRegistrationDto(
    string Id,
    string PatientId,
    string MemberIdentifier,
    string PayorDisplayName,
    string PlanDisplayName,
    DateOnly PeriodStart,
    DateOnly? PeriodEnd,
    string? FhirCoverageResourceId,
    DateTime CreatedAtUtc);

public sealed record EligibilityInquiryDto(
    string Id,
    string PatientCoverageRegistrationId,
    string PatientId,
    string Status,
    string OutcomeCode,
    string? Notes,
    DateTime CreatedAtUtc);

public sealed record FinancialClaimDto(
    string Id,
    string TreatmentSessionId,
    string PatientId,
    string PatientCoverageRegistrationId,
    string? FhirEncounterReference,
    string ClaimUse,
    string Status,
    string? ExternalClaimId,
    string? ExternalClaimResponseId,
    string? OutcomeDisplay,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record ExplanationOfBenefitDto(
    string Id,
    string DialysisFinancialClaimId,
    string TreatmentSessionId,
    string FhirExplanationOfBenefitReference,
    decimal? PatientResponsibilityAmount,
    DateTimeOffset LinkedAtUtc,
    DateTime CreatedAtUtc);

public sealed record FinancialSessionTimelineDto(
    string TreatmentSessionId,
    string? ResolvedPatientId,
    IReadOnlyList<PatientCoverageRegistrationDto> Coverages,
    IReadOnlyList<EligibilityInquiryDto> EligibilityInquiries,
    IReadOnlyList<FinancialClaimDto> Claims,
    IReadOnlyList<ExplanationOfBenefitDto> ExplanationOfBenefits);
