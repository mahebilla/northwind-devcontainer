using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.BasicQueries.Queries;

public record GetProductsAnyQuery() : IRequest<object>;

public class GetProductsAnyQueryHandler : IRequestHandler<GetProductsAnyQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductsAnyQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductsAnyQuery request, CancellationToken ct)
    {
        var hasExpensive = await _readDb.Products.AnyAsync(p => p.UnitPrice > 100, ct);
        return new { Method = "AnyAsync()", Description = "Check if any product costs > $100", Data = hasExpensive };
    }
}
