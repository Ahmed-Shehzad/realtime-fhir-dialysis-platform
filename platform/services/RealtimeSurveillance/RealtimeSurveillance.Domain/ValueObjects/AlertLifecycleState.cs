namespace RealtimeSurveillance.Domain.ValueObjects;

public sealed class AlertLifecycleState : IEquatable<AlertLifecycleState>
{
    public const int MaxLength = 32;

    public string Value { get; }

    private AlertLifecycleState(string value) => Value = value;

    public static AlertLifecycleState Active { get; } = new("Active");

    public static AlertLifecycleState Acknowledged { get; } = new("Acknowledged");

    public static AlertLifecycleState Escalated { get; } = new("Escalated");

    public static AlertLifecycleState Resolved { get; } = new("Resolved");

    public static AlertLifecycleState FromStored(string? stored)
    {
        string t = (stored ?? string.Empty).Trim();
        return t switch
        {
            "Active" => Active,
            "Acknowledged" => Acknowledged,
            "Escalated" => Escalated,
            "Resolved" => Resolved,
            _ => throw new InvalidOperationException($"Unknown alert lifecycle state: {t}."),
        };
    }

    public bool Equals(AlertLifecycleState? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is AlertLifecycleState s && Equals(s);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(AlertLifecycleState? a, AlertLifecycleState? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(AlertLifecycleState? a, AlertLifecycleState? b) => !(a == b);
}
