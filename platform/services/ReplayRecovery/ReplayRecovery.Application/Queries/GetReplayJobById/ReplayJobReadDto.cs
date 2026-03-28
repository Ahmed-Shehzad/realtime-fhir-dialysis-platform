namespace ReplayRecovery.Application.Queries.GetReplayJobById;

public sealed record ReplayJobReadDto(
    string Id,
    string ReplayMode,
    string State,
    string ProjectionSetName,
    int CheckpointSequence,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
