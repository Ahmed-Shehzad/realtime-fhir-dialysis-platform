using Verifier;

namespace QueryReadModel.Application.Commands.UpsertSessionOverviewProjection;

public sealed class UpsertSessionOverviewProjectionCommandValidator : AbstractValidator<UpsertSessionOverviewProjectionCommand>
{
    public UpsertSessionOverviewProjectionCommandValidator()
    {
        _ = RuleFor(c => c.TreatmentSessionId).NotEmpty();
        _ = RuleFor(c => c.SessionState).NotEmpty();
    }
}
