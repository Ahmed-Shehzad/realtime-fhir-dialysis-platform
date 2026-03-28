using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.Abstractions;

using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Queries.GetThresholdProfileById;

public sealed class GetThresholdProfileByIdQueryHandler
    : IQueryHandler<GetThresholdProfileByIdQuery, ThresholdProfileReadDto?>
{
    private readonly IThresholdProfileRepository _profiles;

    public GetThresholdProfileByIdQueryHandler(IThresholdProfileRepository profiles) =>
        _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));

    public async Task<ThresholdProfileReadDto?> HandleAsync(
        GetThresholdProfileByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ThresholdProfile? row = await _profiles.GetByIdAsync(query.ProfileId, cancellationToken).ConfigureAwait(false);
        if (row is null)
            return null;
        return new ThresholdProfileReadDto(
            row.Id.ToString(),
            row.ProfileCode.Value,
            row.Payload.Json,
            row.ProfileRevision);
    }
}
