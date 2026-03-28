using Intercessor.Abstractions;

namespace ClinicalAnalytics.Application.Commands.RunSessionAnalysis;

public sealed record RunSessionAnalysisCommand(
    Ulid CorrelationId,
    string TreatmentSessionId,
    string ModelVersion,
    string? AuthenticatedUserId = null) : ICommand<Ulid>;
