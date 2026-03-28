using Intercessor.Abstractions;

namespace QueryReadModel.Application.Commands.UpsertSessionOverviewProjection;

public sealed record UpsertSessionOverviewProjectionCommand(
    string TreatmentSessionId,
    string SessionState,
    string? PatientDisplayLabel,
    string? LinkedDeviceId,
    DateTimeOffset SessionStartedAtUtc,
    string? AuthenticatedUserId = null) : ICommand<bool>;
