namespace QueryReadModel.Application.Queries.ListAlertProjections;

public sealed record AlertProjectionReadDto(
    string Id,
    string AlertRowKey,
    string AlertType,
    string Severity,
    string AlertState,
    string? TreatmentSessionId,
    DateTimeOffset RaisedAtUtc,
    DateTimeOffset ProjectionUpdatedAtUtc);
