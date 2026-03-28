using Intercessor.Abstractions;

namespace ReplayRecovery.Application.Queries.GetReplayJobById;

public sealed record GetReplayJobByIdQuery(Ulid ReplayJobId) : IQuery<ReplayJobReadDto?>;
