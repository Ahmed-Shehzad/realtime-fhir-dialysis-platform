using Intercessor.Abstractions;

namespace FinancialInteroperability.Application.Commands.RecordPatientCoverage;

public sealed record RecordPatientCoverageCommand(
    Ulid CorrelationId,
    string PatientId,
    string MemberIdentifier,
    string PayorDisplayName,
    string PlanDisplayName,
    DateOnly PeriodStart,
    DateOnly? PeriodEnd,
    string? FhirCoverageResourceId,
    string? AuthenticatedUserId = null) : ICommand<RecordPatientCoverageResult>;
