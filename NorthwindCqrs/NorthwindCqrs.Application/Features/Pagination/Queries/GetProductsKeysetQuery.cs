using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.Pagination.Queries;

public record GetProductsKeysetQuery(int LastId, int PageSize) : IRequest<object>;

public class GetProductsKeysetQueryHandler : IRequestHandler<GetProductsKeysetQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductsKeysetQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductsKeysetQuery request, CancellationToken ct)
    {
        var products = await _readDb.Products
            .OrderBy(p => p.ProductId)
            .Where(p => p.ProductId > request.LastId)
            .Take(request.PageSize)
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
            .ToListAsync(ct);

        var nextCursor = products.LastOrDefault()?.ProductId ?? request.LastId;

        return new
        {
            Method = "Keyset (Cursor-Based) Pagination",
            Description = "Uses WHERE ProductId > @lastId — constant performance regardless of page depth",
            LastId = request.LastId,
            PageSize = request.PageSize,
            NextCursor = nextCursor,
            HasMore = products.Count == request.PageSize,
            Data = products
        };
    }
}
