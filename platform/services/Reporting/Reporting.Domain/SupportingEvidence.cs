using Reporting.Domain.ValueObjects;

namespace Reporting.Domain;

public sealed class SupportingEvidence
{
    public Ulid Id { get; private set; }

    public Ulid SessionReportId { get; private set; }

    public EvidenceKind Kind { get; private set; } = null!;

    public EvidenceLocator Locator { get; private set; } = null!;

    private SupportingEvidence()
    {
    }

    internal static SupportingEvidence Create(Ulid sessionReportId, EvidenceReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        return new SupportingEvidence
        {
            Id = Ulid.NewUlid(),
            SessionReportId = sessionReportId,
            Kind = reference.Kind,
            Locator = reference.Locator,
        };
    }
}
