using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Queries.GetThresholdProfileById;

public sealed record GetThresholdProfileByIdQuery(Ulid ProfileId) : IQuery<ThresholdProfileReadDto?>;
