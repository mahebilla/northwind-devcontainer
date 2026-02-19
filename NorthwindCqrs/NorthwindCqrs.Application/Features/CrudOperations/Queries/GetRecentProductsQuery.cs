using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.CrudOperations.Queries;

public record GetRecentProductsQuery() : IRequest<object>;

public class GetRecentProductsQueryHandler : IRequestHandler<GetRecentProductsQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetRecentProductsQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetRecentProductsQuery request, CancellationToken ct)
    {
        var products = await _readDb.Products
            .OrderByDescending(p => p.ProductId)
            .Take(20)
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice, p.Discontinued })
            .ToListAsync(ct);

        return new { Method = "List Products (from ReadDB)", Description = "View latest 20 products from NorthwindRead — verify CRUD operations are syncing correctly", Data = products };
    }
}
