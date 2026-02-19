using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.BasicQueries.Queries;

public record GetProductSingleOrDefaultQuery(int Id) : IRequest<object>;

public class GetProductSingleOrDefaultQueryHandler : IRequestHandler<GetProductSingleOrDefaultQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductSingleOrDefaultQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductSingleOrDefaultQuery request, CancellationToken ct)
    {
        var product = await _readDb.Products
            .SingleOrDefaultAsync(p => p.ProductId == request.Id, ct);

        return new
        {
            Method = "SingleOrDefaultAsync()",
            Description = "Expects exactly one result — throws if more than one match",
            Data = product == null ? null : new { product.ProductId, product.ProductName, product.UnitPrice }
        };
    }
}
