using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Commands.UpsertFeatureToggle;

public sealed record UpsertFeatureToggleCommand(
    Ulid CorrelationId,
    string FeatureKey,
    bool IsEnabled,
    string? AuthenticatedUserId = null) : ICommand;
