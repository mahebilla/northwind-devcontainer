using MediatR;
using Microsoft.AspNetCore.Mvc;
using NorthwindCqrs.Application.Features.BasicQueries.Queries;

namespace NorthwindCqrs.Api.Controllers;

/// <summary>
/// Identical routes to NorthwindApi BasicQueriesController.
/// Controllers are a pure delivery mechanism — all logic lives in MediatR handlers.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class BasicQueriesController : ControllerBase
{
    private readonly IMediator _mediator;
    public BasicQueriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("where")]
    public async Task<IActionResult> WhereExample()
        => Ok(await _mediator.Send(new GetProductsWhereQuery()));

    [HttpGet("find/{id}")]
    public async Task<IActionResult> FindExample(int id)
        => Ok(await _mediator.Send(new GetProductByIdQuery(id)));

    [HttpGet("firstordefault")]
    public async Task<IActionResult> FirstOrDefaultExample()
        => Ok(await _mediator.Send(new GetProductFirstOrDefaultQuery()));

    [HttpGet("singleordefault/{id}")]
    public async Task<IActionResult> SingleOrDefaultExample(int id)
        => Ok(await _mediator.Send(new GetProductSingleOrDefaultQuery(id)));

    [HttpGet("any")]
    public async Task<IActionResult> AnyExample()
        => Ok(await _mediator.Send(new GetProductsAnyQuery()));

    [HttpGet("count")]
    public async Task<IActionResult> CountExample()
        => Ok(await _mediator.Send(new GetProductsCountQuery()));

    [HttpGet("aggregates")]
    public async Task<IActionResult> AggregatesExample()
        => Ok(await _mediator.Send(new GetProductsAggregatesQuery()));

    [HttpGet("orderby")]
    public async Task<IActionResult> OrderByExample()
        => Ok(await _mediator.Send(new GetProductsOrderByQuery()));

    [HttpGet("groupby")]
    public async Task<IActionResult> GroupByExample()
        => Ok(await _mediator.Send(new GetProductsGroupByQuery()));

    [HttpGet("select")]
    public async Task<IActionResult> SelectExample()
        => Ok(await _mediator.Send(new GetProductsSelectQuery()));

    [HttpGet("distinct")]
    public async Task<IActionResult> DistinctExample()
        => Ok(await _mediator.Send(new GetDistinctCustomerCitiesQuery()));

    [HttpGet("contains")]
    public async Task<IActionResult> ContainsExample()
        => Ok(await _mediator.Send(new GetProductsContainsQuery()));

    [HttpGet("all")]
    public async Task<IActionResult> AllExample()
        => Ok(await _mediator.Send(new GetProductsAllQuery()));

    [HttpGet("querysyntax")]
    public async Task<IActionResult> QuerySyntaxExample()
        => Ok(await _mediator.Send(new GetProductsQuerySyntaxQuery()));

    [HttpGet("join")]
    public async Task<IActionResult> JoinExample()
        => Ok(await _mediator.Send(new GetProductsJoinQuery()));

    [HttpGet("tagwith")]
    public async Task<IActionResult> TagWithExample()
        => Ok(await _mediator.Send(new GetProductsTagWithQuery()));
}
