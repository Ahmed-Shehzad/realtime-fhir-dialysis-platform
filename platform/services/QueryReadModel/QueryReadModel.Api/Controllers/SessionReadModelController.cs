using Asp.Versioning;

using QueryReadModel.Application.Queries.GetSessionOverview;

using BuildingBlocks.Authorization;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QueryReadModel.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/sessions")]
public sealed class SessionReadModelController : ControllerBase
{
    private readonly ISender _sender;

    public SessionReadModelController(ISender sender) =>
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));

    [HttpGet("{treatmentSessionId}/overview")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReadModelRead)]
    [ProducesResponseType(typeof(SessionOverviewReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionOverviewReadDto>> GetOverviewAsync(
        string treatmentSessionId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(treatmentSessionId);
        SessionOverviewReadDto? row = await _sender
            .SendAsync(new GetSessionOverviewQuery(treatmentSessionId.Trim()), cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return NotFound();
        return Ok(row);
    }
}
