namespace AdministrationConfiguration.Application.Queries.GetThresholdProfileById;

public sealed record ThresholdProfileReadDto(
    string Id,
    string ProfileCode,
    string PayloadJson,
    int ProfileRevision);
