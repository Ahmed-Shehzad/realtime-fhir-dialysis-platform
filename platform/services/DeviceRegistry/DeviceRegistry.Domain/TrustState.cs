namespace DeviceRegistry.Domain;

/// <summary>
/// Device trust lifecycle classification (bounded vocabulary).
/// </summary>
public sealed class TrustState : IEquatable<TrustState>
{
    public string Value { get; }

    private TrustState(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public static TrustState Active { get; } = new("Active");
    public static TrustState Suspended { get; } = new("Suspended");
    public static TrustState Retired { get; } = new("Retired");

    public static TrustState From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (string.Equals(Active.Value, value, StringComparison.OrdinalIgnoreCase)) return Active;
        if (string.Equals(Suspended.Value, value, StringComparison.OrdinalIgnoreCase)) return Suspended;
        if (string.Equals(Retired.Value, value, StringComparison.OrdinalIgnoreCase)) return Retired;
        throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown trust state.");
    }

    public bool Equals(TrustState? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is TrustState other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => Value;
}
