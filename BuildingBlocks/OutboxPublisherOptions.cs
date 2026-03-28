namespace BuildingBlocks;

/// <summary>
/// Optional metadata for integration event publishes (for example transport headers).
/// </summary>
public sealed class OutboxPublisherOptions
{
    /// <summary>
    /// Logical sender URI (for example the publishing service base address).
    /// </summary>
    public Uri? SourceAddress { get; set; }
}
