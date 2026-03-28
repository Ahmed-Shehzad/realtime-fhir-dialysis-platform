namespace RealtimeSurveillance.Application.Queries.GetSessionRiskSnapshot;

public sealed record SessionRiskReadDto(
    string SnapshotId,
    string TreatmentSessionId,
    string RiskLevel,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
