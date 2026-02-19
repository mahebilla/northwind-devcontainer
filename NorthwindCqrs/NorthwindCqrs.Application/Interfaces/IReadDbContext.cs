using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.ReadModels;

namespace NorthwindCqrs.Application.Interfaces;

// Query handlers read through this interface — never touch WriteDbContext.
// SaveChangesAsync is included so command handlers can sync read models after a write.
public interface IReadDbContext
{
    DbSet<ProductReadModel>   Products   { get; }
    DbSet<OrderReadModel>     Orders     { get; }
    DbSet<OrderLineReadModel> OrderLines { get; }
    DbSet<CustomerReadModel>  Customers  { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
