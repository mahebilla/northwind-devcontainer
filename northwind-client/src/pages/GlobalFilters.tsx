import DemoSection, { type EndpointDemo } from '../components/DemoSection'

const demos: EndpointDemo[] = [
  {
    name: 'Filtered (Auto-Applied)',
    endpoint: '/api/globalfilters/filtered',
    method: 'GET',
    description: 'Global filter p => !p.Discontinued is auto-applied — only active products shown.',
    code: `// In NorthwindContext.OnModelCreating:
modelBuilder.Entity<Product>()
    .HasQueryFilter(p => !p.Discontinued);

// All queries on Products auto-exclude discontinued:
[HttpGet("filtered")]
public async Task<IActionResult> GetFiltered()
{
    // No WHERE clause needed — the filter is applied automatically
    var products = await _context.Products
        .OrderBy(p => p.ProductName)
        .ToListAsync();

    return Ok(new {
        method = "HasQueryFilter (auto-applied)",
        description = $"Returned {products.Count} active products — discontinued are excluded automatically",
        data = products
    });
}`,
  },
  {
    name: 'Unfiltered (IgnoreQueryFilters)',
    endpoint: '/api/globalfilters/unfiltered',
    method: 'GET',
    description: 'Bypasses the global filter with IgnoreQueryFilters() to see ALL products including discontinued.',
    code: `[HttpGet("unfiltered")]
public async Task<IActionResult> GetUnfiltered()
{
    // IgnoreQueryFilters() bypasses ALL global filters on this entity
    var products = await _context.Products
        .IgnoreQueryFilters()
        .OrderBy(p => p.ProductName)
        .ToListAsync();

    return Ok(new {
        method = "IgnoreQueryFilters()",
        description = $"Returned {products.Count} products — includes discontinued items",
        data = products
    });
}`,
  },
  {
    name: 'Filter Comparison',
    endpoint: '/api/globalfilters/comparison',
    method: 'GET',
    description: 'Runs both filtered and unfiltered queries side-by-side to show the effect of HasQueryFilter.',
    code: `[HttpGet("comparison")]
public async Task<IActionResult> GetComparison()
{
    // Filtered count (global filter active)
    var filteredCount = await _context.Products.CountAsync();

    // Unfiltered count (global filter bypassed)
    var unfilteredCount = await _context.Products
        .IgnoreQueryFilters()
        .CountAsync();

    // Get the discontinued products that are hidden by the filter
    var discontinuedProducts = await _context.Products
        .IgnoreQueryFilters()
        .Where(p => p.Discontinued)
        .Select(p => new { p.ProductId, p.ProductName, p.Discontinued })
        .ToListAsync();

    return Ok(new {
        method = "HasQueryFilter vs IgnoreQueryFilters comparison",
        description = $"Filter hides {unfilteredCount - filteredCount} discontinued products",
        data = new {
            filteredCount,
            unfilteredCount,
            hiddenByFilter = unfilteredCount - filteredCount,
            discontinuedProducts
        }
    });
}`,
  },
]

const GlobalFilters = () => (
  <DemoSection
    title="Global Query Filters"
    subtitle="Demonstrates HasQueryFilter (auto-applied WHERE clause) and IgnoreQueryFilters to bypass it."
    demos={demos}
  />
)

export default GlobalFilters
