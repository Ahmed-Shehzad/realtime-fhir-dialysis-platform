using Intercessor.Abstractions;

namespace RealtimeSurveillance.Application.Commands.UpdateSessionRiskSnapshot;

public sealed record UpdateSessionRiskSnapshotCommand(
    Ulid CorrelationId,
    string TreatmentSessionId,
    string RiskLevel,
    string? AuthenticatedUserId = null) : ICommand;
