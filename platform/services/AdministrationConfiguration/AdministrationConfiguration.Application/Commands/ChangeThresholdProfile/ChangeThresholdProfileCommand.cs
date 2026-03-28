using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Commands.ChangeThresholdProfile;

public sealed record ChangeThresholdProfileCommand(
    Ulid CorrelationId,
    Ulid ProfileId,
    string PayloadJson,
    string? AuthenticatedUserId = null) : ICommand;
