namespace Reporting.Application.Queries.GetSessionReportById;

public sealed record ReportSectionReadDto(string Heading, string Body);

public sealed record SupportingEvidenceReadDto(string Kind, string Locator);

public sealed record SessionReportReadDto(
    string Id,
    string TreatmentSessionId,
    string Status,
    string NarrativeVersion,
    IReadOnlyList<ReportSectionReadDto> Sections,
    IReadOnlyList<SupportingEvidenceReadDto> SupportingEvidence,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
