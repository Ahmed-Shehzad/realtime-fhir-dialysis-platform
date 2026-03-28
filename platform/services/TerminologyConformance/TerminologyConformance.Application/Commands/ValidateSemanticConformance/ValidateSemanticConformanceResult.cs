using TerminologyConformance.Domain;

namespace TerminologyConformance.Application.Commands.ValidateSemanticConformance;

public sealed record ValidateSemanticConformanceResult(
    Ulid AssessmentId,
    ConformanceSliceOutcome TerminologyOutcome,
    ConformanceSliceOutcome ProfileOutcome);
