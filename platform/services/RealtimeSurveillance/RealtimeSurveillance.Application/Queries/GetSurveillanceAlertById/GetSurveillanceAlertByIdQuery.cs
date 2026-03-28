using Intercessor.Abstractions;

namespace RealtimeSurveillance.Application.Queries.GetSurveillanceAlertById;

public sealed record GetSurveillanceAlertByIdQuery(Ulid AlertId) : IQuery<SurveillanceAlertReadDto?>;
