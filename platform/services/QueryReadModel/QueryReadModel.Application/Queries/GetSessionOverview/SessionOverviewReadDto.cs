namespace QueryReadModel.Application.Queries.GetSessionOverview;

public sealed record SessionOverviewReadDto(
    string Id,
    string TreatmentSessionId,
    string SessionState,
    string? PatientDisplayLabel,
    string? LinkedDeviceId,
    DateTimeOffset SessionStartedAtUtc,
    DateTimeOffset ProjectionUpdatedAtUtc);
