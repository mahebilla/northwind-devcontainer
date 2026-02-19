using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApi.Data;
using NorthwindApi.Models;

namespace NorthwindApi.Controllers;

/// <summary>
/// Demonstrates: ChangeTracker.Entries, EntityState enum, OriginalValues/CurrentValues,
/// IsModified, DetectChanges, HasChanges
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ChangeTrackerController : ControllerBase
{
    private readonly NorthwindContext _context;

    public ChangeTrackerController(NorthwindContext context) => _context = context;

    // GET api/changetracker/entries
    [HttpGet("entries")]
    public async Task<IActionResult> EntriesExample()
    {
        // Load some entities
        var products = await _context.Products.IgnoreQueryFilters().Take(3).ToListAsync();
        // Modify one
        products[0].UnitPrice = (products[0].UnitPrice ?? 0) + 1;
        // Add a new one
        _context.Products.Add(new Product { ProductName = "ChangeTracker Demo" });

        var entries = _context.ChangeTracker.Entries().Select(e => new
        {
            EntityType = e.Entity.GetType().Name,
            State = e.State.ToString(),
            PrimaryKey = e.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue
        }).ToList();

        // Don't save — just demonstrate tracking state
        return Ok(new
        {
            Method = "ChangeTracker.Entries()",
            Description = "Inspect all entities tracked by the context and their states",
            Data = entries
        });
    }

    // GET api/changetracker/entitystate
    [HttpGet("entitystate")]
    public async Task<IActionResult> EntityStateExample()
    {
        // Use a fresh context
        var connStr = _context.Database.GetConnectionString()!;
        using var ctx = new NorthwindContext(
            new DbContextOptionsBuilder<NorthwindContext>().UseSqlServer(connStr).Options);

        // Unchanged
        var product = await ctx.Products.IgnoreQueryFilters().FirstAsync();
        var unchangedState = ctx.Entry(product).State;

        // Modified
        product.UnitPrice = (product.UnitPrice ?? 0) + 1;
        var modifiedState = ctx.Entry(product).State;

        // Added
        var newProduct = new Product { ProductName = "EntityState Demo" };
        ctx.Products.Add(newProduct);
        var addedState = ctx.Entry(newProduct).State;

        // Detached
        var detached = new Product { ProductName = "Detached Product" };
        var detachedState = ctx.Entry(detached).State;

        // Deleted
        var toDelete = await ctx.Products.IgnoreQueryFilters()
            .OrderByDescending(p => p.ProductId).FirstAsync();
        ctx.Products.Remove(toDelete);
        var deletedState = ctx.Entry(toDelete).State;

        return Ok(new
        {
            Method = "EntityState values",
            Description = "All five possible entity states in EF Core",
            States = new[]
            {
                new { State = unchangedState.ToString(), Meaning = "Entity loaded from DB, no changes detected" },
                new { State = modifiedState.ToString(), Meaning = "Entity property was changed" },
                new { State = addedState.ToString(), Meaning = "New entity added via Add()" },
                new { State = detachedState.ToString(), Meaning = "Entity not tracked by the context" },
                new { State = deletedState.ToString(), Meaning = "Entity marked for deletion via Remove()" }
            }
        });
    }

    // GET api/changetracker/original-current
    [HttpGet("original-current")]
    public async Task<IActionResult> OriginalCurrentExample()
    {
        var connStr = _context.Database.GetConnectionString()!;
        using var ctx = new NorthwindContext(
            new DbContextOptionsBuilder<NorthwindContext>().UseSqlServer(connStr).Options);

        var product = await ctx.Products.IgnoreQueryFilters().FirstAsync();
        product.UnitPrice = 999.99m;
        product.ProductName = "Modified Name";

        var entry = ctx.Entry(product);

        var changes = entry.Properties
            .Where(p => p.Metadata.Name == "UnitPrice" || p.Metadata.Name == "ProductName")
            .Select(p => new
            {
                Property = p.Metadata.Name,
                OriginalValue = p.OriginalValue?.ToString(),
                CurrentValue = p.CurrentValue?.ToString(),
                IsModified = p.IsModified
            }).ToList();

        var modifiedProperties = entry.Properties
            .Where(p => p.IsModified)
            .Select(p => p.Metadata.Name)
            .ToList();

        return Ok(new
        {
            Method = "OriginalValues / CurrentValues / IsModified",
            Description = "Compare original DB values with current in-memory values for each property",
            Changes = changes,
            AllModifiedProperties = modifiedProperties
        });
    }

    // GET api/changetracker/detect-changes
    [HttpGet("detect-changes")]
    public async Task<IActionResult> DetectChangesExample()
    {
        var connStr = _context.Database.GetConnectionString()!;
        using var ctx = new NorthwindContext(
            new DbContextOptionsBuilder<NorthwindContext>().UseSqlServer(connStr).Options);

        var product = await ctx.Products.IgnoreQueryFilters().FirstAsync();

        var hasChangesBefore = ctx.ChangeTracker.HasChanges();

        product.UnitPrice = (product.UnitPrice ?? 0) + 1;
        ctx.ChangeTracker.DetectChanges(); // Explicitly scan for changes

        var hasChangesAfter = ctx.ChangeTracker.HasChanges();

        return Ok(new
        {
            Method = "DetectChanges() / HasChanges()",
            Description = "Manually trigger change detection and check if any tracked entities have been modified",
            HasChangesBefore = hasChangesBefore,
            HasChangesAfter = hasChangesAfter,
            Note = "DetectChanges() is normally called automatically by SaveChanges() — calling it explicitly is for advanced scenarios"
        });
    }
}
