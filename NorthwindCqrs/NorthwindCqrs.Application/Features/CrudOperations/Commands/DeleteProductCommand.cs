using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.CrudOperations.Commands;

public record DeleteProductCommand(int Id) : IRequest<object>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, object>
{
    private readonly IWriteDbContext _writeDb;
    private readonly IReadDbContext  _readDb;

    public DeleteProductCommandHandler(IWriteDbContext writeDb, IReadDbContext readDb)
    {
        _writeDb = writeDb;
        _readDb  = readDb;
    }

    public async Task<object> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await _writeDb.Products.FirstOrDefaultAsync(p => p.ProductId == request.Id, ct);
        if (product == null)
            return new { Error = $"Product {request.Id} not found" };

        var hasOrders = await _writeDb.OrderDetails.AnyAsync(od => od.ProductId == request.Id, ct);
        if (hasOrders)
            return new { Error = $"Product {request.Id} has related orders and cannot be deleted" };

        _writeDb.Products.Remove(product);
        await _writeDb.SaveChangesAsync(ct);

        // Sync Read DB — remove the read model projection too
        var readModel = await _readDb.Products.FirstOrDefaultAsync(p => p.ProductId == request.Id, ct);
        if (readModel != null)
        {
            _readDb.Products.Remove(readModel);
            await _readDb.SaveChangesAsync(ct);
        }

        return new
        {
            Method = "Remove() + SaveChangesAsync() + ReadDB sync",
            Description = "Delete from WriteDB then remove the read model projection from NorthwindRead",
            DeletedProduct = new { product.ProductId, product.ProductName }
        };
    }
}
