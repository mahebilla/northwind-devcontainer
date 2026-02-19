import DemoSection, { type EndpointDemo } from '../components/DemoSection'

const demos: EndpointDemo[] = [
  {
    name: 'Basic Transaction (Commit)',
    endpoint: '/api/transactions/basic',
    method: 'POST',
    description: 'Wraps a price update in an explicit transaction — BeginTransactionAsync, SaveChangesAsync, then CommitAsync.',
    code: `using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    var product = await _context.Products.FirstAsync();
    product.UnitPrice += 1;
    await _context.SaveChangesAsync();

    // Commit persists the change to the database
    await transaction.CommitAsync();
    return Ok(new { message = "Committed", product.ProductName, product.UnitPrice });
}
catch
{
    // Rollback undoes everything since BeginTransaction
    await transaction.RollbackAsync();
    throw;
}`,
  },
  {
    name: 'Transaction Rollback',
    endpoint: '/api/transactions/rollback',
    method: 'POST',
    description: 'Modifies data and calls SaveChangesAsync, then explicitly rolls back — proving the database is unchanged.',
    code: `using var transaction = await _context.Database.BeginTransactionAsync();

var product = await _context.Products.FirstAsync();
var originalPrice = product.UnitPrice;

// Modify and flush to DB (within the transaction)
product.UnitPrice += 100;
await _context.SaveChangesAsync();

// Rollback undoes the SaveChanges
await transaction.RollbackAsync();

// Verify: re-read from DB shows original price
var verify = await _context.Products
    .AsNoTracking()
    .FirstAsync(p => p.ProductId == product.ProductId);

return Ok(new {
    originalPrice,
    priceAfterSaveChanges = product.UnitPrice,
    priceAfterRollback = verify.UnitPrice,
    rollbackSucceeded = verify.UnitPrice == originalPrice
});`,
  },
  {
    name: 'Savepoints',
    endpoint: '/api/transactions/savepoint',
    method: 'POST',
    description: 'Creates a savepoint mid-transaction, then rolls back to it — partial undo within a single transaction.',
    code: `using var transaction = await _context.Database.BeginTransactionAsync();

// Step 1: Insert first product
_context.Products.Add(new Product { ProductName = "Savepoint Product A" });
await _context.SaveChangesAsync();

// Create savepoint after first insert
await transaction.CreateSavepointAsync("AfterFirstInsert");

// Step 2: Insert second product
_context.Products.Add(new Product { ProductName = "Savepoint Product B" });
await _context.SaveChangesAsync();

// Roll back to savepoint — undoes Product B but keeps Product A
await transaction.RollbackToSavepointAsync("AfterFirstInsert");

// Commit — only Product A is persisted
await transaction.CommitAsync();

return Ok(new {
    message = "Product A committed, Product B rolled back via savepoint"
});`,
  },
  {
    name: 'Multiple SaveChanges in Transaction',
    endpoint: '/api/transactions/multiple-savechanges',
    method: 'POST',
    description: 'Calls SaveChangesAsync twice within one transaction, then commits both as a single atomic unit.',
    code: `using var transaction = await _context.Database.BeginTransactionAsync();

// First batch: update a product price
var product = await _context.Products.FirstAsync();
product.UnitPrice += 5;
var firstSave = await _context.SaveChangesAsync();

// Second batch: update a different product
var product2 = await _context.Products.Skip(1).FirstAsync();
product2.UnitsInStock += 10;
var secondSave = await _context.SaveChangesAsync();

// Single commit for both SaveChanges calls
await transaction.CommitAsync();

return Ok(new {
    message = "Both SaveChanges committed atomically",
    firstSaveAffectedRows = firstSave,
    secondSaveAffectedRows = secondSave,
    product1 = new { product.ProductName, product.UnitPrice },
    product2 = new { product2.ProductName, product2.UnitsInStock }
});`,
  },
]

const Transactions = () => (
  <DemoSection
    title="Transactions"
    subtitle="Demonstrates explicit transactions with BeginTransaction, Commit, Rollback, Savepoints, and multiple SaveChanges."
    demos={demos}
  />
)

export default Transactions
