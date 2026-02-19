import DemoSection, { type EndpointDemo } from '../components/DemoSection'

const demos: EndpointDemo[] = [
  {
    name: 'Where (LINQ Filter)',
    endpoint: '/api/basicqueries/where',
    method: 'GET',
    description: 'Filter products with UnitPrice > 20 using Where()',
    code: `// GET /api/basicqueries/where
var products = await _context.Products
    .Where(p => p.UnitPrice > 20)
    .Select(p => new {
        p.ProductId,
        p.ProductName,
        p.UnitPrice,
        p.UnitsInStock
    })
    .ToListAsync();`,
  },
  {
    name: 'Find (Primary Key)',
    endpoint: '/api/basicqueries/find/1',
    method: 'GET',
    description: 'Look up product by primary key using FindAsync()',
    code: `// GET /api/basicqueries/find/{id}
var product = await _context.Products.FindAsync(id);

// FindAsync checks the local cache first,
// only hitting the database if needed.`,
  },
  {
    name: 'FirstOrDefault',
    endpoint: '/api/basicqueries/firstordefault',
    method: 'GET',
    description: 'Get first product in category 1',
    code: `// GET /api/basicqueries/firstordefault
var product = await _context.Products
    .Where(p => p.CategoryId == 1)
    .FirstOrDefaultAsync();

// Returns null if no match is found.`,
  },
  {
    name: 'SingleOrDefault',
    endpoint: '/api/basicqueries/singleordefault/1',
    method: 'GET',
    description: 'Expects exactly one result \u2014 throws if more than one',
    code: `// GET /api/basicqueries/singleordefault/{id}
var product = await _context.Products
    .Where(p => p.ProductId == id)
    .SingleOrDefaultAsync();

// Throws InvalidOperationException if more than one element.`,
  },
  {
    name: 'Any (Existence Check)',
    endpoint: '/api/basicqueries/any',
    method: 'GET',
    description: 'Check if any product costs > $100',
    code: `// GET /api/basicqueries/any
var anyExpensive = await _context.Products
    .AnyAsync(p => p.UnitPrice > 100);

return Ok(new { anyExpensive });`,
  },
  {
    name: 'Count',
    endpoint: '/api/basicqueries/count',
    method: 'GET',
    description: 'Count total and discontinued products',
    code: `// GET /api/basicqueries/count
var totalCount = await _context.Products.CountAsync();
var discontinuedCount = await _context.Products
    .CountAsync(p => p.Discontinued);

return Ok(new { totalCount, discontinuedCount });`,
  },
  {
    name: 'Aggregates (Avg, Max, Min, Sum)',
    endpoint: '/api/basicqueries/aggregates',
    method: 'GET',
    description: 'Aggregate functions on Products table',
    code: `// GET /api/basicqueries/aggregates
var avgPrice = await _context.Products
    .AverageAsync(p => (double?)p.UnitPrice ?? 0);
var maxPrice = await _context.Products
    .MaxAsync(p => (double?)p.UnitPrice ?? 0);
var minPrice = await _context.Products
    .MinAsync(p => (double?)p.UnitPrice ?? 0);
var totalValue = await _context.Products
    .SumAsync(p => (double?)(p.UnitPrice * p.UnitsInStock) ?? 0);

return Ok(new { avgPrice, maxPrice, minPrice, totalValue });`,
  },
  {
    name: 'OrderBy / ThenBy',
    endpoint: '/api/basicqueries/orderby',
    method: 'GET',
    description: 'Top 10 most expensive products sorted',
    code: `// GET /api/basicqueries/orderby
var products = await _context.Products
    .OrderByDescending(p => p.UnitPrice)
    .ThenBy(p => p.ProductName)
    .Take(10)
    .Select(p => new {
        p.ProductName,
        p.UnitPrice,
        p.CategoryId
    })
    .ToListAsync();`,
  },
  {
    name: 'GroupBy',
    endpoint: '/api/basicqueries/groupby',
    method: 'GET',
    description: 'Group products by category',
    code: `// GET /api/basicqueries/groupby
var groups = await _context.Products
    .GroupBy(p => p.CategoryId)
    .Select(g => new {
        CategoryId = g.Key,
        ProductCount = g.Count(),
        AvgPrice = g.Average(p => p.UnitPrice),
        MaxPrice = g.Max(p => p.UnitPrice)
    })
    .ToListAsync();`,
  },
  {
    name: 'Select Projection',
    endpoint: '/api/basicqueries/select',
    method: 'GET',
    description: 'Project products with computed properties',
    code: `// GET /api/basicqueries/select
var products = await _context.Products
    .Select(p => new {
        p.ProductName,
        p.UnitPrice,
        p.UnitsInStock,
        StockValue = p.UnitPrice * p.UnitsInStock,
        IsExpensive = p.UnitPrice > 50
    })
    .Take(10)
    .ToListAsync();`,
  },
  {
    name: 'Distinct',
    endpoint: '/api/basicqueries/distinct',
    method: 'GET',
    description: 'Get distinct customer cities',
    code: `// GET /api/basicqueries/distinct
var cities = await _context.Customers
    .Select(c => c.City)
    .Where(c => c != null)
    .Distinct()
    .OrderBy(c => c)
    .ToListAsync();`,
  },
  {
    name: 'Contains (SQL IN)',
    endpoint: '/api/basicqueries/contains',
    method: 'GET',
    description: 'Products in categories 1, 2, or 3 using Contains()',
    code: `// GET /api/basicqueries/contains
var categoryIds = new List<int> { 1, 2, 3 };

var products = await _context.Products
    .Where(p => categoryIds.Contains(p.CategoryId ?? 0))
    .Select(p => new {
        p.ProductName,
        p.CategoryId,
        p.UnitPrice
    })
    .ToListAsync();

// Translates to: WHERE CategoryID IN (1, 2, 3)`,
  },
  {
    name: 'All',
    endpoint: '/api/basicqueries/all',
    method: 'GET',
    description: 'Check if ALL products are in stock',
    code: `// GET /api/basicqueries/all
var allInStock = await _context.Products
    .AllAsync(p => p.UnitsInStock > 0);

return Ok(new { allInStock });`,
  },
  {
    name: 'LINQ Query Syntax',
    endpoint: '/api/basicqueries/querysyntax',
    method: 'GET',
    description: 'Using from/where/select keywords instead of method chain',
    code: `// GET /api/basicqueries/querysyntax
var products = await (
    from p in _context.Products
    where p.UnitPrice > 20
    orderby p.ProductName
    select new {
        p.ProductName,
        p.UnitPrice,
        p.CategoryId
    }
).Take(10).ToListAsync();`,
  },
  {
    name: 'Join',
    endpoint: '/api/basicqueries/join',
    method: 'GET',
    description: 'Explicit LINQ join between Products and Categories',
    code: `// GET /api/basicqueries/join
var results = await _context.Products
    .Join(
        _context.Categories,
        p => p.CategoryId,
        c => c.CategoryId,
        (p, c) => new {
            p.ProductName,
            p.UnitPrice,
            CategoryName = c.CategoryName
        }
    )
    .Take(10)
    .ToListAsync();`,
  },
  {
    name: 'TagWith',
    endpoint: '/api/basicqueries/tagwith',
    method: 'GET',
    description: 'Adds SQL comment for diagnostics',
    code: `// GET /api/basicqueries/tagwith
var products = await _context.Products
    .TagWith("Getting top 5 expensive products for dashboard")
    .OrderByDescending(p => p.UnitPrice)
    .Take(5)
    .ToListAsync();

// Generated SQL includes:
// -- Getting top 5 expensive products for dashboard`,
  },
]

const Copyright = () => (
  <div style={{ textAlign: 'center', padding: '2rem 1rem', color: '#888', fontSize: '0.85rem', borderTop: '1px solid #eee', marginTop: '2rem' }}>
    &copy; 2026 Feb, Mahendran Rethinam. All rights reserved.
  </div>
)

const BasicQueries = () => (
  <>
    <DemoSection
      title="Basic LINQ Queries"
      subtitle="Demonstrates LINQ method syntax, query syntax, Find, FirstOrDefault, aggregates, and more."
      demos={demos}
    />
    <Copyright />
  </>
)

export default BasicQueries
