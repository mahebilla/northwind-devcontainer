using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.BasicQueries.Queries;

public record GetProductByIdQuery(int Id) : IRequest<object>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductByIdQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        // ReadDbContext uses NoTracking — no identity cache, so use FirstOrDefaultAsync instead of FindAsync
        var product = await _readDb.Products
            .FirstOrDefaultAsync(p => p.ProductId == request.Id, ct);

        return new
        {
            Method = "FirstOrDefaultAsync() by PK (CQRS Read Model)",
            Description = "Look up product by primary key from the denormalized ReadDB",
            Data = product == null ? null : new { product.ProductId, product.ProductName, product.UnitPrice }
        };
    }
}
