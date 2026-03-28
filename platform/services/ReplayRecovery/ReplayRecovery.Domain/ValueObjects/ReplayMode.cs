namespace ReplayRecovery.Domain.ValueObjects;

public sealed class ReplayMode : IEquatable<ReplayMode>
{
    public const int MaxLength = 32;

    public string Value { get; }

    private ReplayMode(string value) => Value = value;

    public static ReplayMode Deterministic { get; } = new("Deterministic");

    public static ReplayMode Shadow { get; } = new("Shadow");

    public static ReplayMode ParseApi(string? raw)
    {
        string t = (raw ?? string.Empty).Trim();
        return t switch
        {
            nameof(Deterministic) => Deterministic,
            nameof(Shadow) => Shadow,
            _ => throw new ArgumentException("Replay mode must be Deterministic or Shadow.", nameof(raw)),
        };
    }

    public static ReplayMode FromStored(string? stored)
    {
        string t = (stored ?? string.Empty).Trim();
        return t switch
        {
            "Deterministic" => Deterministic,
            "Shadow" => Shadow,
            _ => throw new InvalidOperationException($"Unknown replay mode: {t}."),
        };
    }

    public bool Equals(ReplayMode? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is ReplayMode m && Equals(m);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(ReplayMode? a, ReplayMode? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(ReplayMode? a, ReplayMode? b) => !(a == b);
}
