namespace BuildingBlocks.Correlation;

/// <summary>
/// Provides the correlation id for the current HTTP request (set by <see cref="CorrelationIdMiddleware"/>).
/// </summary>
public interface ICorrelationIdAccessor
{
    /// <summary>
    /// Correlation id for the active request, or a newly generated id when not in an HTTP request scope.
    /// </summary>
    Ulid GetOrCreate();
}
