using Intercessor.Abstractions;

namespace RealtimeSurveillance.Application.Queries.GetSessionRiskSnapshot;

public sealed record GetSessionRiskSnapshotQuery(string TreatmentSessionId) : IQuery<SessionRiskReadDto?>;
