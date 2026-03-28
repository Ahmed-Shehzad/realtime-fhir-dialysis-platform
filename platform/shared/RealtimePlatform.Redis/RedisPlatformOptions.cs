namespace RealtimePlatform.Redis;

/// <summary>Redis wiring aligned with ADR-0001 (cache-aside; SignalR backplane may reuse the same multiplexer later).</summary>
public sealed class RedisPlatformOptions
{
    public const string SectionName = "Redis";

    /// <summary>When false, no <see cref="StackExchange.Redis.IConnectionMultiplexer"/> or distributed cache is registered.</summary>
    public bool Enabled { get; set; }

    /// <summary>StackExchange.Redis configuration string (cluster/sentinel as supported by the client). Falls back to connection string <c>Redis</c> when empty.</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Prefix for distributed cache instance keys (tenant-aware segments should be appended by callers).</summary>
    public string InstanceName { get; set; } = "realtime-platform:";
}
