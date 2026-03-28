using Intercessor.Abstractions;

namespace TerminologyConformance.Application.Commands.ValidateSemanticConformance;

public sealed record ValidateSemanticConformanceCommand(
    Ulid CorrelationId,
    string ResourceId,
    string? CodeSystemUri,
    string? CodeValue,
    string? UnitCode,
    string? ProfileUrl,
    string? AuthenticatedUserId = null) : ICommand<ValidateSemanticConformanceResult>;
