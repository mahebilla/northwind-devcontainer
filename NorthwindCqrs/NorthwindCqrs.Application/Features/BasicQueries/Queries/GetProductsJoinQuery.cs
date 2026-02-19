using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.BasicQueries.Queries;

public record GetProductsJoinQuery() : IRequest<object>;

public class GetProductsJoinQueryHandler : IRequestHandler<GetProductsJoinQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetProductsJoinQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetProductsJoinQuery request, CancellationToken ct)
    {
        // ── CQRS Read Model Key Advantage ──────────────────────────────────────
        // The original NorthwindApi runs a LINQ Join between Products and Categories
        // tables on every call — two tables, one JOIN in SQL.
        //
        // Here, CategoryName is already denormalized into ProductReadModel.
        // The read model was populated with the join pre-computed at write time.
        // At query time: zero joins, one table, faster reads at scale.
        // ───────────────────────────────────────────────────────────────────────
        var result = await _readDb.Products
            .OrderBy(p => p.CategoryName)
            .ThenBy(p => p.ProductName)
            .Select(p => new { p.ProductName, p.CategoryName, p.UnitPrice })
            .ToListAsync(ct);

        return new
        {
            Method = "No JOIN needed — CategoryName pre-denormalized in read model",
            Description = "CQRS advantage: the join was done at write time, not at read time. Zero SQL JOINs at query time.",
            Data = result
        };
    }
}
