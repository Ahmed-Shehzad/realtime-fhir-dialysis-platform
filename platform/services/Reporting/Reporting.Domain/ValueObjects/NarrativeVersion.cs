namespace Reporting.Domain.ValueObjects;

public sealed class NarrativeVersion : IEquatable<NarrativeVersion>
{
    public const int MaxLength = 64;

    public string Value { get; }

    public NarrativeVersion(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string t = value.Trim();
        if (t.Length > MaxLength)
            throw new ArgumentException($"Narrative version exceeds {MaxLength} characters.", nameof(value));
        Value = t;
    }

    public bool Equals(NarrativeVersion? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is NarrativeVersion v && Equals(v);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(NarrativeVersion? a, NarrativeVersion? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(NarrativeVersion? a, NarrativeVersion? b) => !(a == b);
}
