using MediatR;
using Microsoft.AspNetCore.Mvc;
using NorthwindCqrs.Application.Features.Pagination.Queries;

namespace NorthwindCqrs.Api.Controllers;

/// <summary>
/// Identical routes to NorthwindApi PaginationController.
/// Read models eliminate runtime JOINs — see GetOrdersPagedQuery for the key CQRS benefit.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class PaginationController : ControllerBase
{
    private readonly IMediator _mediator;
    public PaginationController(IMediator mediator) => _mediator = mediator;

    [HttpGet("offset")]
    public async Task<IActionResult> OffsetPaginationExample(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _mediator.Send(new GetProductsOffsetQuery(page, pageSize)));

    [HttpGet("keyset")]
    public async Task<IActionResult> KeysetPaginationExample(
        [FromQuery] int lastId = 0, [FromQuery] int pageSize = 10)
        => Ok(await _mediator.Send(new GetProductsKeysetQuery(lastId, pageSize)));

    [HttpGet("orders")]
    public async Task<IActionResult> OrdersPaginationExample(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _mediator.Send(new GetOrdersPagedQuery(page, pageSize)));
}
