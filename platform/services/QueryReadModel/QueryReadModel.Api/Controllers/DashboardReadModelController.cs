using Asp.Versioning;

using QueryReadModel.Application.Queries.GetDashboardSummary;

using BuildingBlocks.Authorization;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QueryReadModel.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/dashboard")]
public sealed class DashboardReadModelController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardReadModelController(ISender sender) =>
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));

    [HttpGet("summary")]
    [Authorize(Policy = PlatformAuthorizationPolicies.ReadModelRead)]
    [ProducesResponseType(typeof(DashboardSummaryReadDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryReadDto>> GetSummaryAsync(CancellationToken cancellationToken)
    {
        DashboardSummaryReadDto dto = await _sender
            .SendAsync(new GetDashboardSummaryQuery(), cancellationToken)
            .ConfigureAwait(false);
        return Ok(dto);
    }
}
