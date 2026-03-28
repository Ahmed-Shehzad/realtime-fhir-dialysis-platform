namespace ReplayRecovery.Domain.ValueObjects;

public sealed class RecoveryPlanCode : IEquatable<RecoveryPlanCode>
{
    public const int MaxLength = 128;

    public string Value { get; }

    public RecoveryPlanCode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string t = value.Trim();
        if (t.Length > MaxLength)
            throw new ArgumentException($"Recovery plan code exceeds {MaxLength} characters.", nameof(value));
        Value = t;
    }

    public bool Equals(RecoveryPlanCode? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is RecoveryPlanCode c && Equals(c);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(RecoveryPlanCode? a, RecoveryPlanCode? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(RecoveryPlanCode? a, RecoveryPlanCode? b) => !(a == b);
}
