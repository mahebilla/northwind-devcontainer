using MediatR;
using NorthwindCqrs.Application.Interfaces;
using NorthwindCqrs.Domain.Entities;

namespace NorthwindCqrs.Application.Features.CrudOperations.Commands;

public record AttachDemoCommand() : IRequest<object>;

public class AttachDemoCommandHandler : IRequestHandler<AttachDemoCommand, object>
{
    private readonly IWriteDbContext _writeDb;

    public AttachDemoCommandHandler(IWriteDbContext writeDb) => _writeDb = writeDb;

    public async Task<object> Handle(AttachDemoCommand request, CancellationToken ct)
    {
        // Demonstrate EntityState transitions without actually persisting.
        // We use the injected WriteDbContext directly for this state demo.
        var product = new Product { ProductId = 1, ProductName = "Chai" };
        _writeDb.Products.Attach(product);
        var stateAfterAttach = _writeDb.Entry(product).State;  // Unchanged

        product.UnitPrice = 99.99m;
        var stateAfterModify = _writeDb.Entry(product).State;  // Modified

        // No SaveChangesAsync — demonstration only
        return await Task.FromResult<object>(new
        {
            Method = "Attach() + EntityState",
            Description = "Attach a disconnected entity (Unchanged state), then modify it (changes to Modified). No save — demo only.",
            StateAfterAttach = stateAfterAttach.ToString(),
            StateAfterModify = stateAfterModify.ToString(),
            Note = "Changes NOT saved — demonstration only"
        });
    }
}
