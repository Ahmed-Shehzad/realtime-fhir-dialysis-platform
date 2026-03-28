namespace RealtimeSurveillance.Domain.ValueObjects;

public sealed class SessionRiskLevel : IEquatable<SessionRiskLevel>
{
    public const int MaxLength = 32;

    public string Value { get; }

    private SessionRiskLevel(string value) => Value = value;

    public static SessionRiskLevel Low { get; } = new("Low");

    public static SessionRiskLevel Medium { get; } = new("Medium");

    public static SessionRiskLevel High { get; } = new("High");

    public static SessionRiskLevel Critical { get; } = new("Critical");

    public static SessionRiskLevel FromRequiredString(string raw)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(raw);
        string t = raw.Trim();
        if (t.Equals("Low", StringComparison.OrdinalIgnoreCase))
            return Low;
        if (t.Equals("Medium", StringComparison.OrdinalIgnoreCase))
            return Medium;
        if (t.Equals("High", StringComparison.OrdinalIgnoreCase))
            return High;
        if (t.Equals("Critical", StringComparison.OrdinalIgnoreCase))
            return Critical;
        throw new ArgumentOutOfRangeException(nameof(raw), t, "Risk level must be Low, Medium, High, or Critical.");
    }

    public static SessionRiskLevel FromStored(string? stored)
    {
        string t = (stored ?? string.Empty).Trim();
        return t switch
        {
            "Low" => Low,
            "Medium" => Medium,
            "High" => High,
            "Critical" => Critical,
            _ => throw new InvalidOperationException($"Unknown session risk level: {t}."),
        };
    }

    public bool Equals(SessionRiskLevel? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is SessionRiskLevel s && Equals(s);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(SessionRiskLevel? a, SessionRiskLevel? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(SessionRiskLevel? a, SessionRiskLevel? b) => !(a == b);
}
