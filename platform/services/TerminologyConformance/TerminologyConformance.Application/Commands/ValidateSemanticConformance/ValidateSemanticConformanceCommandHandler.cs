using TerminologyConformance.Domain;
using TerminologyConformance.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

namespace TerminologyConformance.Application.Commands.ValidateSemanticConformance;

public sealed class ValidateSemanticConformanceCommandHandler
    : ICommandHandler<ValidateSemanticConformanceCommand, ValidateSemanticConformanceResult>
{
    private readonly IConformanceAssessmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public ValidateSemanticConformanceCommandHandler(
        IConformanceAssessmentRepository repository,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<ValidateSemanticConformanceResult> HandleAsync(
        ValidateSemanticConformanceCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ConformanceAssessment assessment = ConformanceAssessment.Run(
            command.CorrelationId,
            command.ResourceId,
            command.CodeSystemUri,
            command.CodeValue,
            command.UnitCode,
            command.ProfileUrl,
            _tenant.TenantId);
        await _repository.AddAsync(assessment, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Execute,
                    "ConformanceAssessment",
                    assessment.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Semantic conformance: terminology={assessment.TerminologySliceOutcome}; profile={assessment.ProfileSliceOutcome}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new ValidateSemanticConformanceResult(
            assessment.Id,
            assessment.TerminologySliceOutcome,
            assessment.ProfileSliceOutcome);
    }
}
