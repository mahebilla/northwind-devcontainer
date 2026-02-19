using MediatR;
using Microsoft.AspNetCore.Mvc;
using NorthwindCqrs.Application.Features.CrudOperations.Commands;
using NorthwindCqrs.Application.Features.CrudOperations.Queries;

namespace NorthwindCqrs.Api.Controllers;

/// <summary>
/// Identical routes to NorthwindApi CrudOperationsController.
/// Controllers only dispatch to MediatR — zero EF Core or business logic here.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CrudOperationsController : ControllerBase
{
    private readonly IMediator _mediator;
    public CrudOperationsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("add")]
    public async Task<IActionResult> AddExample([FromBody] AddProductRequest? request)
        => Ok(await _mediator.Send(new AddProductCommand(request?.ProductName, request?.UnitPrice, request?.CategoryId)));

    [HttpPost("addrange")]
    public async Task<IActionResult> AddRangeExample()
        => Ok(await _mediator.Send(new AddProductsRangeCommand()));

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateExample(int id, [FromBody] UpdateProductRequest? request)
        => Ok(await _mediator.Send(new UpdateProductCommand(id, request?.ProductName, request?.UnitPrice)));

    [HttpPut("update-method")]
    public async Task<IActionResult> UpdateMethodExample()
        => Ok(await _mediator.Send(new UpdateProductDisconnectedCommand()));

    [HttpDelete("remove/{id}")]
    public async Task<IActionResult> RemoveExample(int id)
        => Ok(await _mediator.Send(new DeleteProductCommand(id)));

    [HttpPost("attach")]
    public async Task<IActionResult> AttachExample()
        => Ok(await _mediator.Send(new AttachDemoCommand()));

    [HttpGet("products")]
    public async Task<IActionResult> ListProducts()
        => Ok(await _mediator.Send(new GetRecentProductsQuery()));
}

// Input DTOs live in the API layer — they are HTTP contracts, not domain objects
public record AddProductRequest(string? ProductName, decimal? UnitPrice, int? CategoryId);
public record UpdateProductRequest(string? ProductName, decimal? UnitPrice);
