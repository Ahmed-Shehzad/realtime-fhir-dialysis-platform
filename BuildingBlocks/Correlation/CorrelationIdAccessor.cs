using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Correlation;

/// <inheritdoc />
public sealed class CorrelationIdAccessor : ICorrelationIdAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public Ulid GetOrCreate()
    {
        HttpContext? http = _httpContextAccessor.HttpContext;
        if (http?.Items.TryGetValue(CorrelationIdConstants.HttpContextItemKey, out object? value) == true
            && value is Ulid ulid)
            return ulid;

        return Ulid.NewUlid();
    }
}
