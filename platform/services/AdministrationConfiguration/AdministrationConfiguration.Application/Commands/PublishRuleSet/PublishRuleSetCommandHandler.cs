using AdministrationConfiguration.Domain;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using AdministrationConfiguration.Domain.Abstractions;

using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Commands.PublishRuleSet;

public sealed class PublishRuleSetCommandHandler : ICommandHandler<PublishRuleSetCommand>
{
    private readonly IRuleSetRepository _ruleSets;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public PublishRuleSetCommandHandler(
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

    public async Task HandleAsync(PublishRuleSetCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        RuleSet ruleSet = await _ruleSets.GetByIdForUpdateAsync(command.RuleSetId, cancellationToken).ConfigureAwait(false)
                          ?? throw new InvalidOperationException("Rule set not found.");
        ruleSet.Publish(command.CorrelationId, _tenant.TenantId);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Update,
                    "RuleSet",
                    ruleSet.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Rule set published (version {ruleSet.Version.Value}).",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
