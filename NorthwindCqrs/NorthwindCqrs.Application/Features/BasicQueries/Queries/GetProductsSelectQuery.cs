using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.BasicQueries.Queries;

public record GetProductsSelectQuery() : IRequest<object>;

public class GetProductsSelectQueryHandler : IRequestHandler<GetProductsSelectQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductsSelectQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductsSelectQuery request, CancellationToken ct)
    {
        var projected = await _readDb.Products
            .Select(p => new
            {
                p.ProductName,
                p.UnitPrice,
                IsExpensive = p.UnitPrice > 50,
                StockStatus = p.UnitsInStock > 0 ? "In Stock" : "Out of Stock"
            })
            .Take(15)
            .ToListAsync(ct);

        return new { Method = "Select() Projection", Description = "Project read model into anonymous type with computed properties", Data = projected };
    }
}
