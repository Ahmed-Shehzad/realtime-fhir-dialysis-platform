namespace ClinicalAnalytics.Domain.ValueObjects;

public sealed class MetricCode : IEquatable<MetricCode>
{
    public const int MaxLength = 64;

    public string Value { get; }

    public MetricCode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string t = value.Trim();
        if (t.Length > MaxLength)
            throw new ArgumentException($"Metric code exceeds {MaxLength} characters.", nameof(value));
        Value = t;
    }

    public bool Equals(MetricCode? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is MetricCode c && Equals(c);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(MetricCode? a, MetricCode? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(MetricCode? a, MetricCode? b) => !(a == b);
}
