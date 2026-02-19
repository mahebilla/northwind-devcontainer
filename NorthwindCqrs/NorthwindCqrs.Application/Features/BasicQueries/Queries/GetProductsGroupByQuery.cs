using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.BasicQueries.Queries;

public record GetProductsGroupByQuery() : IRequest<object>;

public class GetProductsGroupByQueryHandler : IRequestHandler<GetProductsGroupByQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductsGroupByQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductsGroupByQuery request, CancellationToken ct)
    {
        // CQRS advantage: CategoryName is pre-denormalized — group by name directly.
        // The original NorthwindApi groups by CategoryId and returns an int key.
        // Here we group by the human-readable name with zero joins.
        var grouped = await _readDb.Products
            .GroupBy(p => p.CategoryName)
            .Select(g => new { CategoryName = g.Key, ProductCount = g.Count(), AvgPrice = g.Average(p => p.UnitPrice) })
            .ToListAsync(ct);

        return new { Method = "GroupBy() on denormalized CategoryName", Description = "No JOIN needed — CategoryName pre-joined in read model", Data = grouped };
    }
}
