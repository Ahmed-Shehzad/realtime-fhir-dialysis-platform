using ReplayRecovery.Domain;
using ReplayRecovery.Domain.Abstractions;

using Intercessor.Abstractions;

namespace ReplayRecovery.Application.Queries.GetReplayJobById;

public sealed class GetReplayJobByIdQueryHandler : IQueryHandler<GetReplayJobByIdQuery, ReplayJobReadDto?>
{
    private readonly IReplayJobRepository _jobs;

    public GetReplayJobByIdQueryHandler(IReplayJobRepository jobs) =>
        _jobs = jobs ?? throw new ArgumentNullException(nameof(jobs));

    public async Task<ReplayJobReadDto?> HandleAsync(
        GetReplayJobByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ReplayJob? row = await _jobs.GetByIdAsync(query.ReplayJobId, cancellationToken).ConfigureAwait(false);
        if (row is null)
            return null;

        return new ReplayJobReadDto(
            row.Id.ToString(),
            row.Mode.Value,
            row.State.Value,
            row.ProjectionSet.Value,
            row.CheckpointSequence,
            row.CreatedAtUtc,
            row.UpdatedAtUtc);
    }
}
