using AuditProvenance.Domain;
using AuditProvenance.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

namespace AuditProvenance.Application.Commands.RecordPlatformAuditFact;

public sealed class RecordPlatformAuditFactCommandHandler : ICommandHandler<RecordPlatformAuditFactCommand, RecordPlatformAuditFactResult>
{
    private readonly IPlatformAuditFactRepository _facts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public RecordPlatformAuditFactCommandHandler(
        IPlatformAuditFactRepository facts,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _facts = facts ?? throw new ArgumentNullException(nameof(facts));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<RecordPlatformAuditFactResult> HandleAsync(
        RecordPlatformAuditFactCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        DateTimeOffset occurred = command.OccurredAtUtc ?? DateTimeOffset.UtcNow;
        PlatformAuditFact fact = PlatformAuditFact.Record(
            command.CorrelationId,
            new PlatformAuditFactPayload
            {
                OccurredAtUtc = occurred,
                EventType = command.EventType,
                Summary = command.Summary,
                DetailJson = command.DetailJson,
                CorrelationIdString = command.CorrelationIdString,
                CausationIdString = command.CausationIdString,
                TenantId = _tenant.TenantId,
                ActorId = command.ActorId ?? command.AuthenticatedUserId,
                SourceSystem = command.SourceSystem,
                RelatedResourceType = command.RelatedResourceType,
                RelatedResourceId = command.RelatedResourceId,
                SessionId = command.SessionId,
                PatientId = command.PatientId,
            });
        await _facts.AddAsync(fact, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Create,
                    "PlatformAuditFact",
                    fact.Id.ToString(),
                    command.AuthenticatedUserId ?? command.ActorId,
                    AuditOutcome.Success,
                    "Platform audit fact recorded.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new RecordPlatformAuditFactResult(fact.Id);
    }
}
