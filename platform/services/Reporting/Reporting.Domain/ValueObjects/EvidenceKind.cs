namespace Reporting.Domain.ValueObjects;

public sealed class EvidenceKind : IEquatable<EvidenceKind>
{
    public const int MaxLength = 64;

    public string Value { get; }

    private EvidenceKind(string value) => Value = value;

    public static EvidenceKind CanonicalObservation { get; } = new("CanonicalObservation");

    public static EvidenceKind SessionAnalysis { get; } = new("SessionAnalysis");

    public static EvidenceKind Other { get; } = new("Other");

    public static EvidenceKind ParseApi(string? raw)
    {
        string t = (raw ?? string.Empty).Trim();
        return t switch
        {
            nameof(CanonicalObservation) => CanonicalObservation,
            nameof(SessionAnalysis) => SessionAnalysis,
            nameof(Other) => Other,
            _ => throw new ArgumentException("Evidence kind must be CanonicalObservation, SessionAnalysis, or Other.", nameof(raw)),
        };
    }

    public static EvidenceKind FromStored(string? stored)
    {
        string t = (stored ?? string.Empty).Trim();
        return t switch
        {
            "CanonicalObservation" => CanonicalObservation,
            "SessionAnalysis" => SessionAnalysis,
            "Other" => Other,
            _ => throw new InvalidOperationException($"Unknown evidence kind: {t}."),
        };
    }

    public bool Equals(EvidenceKind? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is EvidenceKind k && Equals(k);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(EvidenceKind? a, EvidenceKind? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(EvidenceKind? a, EvidenceKind? b) => !(a == b);
}
