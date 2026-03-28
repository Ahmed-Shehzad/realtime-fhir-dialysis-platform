using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;
using BuildingBlocks.ValueObjects;

using Reporting.Domain;
using Reporting.Domain.Abstractions;
using Reporting.Domain.ValueObjects;

using Intercessor.Abstractions;

namespace Reporting.Application.Commands.GenerateSessionReport;

public sealed class GenerateSessionReportCommandHandler : ICommandHandler<GenerateSessionReportCommand, Ulid>
{
    private readonly ISessionReportRepository _reports;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public GenerateSessionReportCommandHandler(
        ISessionReportRepository reports,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _reports = reports ?? throw new ArgumentNullException(nameof(reports));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<Ulid> HandleAsync(
        GenerateSessionReportCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.TreatmentSessionId))
            throw new ArgumentException("TreatmentSessionId is required.", nameof(command));
        if (string.IsNullOrWhiteSpace(command.NarrativeVersion))
            throw new ArgumentException("NarrativeVersion is required.", nameof(command));

        var sessionId = new SessionId(command.TreatmentSessionId.Trim());
        var narrativeVersion = new NarrativeVersion(command.NarrativeVersion.Trim());
        List<EvidenceReference>? links = null;
        if (command.Evidence is { Count: > 0 })
        {
            links = new List<EvidenceReference>(command.Evidence.Count);
            foreach (SessionReportEvidenceDto row in command.Evidence)
            {
                if (string.IsNullOrWhiteSpace(row.Locator))
                    throw new ArgumentException("Each evidence item requires a locator.", nameof(command));
                var kind = EvidenceKind.ParseApi(row.Kind);
                var locator = new EvidenceLocator(row.Locator.Trim());
                links.Add(new EvidenceReference(kind, locator));
            }
        }

        SessionReport report = SessionReport.GenerateMvp(
            command.CorrelationId,
            sessionId,
            narrativeVersion,
            links,
            _tenant.TenantId);

        await _reports.AddAsync(report, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Create,
                    "SessionReport",
                    report.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"MVP session report generated for session {sessionId.Value} (narrative {narrativeVersion.Value}).",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return report.Id;
    }
}
