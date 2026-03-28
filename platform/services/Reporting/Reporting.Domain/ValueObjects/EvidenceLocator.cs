namespace Reporting.Domain.ValueObjects;

public sealed class EvidenceLocator : IEquatable<EvidenceLocator>
{
    public const int MaxLength = 1024;

    public string Value { get; }

    public EvidenceLocator(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string t = value.Trim();
        if (t.Length > MaxLength)
            throw new ArgumentException($"Evidence locator exceeds {MaxLength} characters.", nameof(value));
        Value = t;
    }

    public bool Equals(EvidenceLocator? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is EvidenceLocator l && Equals(l);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(EvidenceLocator? a, EvidenceLocator? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(EvidenceLocator? a, EvidenceLocator? b) => !(a == b);
}
