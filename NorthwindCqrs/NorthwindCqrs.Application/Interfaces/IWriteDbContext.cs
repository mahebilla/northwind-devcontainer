using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NorthwindCqrs.Domain.Entities;

namespace NorthwindCqrs.Application.Interfaces;

// Dependency Inversion: Application defines what it needs from persistence.
// Infrastructure's WriteDbContext implements this interface.
// Command handlers depend on this interface — never on the concrete class.
public interface IWriteDbContext
{
    DbSet<Product>     Products     { get; }
    DbSet<Category>    Categories   { get; }
    DbSet<Customer>    Customers    { get; }
    DbSet<Order>       Orders       { get; }
    DbSet<OrderDetail> OrderDetails { get; }
    DbSet<Employee>    Employees    { get; }
    DbSet<Shipper>     Shippers     { get; }

    // Exposed for the Attach demo — allows reading EntityState through the interface
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
