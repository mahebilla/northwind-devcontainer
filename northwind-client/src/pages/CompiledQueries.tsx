import DemoSection, { type EndpointDemo } from '../components/DemoSection'

const demos: EndpointDemo[] = [
  {
    name: 'Compiled Query by Price',
    endpoint: '/api/compiledqueries/byprice?minPrice=20',
    method: 'GET',
    description: 'Uses EF.CompileQuery to pre-compile a parameterized LINQ query — avoids repeated expression tree compilation.',
    code: `// Compiled once as a static field — reused on every call
private static readonly Func<NorthwindContext, decimal, IEnumerable<Product>> _getByPrice =
    EF.CompileQuery((NorthwindContext ctx, decimal min) =>
        ctx.Products.Where(p => p.UnitPrice > min));

// Usage in controller action:
[HttpGet("byprice")]
public IActionResult GetByPrice(decimal minPrice)
{
    var products = _getByPrice(_context, minPrice).ToList();
    return Ok(new {
        method = "EF.CompileQuery",
        description = $"Products with UnitPrice > {minPrice} (~15% faster than regular LINQ)",
        data = products
    });
}`,
  },
  {
    name: 'Compiled Async Query by ID',
    endpoint: '/api/compiledqueries/byid/1',
    method: 'GET',
    description: 'Uses EF.CompileAsyncQuery returning Task<Product?> for async single-entity lookup.',
    code: `// Async compiled query — returns Task<Product?>
private static readonly Func<NorthwindContext, int, Task<Product?>> _getById =
    EF.CompileAsyncQuery((NorthwindContext ctx, int id) =>
        ctx.Products.FirstOrDefault(p => p.ProductId == id));

// Usage:
[HttpGet("byid/{id}")]
public async Task<IActionResult> GetById(int id)
{
    var product = await _getById(_context, id);
    if (product == null) return NotFound();

    return Ok(new {
        method = "EF.CompileAsyncQuery (single entity)",
        description = "Pre-compiled async lookup by primary key",
        data = product
    });
}`,
  },
  {
    name: 'Compiled Query -- All Products',
    endpoint: '/api/compiledqueries/all',
    method: 'GET',
    description: 'Simplest compiled query with no parameters — returns all products with zero expression tree overhead.',
    code: `// No parameters — compiled query for a full table scan
private static readonly Func<NorthwindContext, IEnumerable<Product>> _getAll =
    EF.CompileQuery((NorthwindContext ctx) =>
        ctx.Products.OrderBy(p => p.ProductName));

// Usage:
[HttpGet("all")]
public IActionResult GetAll()
{
    var products = _getAll(_context).ToList();
    return Ok(new {
        method = "EF.CompileQuery (no parameters)",
        description = "All products, pre-compiled — no expression tree compilation on each call",
        data = products
    });
}`,
  },
  {
    name: 'Compiled Async Search (IAsyncEnumerable)',
    endpoint: '/api/compiledqueries/search?q=Ch',
    method: 'GET',
    description: 'Uses EF.CompileAsyncQuery returning IAsyncEnumerable<T> for streaming results with await foreach.',
    code: `// Compiled async query returning IAsyncEnumerable for streaming
private static readonly Func<NorthwindContext, string, IAsyncEnumerable<Product>> _search =
    EF.CompileAsyncQuery((NorthwindContext ctx, string term) =>
        ctx.Products.Where(p => p.ProductName.Contains(term)));

// Usage with await foreach:
[HttpGet("search")]
public async Task<IActionResult> Search(string q)
{
    var results = new List<Product>();
    await foreach (var product in _search(_context, q))
    {
        results.Add(product);
    }

    return Ok(new {
        method = "EF.CompileAsyncQuery (IAsyncEnumerable)",
        description = $"Streaming search for '{q}' — results arrive as they're read from DB",
        data = results
    });
}`,
  },
]

const CompiledQueries = () => (
  <DemoSection
    title="Compiled Queries"
    subtitle="Demonstrates EF.CompileQuery and EF.CompileAsyncQuery — pre-compiled LINQ for ~15% faster execution."
    demos={demos}
  />
)

export default CompiledQueries
