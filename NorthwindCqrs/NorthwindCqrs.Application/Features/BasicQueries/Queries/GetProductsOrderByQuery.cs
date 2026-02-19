using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.BasicQueries.Queries;

public record GetProductsOrderByQuery() : IRequest<object>;

public class GetProductsOrderByQueryHandler : IRequestHandler<GetProductsOrderByQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductsOrderByQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductsOrderByQuery request, CancellationToken ct)
    {
        var products = await _readDb.Products
            .OrderByDescending(p => p.UnitPrice)
            .ThenBy(p => p.ProductName)
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
            .Take(10)
            .ToListAsync(ct);

        return new { Method = "OrderByDescending().ThenBy().Take()", Description = "Top 10 most expensive products", Data = products };
    }
}
