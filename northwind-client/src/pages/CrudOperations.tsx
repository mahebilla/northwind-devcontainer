import DemoSection, { type EndpointDemo } from '../components/DemoSection'

const demos: EndpointDemo[] = [
  {
    name: 'Add (Single Entity)',
    endpoint: '/api/crudoperations/add',
    method: 'POST',
    description: 'Insert a new product using DbSet.Add and SaveChangesAsync',
    body: { productName: 'Demo Product', unitPrice: 15.99 },
    code: `// POST /api/crudoperations/add
var product = new Product
{
    ProductName = dto.ProductName,
    UnitPrice = dto.UnitPrice,
    Discontinued = false
};

_context.Products.Add(product);
await _context.SaveChangesAsync();

// After SaveChanges, product.ProductId is set
// by the database (identity column).`,
  },
  {
    name: 'AddRange (Batch Insert)',
    endpoint: '/api/crudoperations/addrange',
    method: 'POST',
    description: 'Insert multiple products at once using AddRange',
    code: `// POST /api/crudoperations/addrange
var products = new List<Product>
{
    new Product { ProductName = "Batch Item 1", UnitPrice = 10.00m },
    new Product { ProductName = "Batch Item 2", UnitPrice = 20.00m },
    new Product { ProductName = "Batch Item 3", UnitPrice = 30.00m }
};

_context.Products.AddRange(products);
await _context.SaveChangesAsync();

// AddRange is more efficient than calling Add in a loop.`,
  },
  {
    name: 'Update (Tracked Entity)',
    endpoint: '/api/crudoperations/update/1',
    method: 'PUT',
    description: 'Fetch an entity, modify it, and let the change tracker detect the update',
    code: `// PUT /api/crudoperations/update/{id}
var product = await _context.Products.FindAsync(id);
if (product == null) return NotFound();

product.ProductName = product.ProductName + " (Updated)";
product.UnitPrice = (product.UnitPrice ?? 0) + 1.00m;

// No need to call Update() — the change tracker
// detects modifications automatically.
await _context.SaveChangesAsync();`,
  },
  {
    name: 'DbSet.Update (Disconnected)',
    endpoint: '/api/crudoperations/update-method',
    method: 'PUT',
    description: 'Attach a disconnected entity and mark it as Modified using DbSet.Update()',
    code: `// PUT /api/crudoperations/update-method
var product = new Product
{
    ProductId = dto.ProductId,
    ProductName = dto.ProductName,
    UnitPrice = dto.UnitPrice
};

_context.Products.Update(product);
await _context.SaveChangesAsync();

// Update() marks ALL properties as Modified.
// Use this for disconnected scenarios (e.g., API payloads).`,
  },
  {
    name: 'Remove',
    endpoint: '/api/crudoperations/remove/999',
    method: 'DELETE',
    description: 'Delete a product by ID (uses a high ID to avoid removing real data)',
    code: `// DELETE /api/crudoperations/remove/{id}
var product = await _context.Products.FindAsync(id);
if (product == null) return NotFound();

_context.Products.Remove(product);
await _context.SaveChangesAsync();

// Remove marks the entity as Deleted.
// SaveChanges issues a DELETE SQL statement.`,
  },
  {
    name: 'Attach + EntityState',
    endpoint: '/api/crudoperations/attach',
    method: 'POST',
    description: 'Attach a disconnected entity and explicitly set its EntityState',
    code: `// POST /api/crudoperations/attach
var product = new Product
{
    ProductId = dto.ProductId,
    ProductName = dto.ProductName,
    UnitPrice = dto.UnitPrice
};

_context.Attach(product);
_context.Entry(product).State = EntityState.Modified;

await _context.SaveChangesAsync();

// Attach starts tracking as Unchanged.
// Setting State = Modified forces an UPDATE.`,
  },
  {
    name: 'List Products (Verify)',
    endpoint: '/api/crudoperations/products',
    method: 'GET',
    description: 'List all products to verify CRUD operations',
    code: `// GET /api/crudoperations/products
var products = await _context.Products
    .OrderByDescending(p => p.ProductId)
    .Take(20)
    .Select(p => new {
        p.ProductId,
        p.ProductName,
        p.UnitPrice,
        p.Discontinued
    })
    .ToListAsync();`,
  },
]

const CrudOperations = () => (
  <DemoSection
    title="CRUD Operations"
    subtitle="Demonstrates Add, AddRange, Update, Remove, and Attach with EntityState. Changes are persisted to the database."
    demos={demos}
  />
)

export default CrudOperations
