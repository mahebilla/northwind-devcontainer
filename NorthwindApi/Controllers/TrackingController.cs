using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApi.Data;

namespace NorthwindApi.Controllers;

/// <summary>
/// Demonstrates: Default tracking, AsNoTracking, AsNoTrackingWithIdentityResolution,
/// performance comparison
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class TrackingController : ControllerBase
{
    private readonly NorthwindContext _context;

    public TrackingController(NorthwindContext context) => _context = context;

    // GET api/tracking/tracked
    [HttpGet("tracked")]
    public async Task<IActionResult> TrackedExample()
    {
        var products = await _context.Products.IgnoreQueryFilters().Take(5).ToListAsync();
        var trackedCount = _context.ChangeTracker.Entries().Count();
        return Ok(new
        {
            Method = "Default Tracking",
            Description = "Entities are tracked by ChangeTracker — allows SaveChanges() to detect modifications",
            TrackedEntities = trackedCount,
            Data = products.Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
        });
    }

    // GET api/tracking/notracking
    [HttpGet("notracking")]
    public async Task<IActionResult> NoTrackingExample()
    {
        var products = await _context.Products.IgnoreQueryFilters().AsNoTracking().Take(5).ToListAsync();
        var trackedCount = _context.ChangeTracker.Entries().Count();
        return Ok(new
        {
            Method = "AsNoTracking()",
            Description = "No entities tracked — better performance for read-only queries",
            TrackedEntities = trackedCount,
            Note = "0 entities tracked = less memory, faster queries",
            Data = products.Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
        });
    }

    // GET api/tracking/identity-resolution
    [HttpGet("identity-resolution")]
    public async Task<IActionResult> IdentityResolutionExample()
    {
        var orderDetails = await _context.OrderDetails
            .AsNoTrackingWithIdentityResolution()
            .Include(od => od.Product)
            .Take(20)
            .ToListAsync();
        var trackedCount = _context.ChangeTracker.Entries().Count();
        var uniqueProducts = orderDetails.Select(od => od.Product).Distinct().Count();
        return Ok(new
        {
            Method = "AsNoTrackingWithIdentityResolution()",
            Description = "Not tracked, but duplicate entities are resolved to the same object instance",
            TrackedEntities = trackedCount,
            TotalOrderDetails = orderDetails.Count,
            UniqueProductInstances = uniqueProducts,
            Note = "Same Product referenced by multiple OrderDetails points to the same object"
        });
    }

    // GET api/tracking/comparison
    [HttpGet("comparison")]
    public async Task<IActionResult> ComparisonExample()
    {
        var connStr = _context.Database.GetConnectionString()!;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Tracked query
        using var ctx1 = new NorthwindContext(
            new DbContextOptionsBuilder<NorthwindContext>().UseSqlServer(connStr).Options);
        sw.Restart();
        var tracked = await ctx1.Products.IgnoreQueryFilters().ToListAsync();
        var trackedMs = sw.ElapsedMilliseconds;
        var trackedEntities = ctx1.ChangeTracker.Entries().Count();

        // No-tracking query
        using var ctx2 = new NorthwindContext(
            new DbContextOptionsBuilder<NorthwindContext>().UseSqlServer(connStr).Options);
        sw.Restart();
        var noTracking = await ctx2.Products.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        var noTrackingMs = sw.ElapsedMilliseconds;
        var noTrackingEntities = ctx2.ChangeTracker.Entries().Count();

        return Ok(new
        {
            Method = "Performance Comparison",
            Description = "Compare execution time and memory (tracked entities) between tracking modes",
            Tracked = new { TimeMs = trackedMs, EntitiesTracked = trackedEntities, ResultCount = tracked.Count },
            NoTracking = new { TimeMs = noTrackingMs, EntitiesTracked = noTrackingEntities, ResultCount = noTracking.Count }
        });
    }
}
