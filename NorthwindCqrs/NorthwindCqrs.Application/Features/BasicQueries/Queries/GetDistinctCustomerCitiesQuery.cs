using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.BasicQueries.Queries;

public record GetDistinctCustomerCitiesQuery() : IRequest<object>;

public class GetDistinctCustomerCitiesQueryHandler : IRequestHandler<GetDistinctCustomerCitiesQuery, object>
{
    private readonly IReadDbContext _readDb;
    public GetDistinctCustomerCitiesQueryHandler(IReadDbContext readDb) => _readDb = readDb;

    public async Task<object> Handle(GetDistinctCustomerCitiesQuery request, CancellationToken ct)
    {
        var cities = await _readDb.Customers
            .Select(c => c.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);

        return new { Method = "Distinct()", Description = "Get distinct customer cities from CustomerReadModel", Count = cities.Count, Data = cities };
    }
}
