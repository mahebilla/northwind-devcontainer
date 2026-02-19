using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApi.Data;

namespace NorthwindApi.Controllers;

/// <summary>
/// Demonstrates: HasQueryFilter (global query filter), IgnoreQueryFilters
/// The NorthwindContext has: modelBuilder.Entity&lt;Product&gt;().HasQueryFilter(p => !p.Discontinued);
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class GlobalFiltersController : ControllerBase
{
    private readonly NorthwindContext _context;

    public GlobalFiltersController(NorthwindContext context) => _context = context;

    // GET api/globalfilters/filtered
    [HttpGet("filtered")]
    public async Task<IActionResult> FilteredExample()
    {
        // Global filter is auto-applied: only non-discontinued products
        var products = await _context.Products
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice, p.Discontinued })
            .ToListAsync();
        return Ok(new
        {
            Method = "Global Query Filter (auto-applied)",
            Description = "HasQueryFilter(p => !p.Discontinued) — automatically excludes discontinued products from all queries",
            Count = products.Count,
            Note = "All products have Discontinued=false — the filter is invisible in the LINQ query",
            Data = products
        });
    }

    // GET api/globalfilters/unfiltered
    [HttpGet("unfiltered")]
    public async Task<IActionResult> UnfilteredExample()
    {
        // Bypass the global filter
        var products = await _context.Products
            .IgnoreQueryFilters()
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice, p.Discontinued })
            .ToListAsync();
        return Ok(new
        {
            Method = "IgnoreQueryFilters()",
            Description = "Bypass the global filter — returns ALL products including discontinued ones",
            Count = products.Count,
            Data = products
        });
    }

    // GET api/globalfilters/comparison
    [HttpGet("comparison")]
    public async Task<IActionResult> ComparisonExample()
    {
        var filteredCount = await _context.Products.CountAsync();
        var unfilteredCount = await _context.Products.IgnoreQueryFilters().CountAsync();

        return Ok(new
        {
            Method = "Global Filter Comparison",
            Description = "Compare counts with and without the global query filter",
            ActiveProducts = filteredCount,
            AllProducts = unfilteredCount,
            DiscontinuedProducts = unfilteredCount - filteredCount,
            Note = $"{unfilteredCount - filteredCount} discontinued products are hidden by the global filter"
        });
    }
}
