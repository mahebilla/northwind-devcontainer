using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.BasicQueries.Queries;

public record GetProductFirstOrDefaultQuery() : IRequest<object>;

public class GetProductFirstOrDefaultQueryHandler : IRequestHandler<GetProductFirstOrDefaultQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductFirstOrDefaultQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductFirstOrDefaultQuery request, CancellationToken ct)
    {
        // CategoryName is already denormalized in the read model — no join required
        var product = await _readDb.Products
            .Where(p => p.CategoryName == "Beverages")
            .FirstOrDefaultAsync(ct);

        return new
        {
            Method = "FirstOrDefaultAsync()",
            Description = "Get the first Beverage product — CategoryName pre-joined in read model, no SQL JOIN needed",
            Data = product == null ? null : new { product.ProductId, product.ProductName, product.UnitPrice, product.CategoryName }
        };
    }
}
