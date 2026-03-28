using BuildingBlocks;
using BuildingBlocks.ValueObjects;

using RealtimePlatform.IntegrationEventCatalog;

using Reporting.Domain.ValueObjects;

namespace Reporting.Domain;

public sealed class SessionReport : AggregateRoot
{
    private SessionReport()
    {
    }

    public SessionId TreatmentSessionId { get; private set; }

    public ReportStatus Status { get; private set; } = null!;

    public NarrativeVersion NarrativeVersion { get; private set; } = null!;

    public List<ReportSection> Sections { get; private set; } = [];

    public List<SupportingEvidence> EvidenceItems { get; private set; } = [];

    public static SessionReport GenerateMvp(
        Ulid correlationId,
        SessionId treatmentSessionId,
        NarrativeVersion narrativeVersion,
        IReadOnlyList<EvidenceReference>? evidenceLinks,
        string? tenantId)
    {
        ArgumentNullException.ThrowIfNull(narrativeVersion);
        var report = new SessionReport
        {
            TreatmentSessionId = treatmentSessionId,
            Status = ReportStatus.Draft,
            NarrativeVersion = narrativeVersion,
        };
        report.ApplyCreatedDateTime();
        Ulid reportId = report.Id;
        string sessionStr = treatmentSessionId.Value;
        string reportIdStr = reportId.ToString();

        report.Sections.Add(
            ReportSection.Create(reportId, new SectionHeading("Summary"), "MVP session summary placeholder."));
        report.Sections.Add(
            ReportSection.Create(
                reportId,
                new SectionHeading("Clinical narrative"),
                "Placeholder narrative; not for sole clinical decision-making."));

        if (evidenceLinks is { Count: > 0 })
            foreach (EvidenceReference link in evidenceLinks)
                report.EvidenceItems.Add(SupportingEvidence.Create(reportId, link));
        else
            report.EvidenceItems.Add(
                SupportingEvidence.Create(
                    reportId,
                    new EvidenceReference(
                        EvidenceKind.SessionAnalysis,
                        new EvidenceLocator("urn:placeholder:session-analysis:none"))));

        report.ApplyEvent(
            new SessionReportGeneratedIntegrationEvent(correlationId, reportIdStr, sessionStr, narrativeVersion.Value)
            {
                SessionId = sessionStr,
                TenantId = tenantId,
            });

        return report;
    }

    public void FinalizeReport(Ulid correlationId, string? tenantId)
    {
        if (Status != ReportStatus.Draft)
            throw new InvalidOperationException("Only draft reports can be finalized.");

        Status = ReportStatus.Finalized;
        ApplyUpdateDateTime();
        string sessionStr = TreatmentSessionId.Value;
        ApplyEvent(
            new SessionReportFinalizedIntegrationEvent(correlationId, Id.ToString(), sessionStr)
            {
                SessionId = sessionStr,
                TenantId = tenantId,
            });
    }

    public void PublishDiagnosticReport(Ulid correlationId, string? publicationTargetHint, string? tenantId)
    {
        if (Status != ReportStatus.Finalized)
            throw new InvalidOperationException("Only finalized reports can be published.");

        Status = ReportStatus.Published;
        ApplyUpdateDateTime();
        string sessionStr = TreatmentSessionId.Value;
        ApplyEvent(
            new DiagnosticReportPublishedIntegrationEvent(correlationId, Id.ToString(), sessionStr, publicationTargetHint)
            {
                SessionId = sessionStr,
                TenantId = tenantId,
            });
    }
}
