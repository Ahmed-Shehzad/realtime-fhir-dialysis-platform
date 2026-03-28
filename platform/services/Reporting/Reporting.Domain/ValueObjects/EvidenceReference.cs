namespace Reporting.Domain.ValueObjects;

public sealed class EvidenceReference : IEquatable<EvidenceReference>
{
    public EvidenceKind Kind { get; }

    public EvidenceLocator Locator { get; }

    public EvidenceReference(EvidenceKind kind, EvidenceLocator locator)
    {
        ArgumentNullException.ThrowIfNull(kind);
        ArgumentNullException.ThrowIfNull(locator);
        Kind = kind;
        Locator = locator;
    }

    public bool Equals(EvidenceReference? other) =>
        other is not null && Kind == other.Kind && Locator == other.Locator;

    public override bool Equals(object? obj) => obj is EvidenceReference r && Equals(r);

    public override int GetHashCode() => HashCode.Combine(Kind, Locator);

    public static bool operator ==(EvidenceReference? a, EvidenceReference? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Equals(b));

    public static bool operator !=(EvidenceReference? a, EvidenceReference? b) => !(a == b);
}
