using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.Pagination.Queries;

public record GetProductsOffsetQuery(int Page, int PageSize) : IRequest<object>;

public class GetProductsOffsetQueryHandler : IRequestHandler<GetProductsOffsetQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductsOffsetQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductsOffsetQuery request, CancellationToken ct)
    {
        var totalCount = await _readDb.Products.CountAsync(ct);
        var products = await _readDb.Products
            .OrderBy(p => p.ProductId)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice, p.CategoryId })
            .ToListAsync(ct);

        return new
        {
            Method = "Skip/Take Offset Pagination",
            Description = "Traditional pagination using OFFSET/FETCH on ProductReadModel — simple but slower for large offsets",
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
            Data = products
        };
    }
}
