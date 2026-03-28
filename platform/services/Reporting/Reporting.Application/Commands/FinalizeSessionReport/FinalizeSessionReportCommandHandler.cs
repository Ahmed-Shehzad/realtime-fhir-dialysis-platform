using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Reporting.Domain;
using Reporting.Domain.Abstractions;

using Intercessor.Abstractions;

namespace Reporting.Application.Commands.FinalizeSessionReport;

public sealed class FinalizeSessionReportCommandHandler : ICommandHandler<FinalizeSessionReportCommand>
{
    private readonly ISessionReportRepository _reports;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public FinalizeSessionReportCommandHandler(
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

    public async Task HandleAsync(
        FinalizeSessionReportCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        SessionReport? report = await _reports
            .GetByIdForUpdateAsync(command.ReportId, cancellationToken)
            .ConfigureAwait(false);
        if (report is null)
            throw new InvalidOperationException($"Session report {command.ReportId} was not found.");

        report.FinalizeReport(command.CorrelationId, _tenant.TenantId);

        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Update,
                    "SessionReport",
                    report.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    "Session report finalized.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
