using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.BasicQueries.Queries;

public record GetProductsWhereQuery() : IRequest<object>;

public class GetProductsWhereQueryHandler : IRequestHandler<GetProductsWhereQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductsWhereQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductsWhereQuery request, CancellationToken ct)
    {
        var products = await _readDb.Products
            .Where(p => p.UnitPrice > 20)
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
            .ToListAsync(ct);

        return new { Method = "Where()", Description = "Filter products with UnitPrice > 20 (from ReadDB — no join needed)", Data = products };
    }
}
