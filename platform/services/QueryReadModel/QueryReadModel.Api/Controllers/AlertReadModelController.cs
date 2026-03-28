using Asp.Versioning;

using QueryReadModel.Application.Queries.ListAlertProjections;

using BuildingBlocks.Authorization;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QueryReadModel.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/alerts")]
public sealed class AlertReadModelController : ControllerBase
{
    private readonly ISender _sender;

    public AlertReadModelController(ISender sender) =>
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));

    [HttpGet]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReadModelRead)]
    [ProducesResponseType(typeof(IReadOnlyList<AlertProjectionReadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AlertProjectionReadDto>>> ListAsync(
        [FromQuery] string? severity,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AlertProjectionReadDto> rows = await _sender
            .SendAsync(new ListAlertProjectionsQuery(severity), cancellationToken)
            .ConfigureAwait(false);
        return Ok(rows);
    }
}
