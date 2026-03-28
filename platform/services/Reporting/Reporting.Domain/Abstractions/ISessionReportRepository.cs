using BuildingBlocks.Abstractions;

namespace Reporting.Domain.Abstractions;

public interface ISessionReportRepository : IRepository<Reporting.Domain.SessionReport>
{
    Task<Reporting.Domain.SessionReport?> GetByIdAsync(
        Ulid reportId,
        CancellationToken cancellationToken = default);

    Task<Reporting.Domain.SessionReport?> GetByIdForUpdateAsync(
        Ulid reportId,
        CancellationToken cancellationToken = default);
}
