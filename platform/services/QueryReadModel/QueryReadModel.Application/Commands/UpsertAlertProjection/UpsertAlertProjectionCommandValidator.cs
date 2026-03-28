using Verifier;

namespace QueryReadModel.Application.Commands.UpsertAlertProjection;

public sealed class UpsertAlertProjectionCommandValidator : AbstractValidator<UpsertAlertProjectionCommand>
{
    public UpsertAlertProjectionCommandValidator()
    {
        _ = RuleFor(c => c.AlertRowKey).NotEmpty();
        _ = RuleFor(c => c.AlertType).NotEmpty();
        _ = RuleFor(c => c.Severity).NotEmpty();
        _ = RuleFor(c => c.AlertState).NotEmpty();
    }
}
