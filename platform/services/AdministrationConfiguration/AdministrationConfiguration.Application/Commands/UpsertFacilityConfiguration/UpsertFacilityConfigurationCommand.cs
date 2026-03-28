using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Commands.UpsertFacilityConfiguration;

public sealed record UpsertFacilityConfigurationCommand(
    Ulid CorrelationId,
    string FacilityId,
    string ConfigurationJson,
    DateTimeOffset? EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    string? AuthenticatedUserId = null) : ICommand;
