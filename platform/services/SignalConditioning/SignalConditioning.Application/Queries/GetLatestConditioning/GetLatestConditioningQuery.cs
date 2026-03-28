using SignalConditioning.Domain.Abstractions;

using Intercessor.Abstractions;

namespace SignalConditioning.Application.Queries.GetLatestConditioning;

public sealed record GetLatestConditioningQuery(string MeasurementId) : IQuery<ConditioningSummary?>;
