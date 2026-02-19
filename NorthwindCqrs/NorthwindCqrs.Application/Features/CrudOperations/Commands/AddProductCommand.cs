using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;
using NorthwindCqrs.Application.ReadModels;
using NorthwindCqrs.Domain.Entities;

namespace NorthwindCqrs.Application.Features.CrudOperations.Commands;

public record AddProductCommand(string? ProductName, decimal? UnitPrice, int? CategoryId)
    : IRequest<object>;

public class AddProductCommandHandler : IRequestHandler<AddProductCommand, object>
{
    private readonly IWriteDbContext _writeDb;
    private readonly IReadDbContext  _readDb;

    public AddProductCommandHandler(IWriteDbContext writeDb, IReadDbContext readDb)
    {
        _writeDb = writeDb;
        _readDb  = readDb;
    }

    public async Task<object> Handle(AddProductCommand request, CancellationToken ct)
    {
        // ── Step 1: Write to Northwind (normalized Write DB) ──────────────
        var product = new Product
        {
            ProductName  = request.ProductName ?? "EF Core CQRS Demo Product",
            UnitPrice    = request.UnitPrice ?? 9.99m,
            Discontinued = false,
            CategoryId   = request.CategoryId
        };

        _writeDb.Products.Add(product);
        var entriesWritten = await _writeDb.SaveChangesAsync(ct);
        // After SaveChangesAsync, product.ProductId is populated by the DB identity column.

        // ── Step 2: Sync NorthwindRead (denormalized Read DB) ─────────────
        // Look up CategoryName for denormalization — do this lookup from the Write DB
        // since it's already loaded (avoids a round-trip to the Read DB).
        var categoryName = request.CategoryId.HasValue
            ? await _writeDb.Categories
                .Where(c => c.CategoryId == request.CategoryId)
                .Select(c => c.CategoryName)
                .FirstOrDefaultAsync(ct)
            : null;

        _readDb.Products.Add(new ProductReadModel
        {
            ProductId    = product.ProductId,
            ProductName  = product.ProductName,
            UnitPrice    = product.UnitPrice,
            Discontinued = product.Discontinued,
            CategoryId   = product.CategoryId,
            CategoryName = categoryName,
            SupplierId   = product.SupplierId,
            UnitsInStock = product.UnitsInStock
        });
        await _readDb.SaveChangesAsync(ct);

        return new
        {
            Method = "AddProductCommand (CQRS Write Path)",
            Description = "1) INSERT into Northwind.Products (WriteDB), 2) INSERT into NorthwindRead.ProductReadModel (ReadDB)",
            EntriesWritten = entriesWritten,
            GeneratedId = product.ProductId,
            Data = new { product.ProductId, product.ProductName, product.UnitPrice }
        };
    }
}
