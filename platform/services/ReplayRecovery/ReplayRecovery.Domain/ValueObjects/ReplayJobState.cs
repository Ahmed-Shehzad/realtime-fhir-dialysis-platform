namespace ReplayRecovery.Domain.ValueObjects;

public sealed class ReplayJobState : IEquatable<ReplayJobState>
{
    public const int MaxLength = 24;

    public string Value { get; }

    private ReplayJobState(string value) => Value = value;

    public static ReplayJobState Running { get; } = new("Running");

    public static ReplayJobState Paused { get; } = new("Paused");

    public static ReplayJobState Completed { get; } = new("Completed");

    public static ReplayJobState Failed { get; } = new("Failed");

    public static ReplayJobState Cancelled { get; } = new("Cancelled");

    public static ReplayJobState FromStored(string? stored)
    {
        string t = (stored ?? string.Empty).Trim();
        return t switch
        {
            "Running" => Running,
            "Paused" => Paused,
            "Completed" => Completed,
            "Failed" => Failed,
            "Cancelled" => Cancelled,
            _ => throw new InvalidOperationException($"Unknown replay job state: {t}."),
        };
    }

    public bool Equals(ReplayJobState? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is ReplayJobState s && Equals(s);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(ReplayJobState? a, ReplayJobState? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(ReplayJobState? a, ReplayJobState? b) => !(a == b);
}
