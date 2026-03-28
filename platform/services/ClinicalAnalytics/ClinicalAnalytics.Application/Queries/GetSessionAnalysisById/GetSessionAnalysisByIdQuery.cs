using Intercessor.Abstractions;

namespace ClinicalAnalytics.Application.Queries.GetSessionAnalysisById;

public sealed record GetSessionAnalysisByIdQuery(Ulid AnalysisId) : IQuery<SessionAnalysisReadDto?>;
