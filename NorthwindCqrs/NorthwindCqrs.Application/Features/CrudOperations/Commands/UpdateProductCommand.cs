using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.CrudOperations.Commands;

public record UpdateProductCommand(int Id, string? ProductName, decimal? UnitPrice)
    : IRequest<object>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, object>
{
    private readonly IWriteDbContext _writeDb;
    private readonly IReadDbContext  _readDb;

    public UpdateProductCommandHandler(IWriteDbContext writeDb, IReadDbContext readDb)
    {
        _writeDb = writeDb;
        _readDb  = readDb;
    }

    public async Task<object> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        // Update Write DB (tracked entity — EF generates minimal UPDATE)
        var product = await _writeDb.Products.FirstOrDefaultAsync(p => p.ProductId == request.Id, ct);
        if (product == null)
            return new { Error = $"Product {request.Id} not found" };

        var originalPrice = product.UnitPrice;
        product.UnitPrice = request.UnitPrice ?? (originalPrice ?? 0) + 1;
        if (request.ProductName != null) product.ProductName = request.ProductName;

        var entriesWritten = await _writeDb.SaveChangesAsync(ct);

        // Sync Read DB — find and update the read model projection
        var readModel = await _readDb.Products.FirstOrDefaultAsync(p => p.ProductId == request.Id, ct);
        if (readModel != null)
        {
            readModel.UnitPrice   = product.UnitPrice;
            readModel.ProductName = product.ProductName;
            _readDb.Products.Update(readModel);
            await _readDb.SaveChangesAsync(ct);
        }

        return new
        {
            Method = "Tracked Update + SaveChangesAsync() + ReadDB sync",
            Description = "Update WriteDB with minimal SQL UPDATE, then sync the read model in NorthwindRead",
            EntriesWritten = entriesWritten,
            OriginalPrice = originalPrice,
            NewPrice = product.UnitPrice,
            Data = new { product.ProductId, product.ProductName, product.UnitPrice }
        };
    }
}
