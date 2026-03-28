namespace AdministrationConfiguration.Domain.ValueObjects;

public sealed class RuleVersion : IEquatable<RuleVersion>
{
    public const int MaxLength = 64;

    public string Value { get; }

    public RuleVersion(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"RuleVersion exceeds max length {MaxLength}.", nameof(value));
        Value = trimmed;
    }

    private RuleVersion()
    {
        Value = null!;
    }

    public override string ToString() => Value;

    public bool Equals(RuleVersion? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is RuleVersion other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static implicit operator string(RuleVersion v) => v.Value;
}
