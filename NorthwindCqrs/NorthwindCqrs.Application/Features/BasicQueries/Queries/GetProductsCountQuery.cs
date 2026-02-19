using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.BasicQueries.Queries;

public record GetProductsCountQuery() : IRequest<object>;

public class GetProductsCountQueryHandler : IRequestHandler<GetProductsCountQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductsCountQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductsCountQuery request, CancellationToken ct)
    {
        var total = await _readDb.Products.CountAsync(ct);
        var discontinued = await _readDb.Products.CountAsync(p => p.Discontinued, ct);
        return new { Method = "CountAsync()", Description = "Count all products and discontinued products", TotalProducts = total, DiscontinuedProducts = discontinued };
    }
}
