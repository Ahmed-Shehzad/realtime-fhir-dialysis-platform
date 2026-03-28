using Intercessor.Abstractions;

namespace FinancialInteroperability.Application.Commands.RecordCoverageEligibility;

public sealed record RecordCoverageEligibilityCommand(
    Ulid CorrelationId,
    Ulid PatientCoverageRegistrationId,
    string PatientId,
    string OutcomeCode,
    string? Notes,
    string? AuthenticatedUserId = null) : ICommand<RecordCoverageEligibilityResult>;
