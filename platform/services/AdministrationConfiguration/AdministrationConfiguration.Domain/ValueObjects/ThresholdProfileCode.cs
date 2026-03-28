namespace AdministrationConfiguration.Domain.ValueObjects;

public sealed class ThresholdProfileCode : IEquatable<ThresholdProfileCode>
{
    public const int MaxLength = 128;

    public string Value { get; }

    public ThresholdProfileCode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"ThresholdProfileCode exceeds max length {MaxLength}.", nameof(value));
        Value = trimmed;
    }

    private ThresholdProfileCode()
    {
        Value = null!;
    }

    public override string ToString() => Value;

    public bool Equals(ThresholdProfileCode? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is ThresholdProfileCode other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static implicit operator string(ThresholdProfileCode c) => c.Value;
}
