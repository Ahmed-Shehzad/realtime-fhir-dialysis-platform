namespace ClinicalAnalytics.Domain.ValueObjects;

public sealed class ModelVersion : IEquatable<ModelVersion>
{
    public const int MaxLength = 64;

    public string Value { get; }

    public ModelVersion(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string t = value.Trim();
        if (t.Length > MaxLength)
            throw new ArgumentException($"Model version exceeds {MaxLength} characters.", nameof(value));
        Value = t;
    }

    public bool Equals(ModelVersion? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is ModelVersion v && Equals(v);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(ModelVersion? a, ModelVersion? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(ModelVersion? a, ModelVersion? b) => !(a == b);
}
