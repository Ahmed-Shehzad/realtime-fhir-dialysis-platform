using SignalConditioning.Domain;
using SignalConditioning.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

namespace SignalConditioning.Application.Commands.ConditionSignal;

public sealed class ConditionSignalCommandHandler : ICommandHandler<ConditionSignalCommand, ConditionSignalResult>
{
    private readonly IConditioningResultRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public ConditionSignalCommandHandler(
        IConditioningResultRepository repository,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<ConditionSignalResult> HandleAsync(
        ConditionSignalCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ConditioningResult result = ConditioningResult.Run(
            command.CorrelationId,
            command.MeasurementId,
            command.ChannelId,
            command.SampleValue,
            command.PreviousSampleValue,
            _tenant.TenantId);
        await _repository.AddAsync(result, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Execute,
                    "ConditioningResult",
                    result.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Signal conditioning evaluated; dropout={result.IsDropout}, drift={result.DriftDetected}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new ConditionSignalResult(result.Id, result.IsDropout, result.QualityScorePercent);
    }
}
