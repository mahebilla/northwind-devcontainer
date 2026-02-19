import DemoSection, { type EndpointDemo } from '../components/DemoSection'

const demos: EndpointDemo[] = [
  {
    name: 'Include (Eager Loading)',
    endpoint: '/api/relateddata/eager-include',
    method: 'GET',
    description: 'Load products with their related Category in a single query using Include()',
    code: `// GET /api/relateddata/eager-include
var products = await _context.Products
    .Include(p => p.Category)
    .Take(10)
    .Select(p => new {
        p.ProductName,
        p.UnitPrice,
        CategoryName = p.Category!.CategoryName
    })
    .ToListAsync();

// Include() generates a LEFT JOIN in SQL.`,
  },
  {
    name: 'Include + ThenInclude (Multi-level)',
    endpoint: '/api/relateddata/eager-theninclude',
    method: 'GET',
    description: 'Load orders with customer and order details (multi-level eager loading)',
    code: `// GET /api/relateddata/eager-theninclude
var orders = await _context.Orders
    .Include(o => o.Customer)
    .Include(o => o.OrderDetails)
        .ThenInclude(od => od.Product)
    .Take(5)
    .ToListAsync();

// ThenInclude drills into the collection navigation.
// Customer -> Orders -> OrderDetails -> Product`,
  },
  {
    name: 'Explicit Loading',
    endpoint: '/api/relateddata/explicit-loading',
    method: 'GET',
    description: 'Load related data on demand after the principal entity is already loaded',
    code: `// GET /api/relateddata/explicit-loading
var category = await _context.Categories.FirstAsync();

// Load the related products explicitly
await _context.Entry(category)
    .Collection(c => c.Products)
    .LoadAsync();

// For reference navigation:
// await _context.Entry(product)
//     .Reference(p => p.Category)
//     .LoadAsync();`,
  },
  {
    name: 'Lazy Loading (Proxies)',
    endpoint: '/api/relateddata/lazy-loading',
    method: 'GET',
    description: 'Related data loaded automatically on first access (requires proxies package)',
    code: `// GET /api/relateddata/lazy-loading
// Requires: UseLazyLoadingProxies() in DbContext config
// Navigation properties must be virtual.

var product = await _context.Products.FirstAsync();

// Accessing product.Category triggers a lazy load query:
var categoryName = product.Category?.CategoryName;

// Warning: Can cause N+1 query problems in loops!`,
  },
  {
    name: 'AsSplitQuery',
    endpoint: '/api/relateddata/split-query',
    method: 'GET',
    description: 'Split a multi-Include query into separate SQL queries to avoid cartesian explosion',
    code: `// GET /api/relateddata/split-query
var orders = await _context.Orders
    .Include(o => o.Customer)
    .Include(o => o.OrderDetails)
        .ThenInclude(od => od.Product)
    .AsSplitQuery()
    .Take(5)
    .ToListAsync();

// AsSplitQuery sends multiple SELECT statements
// instead of one large JOIN, avoiding data duplication.`,
  },
  {
    name: 'Multiple Includes',
    endpoint: '/api/relateddata/multiple-includes',
    method: 'GET',
    description: 'Include multiple navigation properties on the same entity',
    code: `// GET /api/relateddata/multiple-includes
var orders = await _context.Orders
    .Include(o => o.Customer)
    .Include(o => o.Employee)
    .Include(o => o.ShipViaNavigation)
    .Take(5)
    .Select(o => new {
        o.OrderId,
        CustomerName = o.Customer!.CompanyName,
        EmployeeName = o.Employee!.FirstName + " " + o.Employee.LastName,
        ShipperName = o.ShipViaNavigation!.CompanyName
    })
    .ToListAsync();`,
  },
  {
    name: 'Filtered Include',
    endpoint: '/api/relateddata/filtered-include',
    method: 'GET',
    description: 'Apply Where/OrderBy/Take inside Include to filter the related collection',
    code: `// GET /api/relateddata/filtered-include
var categories = await _context.Categories
    .Include(c => c.Products
        .Where(p => !p.Discontinued)
        .OrderByDescending(p => p.UnitPrice)
        .Take(3)
    )
    .ToListAsync();

// Filtered Include (EF Core 5+) applies filters
// to the related collection loaded by Include.`,
  },
]

const RelatedData = () => (
  <DemoSection
    title="Related Data Loading"
    subtitle="Demonstrates eager loading (Include/ThenInclude), explicit loading, lazy loading, split queries, and filtered includes."
    demos={demos}
  />
)

export default RelatedData
