using Intercessor.Abstractions;

using TerminologyConformance.Domain.Abstractions;

namespace TerminologyConformance.Application.Queries.GetLatestConformanceAssessment;

public sealed record GetLatestConformanceAssessmentQuery(string ResourceId) : IQuery<ConformanceAssessmentSummary?>;
