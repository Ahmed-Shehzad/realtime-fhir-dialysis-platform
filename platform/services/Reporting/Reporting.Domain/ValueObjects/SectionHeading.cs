namespace Reporting.Domain.ValueObjects;

public sealed class SectionHeading : IEquatable<SectionHeading>
{
    public const int MaxLength = 256;

    public string Value { get; }

    public SectionHeading(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string t = value.Trim();
        if (t.Length > MaxLength)
            throw new ArgumentException($"Section heading exceeds {MaxLength} characters.", nameof(value));
        Value = t;
    }

    public bool Equals(SectionHeading? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is SectionHeading h && Equals(h);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(SectionHeading? a, SectionHeading? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(SectionHeading? a, SectionHeading? b) => !(a == b);
}
