namespace QueryReadModel.Application.Abstractions;

public interface IReadModelProjectionMaintenance
{
    Task<int> ClearAndSeedStubAsync(CancellationToken cancellationToken = default);
}
