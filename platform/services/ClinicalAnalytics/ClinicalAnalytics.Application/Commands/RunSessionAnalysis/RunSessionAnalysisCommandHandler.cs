using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;
using BuildingBlocks.ValueObjects;

using ClinicalAnalytics.Domain;
using ClinicalAnalytics.Domain.Abstractions;
using ClinicalAnalytics.Domain.ValueObjects;

using Intercessor.Abstractions;

namespace ClinicalAnalytics.Application.Commands.RunSessionAnalysis;

public sealed class RunSessionAnalysisCommandHandler : ICommandHandler<RunSessionAnalysisCommand, Ulid>
{
    private readonly ISessionAnalysisRepository _analyses;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public RunSessionAnalysisCommandHandler(
        ISessionAnalysisRepository analyses,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _analyses = analyses ?? throw new ArgumentNullException(nameof(analyses));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<Ulid> HandleAsync(RunSessionAnalysisCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.TreatmentSessionId))
            throw new ArgumentException("TreatmentSessionId is required.", nameof(command));

        var sessionId = new SessionId(command.TreatmentSessionId);
        var modelVersion = new ModelVersion(command.ModelVersion);
        SessionAnalysis analysis = SessionAnalysis.RunMvpAnalysis(
            command.CorrelationId,
            sessionId,
            modelVersion,
            _tenant.TenantId);

        await _analyses.AddAsync(analysis, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Create,
                    "SessionAnalysis",
                    analysis.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"MVP session analysis started for session {sessionId.Value} with model {modelVersion.Value}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return analysis.Id;
    }
}
