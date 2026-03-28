using Intercessor.Abstractions;

namespace QueryReadModel.Application.Commands.UpsertAlertProjection;

public sealed record UpsertAlertProjectionCommand(
    string AlertRowKey,
    string AlertType,
    string Severity,
    string AlertState,
    string? TreatmentSessionId,
    DateTimeOffset RaisedAtUtc,
    string? AuthenticatedUserId = null) : ICommand<bool>;
