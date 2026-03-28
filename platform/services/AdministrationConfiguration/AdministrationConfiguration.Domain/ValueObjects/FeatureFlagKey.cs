namespace AdministrationConfiguration.Domain.ValueObjects;

public sealed class FeatureFlagKey : IEquatable<FeatureFlagKey>
{
    public const int MaxLength = 256;

    public string Value { get; }

    public FeatureFlagKey(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"FeatureFlagKey exceeds max length {MaxLength}.", nameof(value));
        Value = trimmed;
    }

    private FeatureFlagKey()
    {
        Value = null!;
    }

    public override string ToString() => Value;

    public bool Equals(FeatureFlagKey? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is FeatureFlagKey other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static implicit operator string(FeatureFlagKey k) => k.Value;
}
