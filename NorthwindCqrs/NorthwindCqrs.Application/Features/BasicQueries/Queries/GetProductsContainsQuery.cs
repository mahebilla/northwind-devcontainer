using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.BasicQueries.Queries;

public record GetProductsContainsQuery() : IRequest<object>;

public class GetProductsContainsQueryHandler : IRequestHandler<GetProductsContainsQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductsContainsQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductsContainsQuery request, CancellationToken ct)
    {
        var categoryIds = new List<int?> { 1, 2, 3 };
        var products = await _readDb.Products
            .Where(p => categoryIds.Contains(p.CategoryId))
            .Select(p => new { p.ProductId, p.ProductName, p.CategoryId, p.CategoryName })
            .ToListAsync(ct);

        return new { Method = "Contains() → SQL IN clause", Description = "Products in categories 1, 2, or 3 — CategoryId retained in read model for filtering", Data = products };
    }
}
