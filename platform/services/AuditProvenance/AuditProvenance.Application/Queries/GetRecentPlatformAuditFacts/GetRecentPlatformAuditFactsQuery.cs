using AuditProvenance.Domain.Abstractions;

using Intercessor.Abstractions;

namespace AuditProvenance.Application.Queries.GetRecentPlatformAuditFacts;

public sealed record GetRecentPlatformAuditFactsQuery(int Count = 50) : IQuery<IReadOnlyList<PlatformAuditFactSummary>>;
