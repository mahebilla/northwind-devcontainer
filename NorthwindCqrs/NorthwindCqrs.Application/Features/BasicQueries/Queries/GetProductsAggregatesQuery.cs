using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.BasicQueries.Queries;

public record GetProductsAggregatesQuery() : IRequest<object>;

public class GetProductsAggregatesQueryHandler : IRequestHandler<GetProductsAggregatesQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductsAggregatesQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductsAggregatesQuery request, CancellationToken ct)
    {
        var avg = await _readDb.Products.AverageAsync(p => p.UnitPrice, ct);
        var max = await _readDb.Products.MaxAsync(p => p.UnitPrice, ct);
        var min = await _readDb.Products.Where(p => p.UnitPrice > 0).MinAsync(p => p.UnitPrice, ct);
        var totalStock = await _readDb.Products.SumAsync(p => (int)(p.UnitsInStock ?? 0), ct);

        return new
        {
            Method = "AverageAsync / MaxAsync / MinAsync / SumAsync",
            Description = "Aggregate functions on ProductReadModel in NorthwindRead DB",
            AveragePrice = avg,
            MaxPrice = max,
            MinPrice = min,
            TotalUnitsInStock = totalStock
        };
    }
}
