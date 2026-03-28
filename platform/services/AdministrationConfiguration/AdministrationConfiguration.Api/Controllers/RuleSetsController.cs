using System.Security.Claims;

using Asp.Versioning;

using AdministrationConfiguration.Application.Commands.CreateRuleSetDraft;
using AdministrationConfiguration.Application.Commands.PublishRuleSet;
using AdministrationConfiguration.Application.Queries.GetRuleSetById;

using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdministrationConfiguration.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/administration-configuration/rule-sets")]
public sealed class RuleSetsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICorrelationIdAccessor _correlation;

    public RuleSetsController(ISender sender, ICorrelationIdAccessor correlation)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
    }

    [HttpPost]
    [Authorize(Policy = PlatformAuthorizationPolicies.ConfigurationWrite)]
    [ProducesResponseType(typeof(CreateRuleSetDraftResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateRuleSetDraftResponse>> CreateDraftAsync(
        [FromBody] CreateRuleSetDraftRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        Ulid id = await _sender
            .SendAsync(
                new CreateRuleSetDraftCommand(
                    correlationId,
                    request.RuleVersion.Trim(),
                    request.RulesDocument,
                    principalId),
                cancellationToken)
            .ConfigureAwait(false);
        return Ok(new CreateRuleSetDraftResponse(id.ToString()));
    }

    [HttpPost("{ruleSetId}/publish")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ConfigurationWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PublishAsync(string ruleSetId, CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(ruleSetId.Trim(), out Ulid id))
            return BadRequest();
        Ulid correlationId = _correlation.GetOrCreate();
        string? principalId = GetPrincipalObjectId();
        await _sender
            .SendAsync(new PublishRuleSetCommand(correlationId, id, principalId), cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpGet("{ruleSetId}")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ConfigurationRead)]
    [ProducesResponseType(typeof(RuleSetReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RuleSetReadDto>> GetByIdAsync(
        string ruleSetId,
        CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(ruleSetId.Trim(), out Ulid id))
            return BadRequest();
        RuleSetReadDto? row = await _sender.SendAsync(new GetRuleSetByIdQuery(id), cancellationToken).ConfigureAwait(false);
        if (row is null)
            return NotFound();
        return Ok(row);
    }

    private string? GetPrincipalObjectId() =>
        User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public sealed record CreateRuleSetDraftRequest(string RuleVersion, string RulesDocument);

public sealed record CreateRuleSetDraftResponse(string RuleSetId);
