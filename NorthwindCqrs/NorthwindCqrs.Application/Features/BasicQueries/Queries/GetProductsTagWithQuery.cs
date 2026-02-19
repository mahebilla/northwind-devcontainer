using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.BasicQueries.Queries;

public record GetProductsTagWithQuery() : IRequest<object>;

public class GetProductsTagWithQueryHandler : IRequestHandler<GetProductsTagWithQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductsTagWithQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductsTagWithQuery request, CancellationToken ct)
    {
        var products = await _readDb.Products
            .TagWith("NorthwindCqrs.Api: CQRS query handler — list beverages from ReadDB")
            .Where(p => p.CategoryName == "Beverages")
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
            .ToListAsync(ct);

        return new { Method = "TagWith()", Description = "Adds a SQL comment for diagnostics — visible in SQL Server Profiler, applied to NorthwindRead DB", Data = products };
    }
}
