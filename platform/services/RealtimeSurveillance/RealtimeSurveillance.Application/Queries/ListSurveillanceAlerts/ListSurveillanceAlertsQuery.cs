using Intercessor.Abstractions;

namespace RealtimeSurveillance.Application.Queries.ListSurveillanceAlerts;

public sealed record ListSurveillanceAlertsQuery(string? TreatmentSessionId) : IQuery<IReadOnlyList<SurveillanceAlertListItemDto>>;
