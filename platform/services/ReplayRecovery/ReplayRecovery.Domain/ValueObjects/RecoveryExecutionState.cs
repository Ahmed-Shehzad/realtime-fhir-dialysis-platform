namespace ReplayRecovery.Domain.ValueObjects;

public sealed class RecoveryExecutionState : IEquatable<RecoveryExecutionState>
{
    public const int MaxLength = 24;

    public string Value { get; }

    private RecoveryExecutionState(string value) => Value = value;

    public static RecoveryExecutionState Executed { get; } = new("Executed");

    public static RecoveryExecutionState FromStored(string? stored)
    {
        string t = (stored ?? string.Empty).Trim();
        return t switch
        {
            "Executed" => Executed,
            _ => throw new InvalidOperationException($"Unknown recovery execution state: {t}."),
        };
    }

    public bool Equals(RecoveryExecutionState? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is RecoveryExecutionState s && Equals(s);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(RecoveryExecutionState? a, RecoveryExecutionState? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(RecoveryExecutionState? a, RecoveryExecutionState? b) => !(a == b);
}
