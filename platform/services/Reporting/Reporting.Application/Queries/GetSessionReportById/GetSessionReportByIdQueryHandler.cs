using Reporting.Domain;
using Reporting.Domain.Abstractions;

using Intercessor.Abstractions;

namespace Reporting.Application.Queries.GetSessionReportById;

public sealed class GetSessionReportByIdQueryHandler : IQueryHandler<GetSessionReportByIdQuery, SessionReportReadDto?>
{
    private readonly ISessionReportRepository _reports;

    public GetSessionReportByIdQueryHandler(ISessionReportRepository reports) =>
        _reports = reports ?? throw new ArgumentNullException(nameof(reports));

    public async Task<SessionReportReadDto?> HandleAsync(
        GetSessionReportByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        SessionReport? row = await _reports
            .GetByIdAsync(query.ReportId, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return null;

        var sections = row.Sections
            .Select(s => new ReportSectionReadDto(s.Heading.Value, s.Body))
            .ToList();
        var evidence = row.EvidenceItems
            .Select(e => new SupportingEvidenceReadDto(e.Kind.Value, e.Locator.Value))
            .ToList();

        return new SessionReportReadDto(
            row.Id.ToString(),
            row.TreatmentSessionId.Value,
            row.Status.Value,
            row.NarrativeVersion.Value,
            sections,
            evidence,
            row.CreatedAtUtc,
            row.UpdatedAtUtc);
    }
}
