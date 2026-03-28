using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.Abstractions;
using AdministrationConfiguration.Domain.ValueObjects;

using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Commands.CreateRuleSetDraft;

public sealed class CreateRuleSetDraftCommandHandler : ICommandHandler<CreateRuleSetDraftCommand, Ulid>
{
    private readonly IRuleSetRepository _ruleSets;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public CreateRuleSetDraftCommandHandler(
        IRuleSetRepository ruleSets,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _ruleSets = ruleSets ?? throw new ArgumentNullException(nameof(ruleSets));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<Ulid> HandleAsync(
        CreateRuleSetDraftCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var version = new RuleVersion(command.RuleVersion);
        var doc = new RulesDocumentPayload(command.RulesDocument);
        RuleSet draft = RuleSet.CreateDraft(version, doc);
        await _ruleSets.AddAsync(draft, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Create,
                    "RuleSet",
                    draft.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Rule set draft created (version {version.Value}).",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return draft.Id;
    }
}
