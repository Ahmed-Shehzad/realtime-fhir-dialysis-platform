using AuditProvenance.Domain;
using AuditProvenance.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

namespace AuditProvenance.Application.Commands.RecordProvenanceLink;

public sealed class RecordProvenanceLinkCommandHandler : ICommandHandler<RecordProvenanceLinkCommand, RecordProvenanceLinkResult>
{
    private readonly IPlatformAuditFactRepository _facts;
    private readonly IProvenanceLinkRepository _links;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public RecordProvenanceLinkCommandHandler(
        IPlatformAuditFactRepository facts,
        IProvenanceLinkRepository links,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _facts = facts ?? throw new ArgumentNullException(nameof(facts));
        _links = links ?? throw new ArgumentNullException(nameof(links));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<RecordProvenanceLinkResult> HandleAsync(
        RecordProvenanceLinkCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        PlatformAuditFact? from = await _facts
            .GetByIdAsync(command.FromPlatformAuditFactId, cancellationToken)
            .ConfigureAwait(false);
        if (from is null)
            throw new InvalidOperationException("Platform audit fact was not found.");
        PlatformAuditFact? to = await _facts
            .GetByIdAsync(command.ToPlatformAuditFactId, cancellationToken)
            .ConfigureAwait(false);
        if (to is null)
            throw new InvalidOperationException("Platform audit fact was not found.");

        ProvenanceLink link = ProvenanceLink.Create(
            command.CorrelationId,
            command.FromPlatformAuditFactId,
            command.ToPlatformAuditFactId,
            command.RelationType,
            _tenant.TenantId);
        await _links.AddAsync(link, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Create,
                    "ProvenanceLink",
                    link.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    "Provenance link recorded.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new RecordProvenanceLinkResult(link.Id);
    }
}
