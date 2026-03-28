using Reporting.Domain.ValueObjects;

namespace Reporting.Domain;

public sealed class ReportSection
{
    public Ulid Id { get; private set; }

    public Ulid SessionReportId { get; private set; }

    public SectionHeading Heading { get; private set; } = null!;

    public string Body { get; private set; } = null!;

    private ReportSection()
    {
    }

    internal static ReportSection Create(Ulid sessionReportId, SectionHeading heading, string body)
    {
        ArgumentNullException.ThrowIfNull(heading);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        return new ReportSection
        {
            Id = Ulid.NewUlid(),
            SessionReportId = sessionReportId,
            Heading = heading,
            Body = body.Trim(),
        };
    }
}
