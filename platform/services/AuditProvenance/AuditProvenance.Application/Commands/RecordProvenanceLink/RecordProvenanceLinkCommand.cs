using Intercessor.Abstractions;

namespace AuditProvenance.Application.Commands.RecordProvenanceLink;

public sealed record RecordProvenanceLinkCommand(
    Ulid CorrelationId,
    Ulid FromPlatformAuditFactId,
    Ulid ToPlatformAuditFactId,
    string RelationType,
    string? AuthenticatedUserId = null) : ICommand<RecordProvenanceLinkResult>;
