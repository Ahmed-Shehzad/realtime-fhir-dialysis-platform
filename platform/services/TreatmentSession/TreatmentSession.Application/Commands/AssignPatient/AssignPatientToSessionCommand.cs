using Intercessor.Abstractions;

namespace TreatmentSession.Application.Commands.AssignPatient;

public sealed record AssignPatientToSessionCommand(
    Ulid CorrelationId,
    Ulid SessionId,
    string MedicalRecordNumber,
    string? AuthenticatedUserId = null) : ICommand<bool>;
