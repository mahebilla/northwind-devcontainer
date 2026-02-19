using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;

namespace NorthwindCqrs.Application.Features.CrudOperations.Commands;

public record UpdateProductDisconnectedCommand() : IRequest<object>;

public class UpdateProductDisconnectedCommandHandler : IRequestHandler<UpdateProductDisconnectedCommand, object>
{
    private readonly IWriteDbContext _writeDb;
    private readonly IReadDbContext  _readDb;

    public UpdateProductDisconnectedCommandHandler(IWriteDbContext writeDb, IReadDbContext readDb)
    {
        _writeDb = writeDb;
        _readDb  = readDb;
    }

    public async Task<object> Handle(UpdateProductDisconnectedCommand request, CancellationToken ct)
    {
        // Simulate disconnected entity scenario (AsNoTracking, then DbSet.Update)
        var product = await _writeDb.Products.AsNoTracking().FirstAsync(ct);
        var originalName = product.ProductName;
        product.ProductName = originalName + " (Updated)";

        // DbSet.Update() attaches the entity and marks ALL properties Modified
        _writeDb.Products.Update(product);
        var entriesWritten = await _writeDb.SaveChangesAsync(ct);

        // Sync Read DB
        var readModel = await _readDb.Products.FirstOrDefaultAsync(p => p.ProductId == product.ProductId, ct);
        if (readModel != null)
        {
            readModel.ProductName = product.ProductName;
            _readDb.Products.Update(readModel);
            await _readDb.SaveChangesAsync(ct);
        }

        return new
        {
            Method = "DbSet.Update() disconnected entity + ReadDB sync",
            Description = "Attach a disconnected entity and mark ALL properties as modified (full-entity update), then sync read model",
            EntriesWritten = entriesWritten,
            Data = new { product.ProductId, OriginalName = originalName, NewName = product.ProductName }
        };
    }
}
