using Verifier;

namespace TreatmentSession.Application.Commands.AssignPatient;

public sealed class AssignPatientToSessionCommandValidator : AbstractValidator<AssignPatientToSessionCommand>
{
    public AssignPatientToSessionCommandValidator()
    {
        _ = RuleFor(c => c.MedicalRecordNumber).NotEmpty();
    }
}
