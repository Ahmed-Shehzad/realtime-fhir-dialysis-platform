using Intercessor.Abstractions;

namespace FinancialInteroperability.Application.Commands.RecordClaimAdjudication;

public sealed record RecordClaimAdjudicationCommand(
    Ulid CorrelationId,
    Ulid FinancialClaimId,
    string ExternalClaimResponseId,
    string? OutcomeDisplay,
    string? AuthenticatedUserId = null) : ICommand<RecordClaimAdjudicationResult>;
