using Verifier;

namespace AuditProvenance.Application.Commands.RecordPlatformAuditFact;

public sealed class RecordPlatformAuditFactCommandValidator : AbstractValidator<RecordPlatformAuditFactCommand>
{
    public RecordPlatformAuditFactCommandValidator()
    {
        _ = RuleFor(c => c.EventType).NotEmpty();
        _ = RuleFor(c => c.Summary).NotEmpty();
        _ = RuleFor(c => c.SourceSystem).NotEmpty();
    }
}
