import DemoSection, { type EndpointDemo } from '../components/DemoSection'

const demos: EndpointDemo[] = [
  {
    name: 'ExecuteUpdate (Bulk Update)',
    endpoint: '/api/bulkoperations/executeupdate',
    method: 'POST',
    description: 'Update all matching rows in a single SQL UPDATE \u2014 bypasses the change tracker entirely',
    code: `// POST /api/bulkoperations/executeupdate
var rowsAffected = await _context.Products
    .Where(p => p.CategoryId == 1)
    .ExecuteUpdateAsync(setters => setters
        .SetProperty(p => p.UnitPrice, p => p.UnitPrice * 1.10m)
    );

// Generates:
// UPDATE Products SET UnitPrice = UnitPrice * 1.10
// WHERE CategoryID = 1
//
// No entities are loaded into memory.`,
  },
  {
    name: 'ExecuteDelete (Bulk Delete)',
    endpoint: '/api/bulkoperations/executedelete',
    method: 'POST',
    description: 'Delete all matching rows in a single SQL DELETE \u2014 no entity loading needed',
    code: `// POST /api/bulkoperations/executedelete
var rowsAffected = await _context.Products
    .Where(p => p.Discontinued && p.UnitsInStock == 0)
    .ExecuteDeleteAsync();

// Generates:
// DELETE FROM Products
// WHERE Discontinued = 1 AND UnitsInStock = 0
//
// Much faster than loading + Remove() + SaveChanges().`,
  },
  {
    name: 'ExecuteUpdate Multiple Properties',
    endpoint: '/api/bulkoperations/executeupdate-multiple',
    method: 'POST',
    description: 'Update multiple columns in a single bulk operation using chained SetProperty calls',
    code: `// POST /api/bulkoperations/executeupdate-multiple
var rowsAffected = await _context.Products
    .Where(p => p.CategoryId == 2)
    .ExecuteUpdateAsync(setters => setters
        .SetProperty(p => p.UnitPrice, p => p.UnitPrice * 1.05m)
        .SetProperty(p => p.Discontinued, false)
        .SetProperty(p => p.ReorderLevel, (short)10)
    );

// All three columns updated in a single UPDATE statement.
// Chain multiple SetProperty calls for each column.`,
  },
]

const BulkOperations = () => (
  <DemoSection
    title="Bulk Operations (EF Core 7+)"
    subtitle="Demonstrates ExecuteUpdate and ExecuteDelete \u2014 set-based operations that bypass the change tracker."
    demos={demos}
  />
)

export default BulkOperations
