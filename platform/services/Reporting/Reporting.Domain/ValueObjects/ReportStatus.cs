namespace Reporting.Domain.ValueObjects;

public sealed class ReportStatus : IEquatable<ReportStatus>
{
    public const int MaxLength = 24;

    public string Value { get; }

    private ReportStatus(string value) => Value = value;

    public static ReportStatus Draft { get; } = new("Draft");

    public static ReportStatus Finalized { get; } = new("Finalized");

    public static ReportStatus Published { get; } = new("Published");

    public static ReportStatus FromStored(string? stored)
    {
        string t = (stored ?? string.Empty).Trim();
        return t switch
        {
            "Draft" => Draft,
            "Finalized" => Finalized,
            "Published" => Published,
            _ => throw new InvalidOperationException($"Unknown report status: {t}."),
        };
    }

    public bool Equals(ReportStatus? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is ReportStatus s && Equals(s);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(ReportStatus? a, ReportStatus? b) =>
        ReferenceEquals(a, b) || (a is not null && b is not null && a.Value == b.Value);

    public static bool operator !=(ReportStatus? a, ReportStatus? b) => !(a == b);
}
