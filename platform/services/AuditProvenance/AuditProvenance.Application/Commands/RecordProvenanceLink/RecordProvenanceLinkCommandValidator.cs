using Verifier;

namespace AuditProvenance.Application.Commands.RecordProvenanceLink;

public sealed class RecordProvenanceLinkCommandValidator : AbstractValidator<RecordProvenanceLinkCommand>
{
    public RecordProvenanceLinkCommandValidator()
    {
        _ = RuleFor(c => c.RelationType).NotEmpty();
    }
}
