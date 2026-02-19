import DemoSection, { type EndpointDemo } from '../components/DemoSection'

const demos: EndpointDemo[] = [
  {
    name: 'Offset Pagination (Skip/Take)',
    endpoint: '/api/pagination/offset?page=1&pageSize=10',
    method: 'GET',
    description: 'Classic offset pagination using Skip/Take — simple but performance degrades on deep pages.',
    code: `[HttpGet("offset")]
public async Task<IActionResult> GetOffset(int page = 1, int pageSize = 10)
{
    var totalCount = await _context.Products.CountAsync();
    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

    var products = await _context.Products
        .OrderBy(p => p.ProductId)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    // SQL generated: SELECT ... ORDER BY p.ProductId OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY
    return Ok(new {
        method = "Skip/Take (Offset Pagination)",
        description = $"Page {page} of {totalPages} — OFFSET {(page - 1) * pageSize} ROWS FETCH NEXT {pageSize}",
        data = new { page, pageSize, totalCount, totalPages, products }
    });
}`,
  },
  {
    name: 'Keyset (Cursor) Pagination',
    endpoint: '/api/pagination/keyset?lastId=0&pageSize=10',
    method: 'GET',
    description: 'Uses WHERE instead of OFFSET — constant performance regardless of page depth.',
    code: `[HttpGet("keyset")]
public async Task<IActionResult> GetKeyset(int lastId = 0, int pageSize = 10)
{
    var products = await _context.Products
        .OrderBy(p => p.ProductId)
        .Where(p => p.ProductId > lastId)
        .Take(pageSize)
        .ToListAsync();

    // SQL generated: SELECT TOP(@take) ... WHERE p.ProductId > @lastId ORDER BY p.ProductId
    // No OFFSET — uses an index seek instead of scanning past skipped rows
    var nextLastId = products.Any() ? products.Last().ProductId : (int?)null;

    return Ok(new {
        method = "Keyset (Cursor) Pagination",
        description = $"WHERE ProductId > {lastId} — O(1) performance vs O(n) for offset on deep pages",
        data = new { lastId, pageSize, nextLastId, products }
    });
}`,
  },
  {
    name: 'Pagination with Include',
    endpoint: '/api/pagination/orders?page=1&pageSize=10',
    method: 'GET',
    description: 'Combines offset pagination with eager loading (Include) to paginate orders with their related customer data.',
    code: `[HttpGet("orders")]
public async Task<IActionResult> GetOrders(int page = 1, int pageSize = 10)
{
    var totalCount = await _context.Orders.CountAsync();
    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

    var orders = await _context.Orders
        .Include(o => o.Customer)
        .OrderByDescending(o => o.OrderDate)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(o => new {
            o.OrderId,
            o.OrderDate,
            CustomerName = o.Customer != null ? o.Customer.CompanyName : null,
            o.ShipCity,
            o.ShipCountry
        })
        .ToListAsync();

    return Ok(new {
        method = "Pagination + Include (eager loading)",
        description = $"Page {page} of {totalPages} — orders with customer data joined via Include",
        data = new { page, pageSize, totalCount, totalPages, orders }
    });
}`,
  },
]

const Pagination = () => (
  <DemoSection
    title="Pagination"
    subtitle="Demonstrates Skip/Take offset pagination and keyset (cursor-based) pagination."
    demos={demos}
  />
)

export default Pagination
