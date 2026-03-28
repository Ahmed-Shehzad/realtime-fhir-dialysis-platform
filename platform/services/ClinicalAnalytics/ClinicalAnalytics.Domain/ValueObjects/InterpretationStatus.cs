namespace ClinicalAnalytics.Domain.ValueObjects;

public sealed class InterpretationStatus : IEquatable<InterpretationStatus>
{
    public const int MaxLength = 32;

    public string Value { get; }

    private InterpretationStatus(string value) => Value = value;

    public static InterpretationStatus PendingReview { get; } = new("PendingReview");

    public static InterpretationStatus Completed { get; } = new("Completed");

    public static InterpretationStatus FromStored(string? stored)
    {
        string t = (stored ?? string.Empty).Trim();
        return t switch
        {
            "PendingReview" => PendingReview,
            "Completed" => Completed,
            _ => throw new InvalidOperationException($"Unknown interpretation status: {t}."),
        };
    }

    public bool Equals(InterpretationStatus? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is InterpretationStatus s && Equals(s);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(InterpretationStatus? a, InterpretationStatus? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(InterpretationStatus? a, InterpretationStatus? b) => !(a == b);
}
