using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.Pagination.Queries;

public record GetOrdersPagedQuery(int Page, int PageSize) : IRequest<object>;

public class GetOrdersPagedQueryHandler : IRequestHandler<GetOrdersPagedQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetOrdersPagedQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetOrdersPagedQuery request, CancellationToken ct)
    {
        var totalCount = await _readDb.Orders.CountAsync(ct);
        var orders = await _readDb.Orders
            .OrderByDescending(o => o.OrderDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new
            {
                o.OrderId,
                o.OrderDate,
                CustomerName = o.CustomerName,  // pre-joined in read model — no Include() needed
                o.Freight,
                o.ShipCountry
            })
            .ToListAsync(ct);

        return new
        {
            Method = "Pagination with pre-joined CustomerName",
            Description = "CQRS advantage: CustomerName is already in OrderReadModel — no Include() or JOIN needed at query time",
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
            Data = orders
        };
    }
}
