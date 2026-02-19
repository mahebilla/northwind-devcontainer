using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.BasicQueries.Queries;

public record GetProductsAllQuery() : IRequest<object>;

public class GetProductsAllQueryHandler : IRequestHandler<GetProductsAllQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductsAllQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductsAllQuery request, CancellationToken ct)
    {
        var allInStock = await _readDb.Products.AllAsync(p => p.UnitsInStock > 0, ct);
        return new { Method = "AllAsync()", Description = "Check if ALL products are in stock", AllProductsInStock = allInStock };
    }
}
