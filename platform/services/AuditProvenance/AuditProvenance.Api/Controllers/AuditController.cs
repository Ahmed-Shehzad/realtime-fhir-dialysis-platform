using System.Security.Claims;

using Asp.Versioning;

using AuditProvenance.Application.Commands.RecordPlatformAuditFact;
using AuditProvenance.Application.Commands.RecordProvenanceLink;
using AuditProvenance.Application.Queries.GetRecentPlatformAuditFacts;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditProvenance.Api.Controllers;

/// <summary>Central audit facts and provenance links (v1).</summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/audit")]
public sealed class AuditController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public AuditController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost("facts")]
    [Authorize(Policy = PlatformAuthorizationPolicies.AuditWrite)]
    [ProducesResponseType(typeof(RecordPlatformAuditFactResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<RecordPlatformAuditFactResponse>> RecordFactAsync(
        [FromBody] RecordPlatformAuditFactRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        RecordPlatformAuditFactResult result = await _sender
            .SendAsync(
                new RecordPlatformAuditFactCommand(
                    correlationId,
                    request.OccurredAtUtc,
                    request.EventType,
                    request.Summary,
                    request.DetailJson,
                    request.CorrelationId,
                    request.CausationId,
                    request.ActorId,
                    request.SourceSystem,
                    request.RelatedResourceType,
                    request.RelatedResourceId,
                    request.SessionId,
                    request.PatientId,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return StatusCode(
            StatusCodes.Status201Created,
            new RecordPlatformAuditFactResponse(result.PlatformAuditFactId.ToString()));
    }

    [HttpPost("provenance-links")]
    [Authorize(Policy = PlatformAuthorizationPolicies.AuditWrite)]
    [ProducesResponseType(typeof(RecordProvenanceLinkResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<RecordProvenanceLinkResponse>> RecordProvenanceLinkAsync(
        [FromBody] RecordProvenanceLinkRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryParseUlid(request.FromPlatformAuditFactId, out Ulid fromId)
            || !TryParseUlid(request.ToPlatformAuditFactId, out Ulid toId))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        try
        {
            RecordProvenanceLinkResult result = await _sender
                .SendAsync(
                    new RecordProvenanceLinkCommand(correlationId, fromId, toId, request.RelationType, principalId),
                    cancellationToken)
                .ConfigureAwait(false);
            return StatusCode(
                StatusCodes.Status201Created,
                new RecordProvenanceLinkResponse(result.ProvenanceLinkId.ToString()));
        }
        catch (InvalidOperationException ex) when (IsNotFoundMessage(ex.Message))
        {
            return NotFound();
        }
    }

    [HttpGet("facts/recent")]
    [Authorize(Policy = PlatformAuthorizationPolicies.AuditRead)]
    [ProducesResponseType(typeof(IReadOnlyList<PlatformAuditFactResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlatformAuditFactResponse>>> GetRecentFactsAsync(
        [FromQuery(Name = "count")] int count = 50,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AuditProvenance.Domain.Abstractions.PlatformAuditFactSummary> rows = await _sender
            .SendAsync(new GetRecentPlatformAuditFactsQuery(count), cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<PlatformAuditFactResponse> mapped = rows
            .Select(s => new PlatformAuditFactResponse(
                s.Id.ToString(),
                s.OccurredAtUtc,
                s.EventType,
                s.Summary,
                s.SourceSystem))
            .ToList();
        return Ok(mapped);
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

    private static bool TryParseUlid(string value, out Ulid id)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            id = default;
            return false;
        }

        try
        {
            id = Ulid.Parse(value.Trim());
            return true;
        }
        catch (FormatException)
        {
            id = default;
            return false;
        }
    }

    private static bool IsNotFoundMessage(string message) =>
        message.Contains("was not found", StringComparison.Ordinal);
}

public sealed record RecordPlatformAuditFactRequest(
    DateTimeOffset? OccurredAtUtc,
    string EventType,
    string Summary,
    string? DetailJson,
    string? CorrelationId,
    string? CausationId,
    string? ActorId,
    string SourceSystem,
    string? RelatedResourceType,
    string? RelatedResourceId,
    string? SessionId,
    string? PatientId);

public sealed record RecordPlatformAuditFactResponse(string PlatformAuditFactId);

public sealed record RecordProvenanceLinkRequest(
    string FromPlatformAuditFactId,
    string ToPlatformAuditFactId,
    string RelationType);

public sealed record RecordProvenanceLinkResponse(string ProvenanceLinkId);

public sealed record PlatformAuditFactResponse(
    string Id,
    DateTimeOffset OccurredAtUtc,
    string EventType,
    string Summary,
    string SourceSystem);
