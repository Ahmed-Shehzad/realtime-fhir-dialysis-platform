using BuildingBlocks.Abstractions;

namespace TreatmentSession.Domain.Abstractions;

/// <summary>
/// Persistence port for <see cref="DialysisSession"/>.
/// </summary>
public interface ISessionRepository : IRepository<TreatmentSession.Domain.DialysisSession>
{
    Task<TreatmentSession.Domain.DialysisSession?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default);
}
