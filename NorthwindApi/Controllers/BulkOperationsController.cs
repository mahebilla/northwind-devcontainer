using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApi.Data;
using NorthwindApi.Models;

namespace NorthwindApi.Controllers;

/// <summary>
/// Demonstrates: ExecuteUpdate, ExecuteDelete (EF Core 7+ set-based operations)
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class BulkOperationsController : ControllerBase
{
    private readonly NorthwindContext _context;

    public BulkOperationsController(NorthwindContext context) => _context = context;

    // POST api/bulkoperations/executeupdate
    [HttpPost("executeupdate")]
    public async Task<IActionResult> ExecuteUpdateExample()
    {
        // Increase price by 10% for Category 1 (Beverages)
        var rowsAffected = await _context.Products
            .IgnoreQueryFilters()
            .Where(p => p.CategoryId == 1)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.UnitPrice, p => p.UnitPrice * 1.1m));

        return Ok(new
        {
            Method = "ExecuteUpdateAsync()",
            Description = "Bulk update directly in SQL without loading entities into memory",
            RowsAffected = rowsAffected,
            Operation = "Increased price by 10% for all Beverages (Category 1)"
        });
    }

    // POST api/bulkoperations/executedelete
    [HttpPost("executedelete")]
    public async Task<IActionResult> ExecuteDeleteExample()
    {
        // First add temp data to delete
        _context.Products.AddRange(
            new Product { ProductName = "BulkDeleteTest1", Discontinued = true, UnitPrice = 0 },
            new Product { ProductName = "BulkDeleteTest2", Discontinued = true, UnitPrice = 0 }
        );
        await _context.SaveChangesAsync();

        // Bulk delete
        var rowsDeleted = await _context.Products
            .IgnoreQueryFilters()
            .Where(p => p.ProductName.StartsWith("BulkDeleteTest"))
            .ExecuteDeleteAsync();

        return Ok(new
        {
            Method = "ExecuteDeleteAsync()",
            Description = "Bulk delete directly in SQL — no entity loading, no change tracking needed",
            RowsDeleted = rowsDeleted,
            Operation = "Deleted temporary test products"
        });
    }

    // POST api/bulkoperations/executeupdate-multiple
    [HttpPost("executeupdate-multiple")]
    public async Task<IActionResult> ExecuteUpdateMultipleExample()
    {
        // Set all discontinued products: price=0, stock=0
        var rowsAffected = await _context.Products
            .IgnoreQueryFilters()
            .Where(p => p.Discontinued)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.UnitPrice, 0m)
                .SetProperty(p => p.UnitsInStock, (short)0));

        return Ok(new
        {
            Method = "ExecuteUpdateAsync() — multiple SetProperty",
            Description = "Update multiple columns in a single bulk operation",
            RowsAffected = rowsAffected,
            Operation = "Set price=0, stock=0 for all discontinued products"
        });
    }
}
