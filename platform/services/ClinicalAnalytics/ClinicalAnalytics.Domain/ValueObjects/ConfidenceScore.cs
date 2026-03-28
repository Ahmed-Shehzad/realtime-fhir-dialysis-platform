namespace ClinicalAnalytics.Domain.ValueObjects;

public readonly struct ConfidenceScore : IEquatable<ConfidenceScore>
{
    public int Percent { get; }

    public ConfidenceScore(int percent)
    {
        if (percent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percent), percent, "Confidence must be between 0 and 100 inclusive.");
        Percent = percent;
    }

    public static ConfidenceScore FromPercent(int percent) => new(percent);

    public bool Equals(ConfidenceScore other) => Percent == other.Percent;

    public override bool Equals(object? obj) => obj is ConfidenceScore o && Equals(o);

    public override int GetHashCode() => Percent;

    public static bool operator ==(ConfidenceScore left, ConfidenceScore right) => left.Equals(right);

    public static bool operator !=(ConfidenceScore left, ConfidenceScore right) => !left.Equals(right);
}
