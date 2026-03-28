using Verifier;

namespace TerminologyConformance.Application.Commands.ValidateSemanticConformance;

public sealed class ValidateSemanticConformanceCommandValidator : AbstractValidator<ValidateSemanticConformanceCommand>
{
    public ValidateSemanticConformanceCommandValidator() =>
        _ = RuleFor(c => c.ResourceId).NotEmpty();
}
