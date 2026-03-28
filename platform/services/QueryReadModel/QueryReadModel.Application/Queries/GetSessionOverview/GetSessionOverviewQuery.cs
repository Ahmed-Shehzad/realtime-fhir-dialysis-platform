using Intercessor.Abstractions;

namespace QueryReadModel.Application.Queries.GetSessionOverview;

public sealed record GetSessionOverviewQuery(string TreatmentSessionId) : IQuery<SessionOverviewReadDto?>;
