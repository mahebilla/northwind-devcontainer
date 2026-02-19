using MediatR;
using NorthwindCqrs.Application.Interfaces;
using NorthwindCqrs.Application.ReadModels;
using NorthwindCqrs.Domain.Entities;

namespace NorthwindCqrs.Application.Features.CrudOperations.Commands;

public record AddProductsRangeCommand() : IRequest<object>;

public class AddProductsRangeCommandHandler : IRequestHandler<AddProductsRangeCommand, object>
{
    private readonly IWriteDbContext _writeDb;
    private readonly IReadDbContext  _readDb;

    public AddProductsRangeCommandHandler(IWriteDbContext writeDb, IReadDbContext readDb)
    {
        _writeDb = writeDb;
        _readDb  = readDb;
    }

    public async Task<object> Handle(AddProductsRangeCommand request, CancellationToken ct)
    {
        var products = new[]
        {
            new Product { ProductName = "CQRS Batch Product 1", UnitPrice = 10m, Discontinued = false },
            new Product { ProductName = "CQRS Batch Product 2", UnitPrice = 20m, Discontinued = false },
            new Product { ProductName = "CQRS Batch Product 3", UnitPrice = 30m, Discontinued = false }
        };

        await _writeDb.Products.AddRangeAsync(products, ct);
        var entriesWritten = await _writeDb.SaveChangesAsync(ct);

        // Sync all three to the Read DB
        var readModels = products.Select(p => new ProductReadModel
        {
            ProductId    = p.ProductId,
            ProductName  = p.ProductName,
            UnitPrice    = p.UnitPrice,
            Discontinued = p.Discontinued
        }).ToList();

        await _readDb.Products.AddRangeAsync(readModels, ct);
        await _readDb.SaveChangesAsync(ct);

        return new
        {
            Method = "AddRangeAsync() + SaveChangesAsync()",
            Description = "Add 3 products to WriteDB in one batch INSERT, then sync all 3 to ReadDB",
            EntriesWritten = entriesWritten,
            Data = products.Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
        };
    }
}
