using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Commands.CreateThresholdProfile;

public sealed record CreateThresholdProfileCommand(
    Ulid CorrelationId,
    string ProfileCode,
    string PayloadJson,
    string? AuthenticatedUserId = null) : ICommand<Ulid>;
