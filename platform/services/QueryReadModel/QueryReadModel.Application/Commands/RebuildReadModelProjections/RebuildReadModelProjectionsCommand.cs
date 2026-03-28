using Intercessor.Abstractions;

namespace QueryReadModel.Application.Commands.RebuildReadModelProjections;

public sealed record RebuildReadModelProjectionsCommand(
    Ulid CorrelationId,
    string? AuthenticatedUserId = null) : ICommand<int>;
