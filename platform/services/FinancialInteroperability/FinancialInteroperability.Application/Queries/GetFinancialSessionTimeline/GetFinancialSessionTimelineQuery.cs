using FinancialInteroperability.Domain.Abstractions;

using Intercessor.Abstractions;

namespace FinancialInteroperability.Application.Queries.GetFinancialSessionTimeline;

public sealed record GetFinancialSessionTimelineQuery(string TreatmentSessionId, string? PatientId)
    : IQuery<FinancialSessionTimelineReadModel>;
