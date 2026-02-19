using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.BasicQueries.Queries;

public record GetProductsQuerySyntaxQuery() : IRequest<object>;

public class GetProductsQuerySyntaxQueryHandler : IRequestHandler<GetProductsQuerySyntaxQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductsQuerySyntaxQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductsQuerySyntaxQuery request, CancellationToken ct)
    {
        var products = await (
            from p in _readDb.Products
            where p.UnitPrice > 20
            orderby p.ProductName
            select new { p.ProductId, p.ProductName, p.UnitPrice }
        ).ToListAsync(ct);

        return new { Method = "LINQ Query Syntax", Description = "Same as method syntax but using from/where/select keywords — works identically on ReadDB", Data = products };
    }
}
