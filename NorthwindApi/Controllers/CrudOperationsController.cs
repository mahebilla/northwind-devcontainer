using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApi.Data;
using NorthwindApi.Models;

namespace NorthwindApi.Controllers;

/// <summary>
/// Demonstrates: Add, AddRange, tracked Update, DbSet.Update (disconnected),
/// Remove, Attach + EntityState. Changes are actually persisted.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CrudOperationsController : ControllerBase
{
    private readonly NorthwindContext _context;

    public CrudOperationsController(NorthwindContext context) => _context = context;

    // POST api/crudoperations/add
    [HttpPost("add")]
    public async Task<IActionResult> AddExample([FromBody] AddProductRequest? request)
    {
        var newProduct = new Product
        {
            ProductName = request?.ProductName ?? "EF Core Demo Product",
            UnitPrice = request?.UnitPrice ?? 9.99m,
            Discontinued = false,
            CategoryId = request?.CategoryId
        };

        _context.Products.Add(newProduct);
        var entriesWritten = await _context.SaveChangesAsync();

        return Ok(new
        {
            Method = "Add() + SaveChangesAsync()",
            Description = "Add a new entity to the DbSet — EF generates INSERT SQL on SaveChanges",
            EntriesWritten = entriesWritten,
            GeneratedId = newProduct.ProductId,
            Data = new { newProduct.ProductId, newProduct.ProductName, newProduct.UnitPrice }
        });
    }

    // POST api/crudoperations/addrange
    [HttpPost("addrange")]
    public async Task<IActionResult> AddRangeExample()
    {
        var products = new[]
        {
            new Product { ProductName = "Demo Batch Product 1", UnitPrice = 10m, Discontinued = false },
            new Product { ProductName = "Demo Batch Product 2", UnitPrice = 20m, Discontinued = false },
            new Product { ProductName = "Demo Batch Product 3", UnitPrice = 30m, Discontinued = false }
        };

        await _context.Products.AddRangeAsync(products);
        var entriesWritten = await _context.SaveChangesAsync();

        return Ok(new
        {
            Method = "AddRangeAsync() + SaveChangesAsync()",
            Description = "Add multiple entities in one operation — batched INSERT",
            EntriesWritten = entriesWritten,
            Data = products.Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
        });
    }

    // PUT api/crudoperations/update/1
    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateExample(int id, [FromBody] UpdateProductRequest? request)
    {
        var product = await _context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ProductId == id);
        if (product == null) return NotFound(new { Error = $"Product {id} not found" });

        var originalPrice = product.UnitPrice;
        product.UnitPrice = request?.UnitPrice ?? (originalPrice ?? 0) + 1;
        if (request?.ProductName != null) product.ProductName = request.ProductName;

        var entriesWritten = await _context.SaveChangesAsync();

        return Ok(new
        {
            Method = "Tracked Update + SaveChangesAsync()",
            Description = "Modify a tracked entity's properties — EF detects changes and generates UPDATE SQL",
            EntriesWritten = entriesWritten,
            OriginalPrice = originalPrice,
            NewPrice = product.UnitPrice,
            Data = new { product.ProductId, product.ProductName, product.UnitPrice }
        });
    }

    // PUT api/crudoperations/update-method
    [HttpPut("update-method")]
    public async Task<IActionResult> UpdateMethodExample()
    {
        // Simulate disconnected entity scenario
        var product = await _context.Products.IgnoreQueryFilters().AsNoTracking().FirstAsync();
        var originalName = product.ProductName;
        product.ProductName = originalName + " (Updated)";

        _context.Products.Update(product); // Marks ALL properties as Modified
        var entriesWritten = await _context.SaveChangesAsync();

        return Ok(new
        {
            Method = "DbSet.Update()",
            Description = "Attach a disconnected entity and mark ALL properties as modified (full-entity update)",
            EntriesWritten = entriesWritten,
            Data = new { product.ProductId, OriginalName = originalName, NewName = product.ProductName }
        });
    }

    // DELETE api/crudoperations/remove/123
    [HttpDelete("remove/{id}")]
    public async Task<IActionResult> RemoveExample(int id)
    {
        var product = await _context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ProductId == id);
        if (product == null) return NotFound(new { Error = $"Product {id} not found" });

        // Check for related OrderDetails to avoid FK constraint violation
        var hasOrders = await _context.OrderDetails.AnyAsync(od => od.ProductId == id);
        if (hasOrders) return BadRequest(new { Error = $"Product {id} has related orders and cannot be deleted" });

        _context.Products.Remove(product);
        var entriesWritten = await _context.SaveChangesAsync();

        return Ok(new
        {
            Method = "Remove() + SaveChangesAsync()",
            Description = "Mark entity for deletion — EF generates DELETE SQL on SaveChanges",
            EntriesWritten = entriesWritten,
            DeletedProduct = new { product.ProductId, product.ProductName }
        });
    }

    // POST api/crudoperations/attach
    [HttpPost("attach")]
    public Task<IActionResult> AttachExample()
    {
        // Create a fresh context to demonstrate clean state
        var connStr = _context.Database.GetConnectionString()!;
        using var freshContext = new NorthwindContext(
            new DbContextOptionsBuilder<NorthwindContext>().UseSqlServer(connStr).Options);

        var product = new Product { ProductId = 1, ProductName = "Chai" };
        freshContext.Products.Attach(product);
        var stateAfterAttach = freshContext.Entry(product).State;

        product.UnitPrice = 99.99m;
        var stateAfterModify = freshContext.Entry(product).State;

        // Don't save — this is just for demonstration
        return Task.FromResult<IActionResult>(Ok(new
        {
            Method = "Attach() + EntityState",
            Description = "Attach a disconnected entity (Unchanged state), then modify it (changes to Modified)",
            StateAfterAttach = stateAfterAttach.ToString(),
            StateAfterModify = stateAfterModify.ToString(),
            Note = "Changes NOT saved — demonstration only"
        }));
    }

    // GET api/crudoperations/products
    [HttpGet("products")]
    public async Task<IActionResult> ListProducts()
    {
        var products = await _context.Products
            .IgnoreQueryFilters()
            .OrderByDescending(p => p.ProductId)
            .Take(20)
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice, p.Discontinued })
            .ToListAsync();
        return Ok(new { Method = "List Products", Description = "View latest products (for verifying CRUD operations)", Data = products });
    }
}

public class AddProductRequest
{
    public string? ProductName { get; set; }
    public decimal? UnitPrice { get; set; }
    public int? CategoryId { get; set; }
}

public class UpdateProductRequest
{
    public string? ProductName { get; set; }
    public decimal? UnitPrice { get; set; }
}
