using Intercessor.Abstractions;

namespace SignalConditioning.Application.Commands.ConditionSignal;

public sealed record ConditionSignalCommand(
    Ulid CorrelationId,
    string MeasurementId,
    string ChannelId,
    double? SampleValue,
    double? PreviousSampleValue,
    string? AuthenticatedUserId = null) : ICommand<ConditionSignalResult>;
