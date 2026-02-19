-- seed-read-db.sql
-- Creates and populates the NorthwindRead database read model tables.
-- These are denormalized projections used by the NorthwindCqrs CQRS project.
-- The joins are pre-computed here at seed time so read queries need zero JOINs.
--
-- Run once by post-create.sh on first container start.
-- To reset: docker volume rm devconainersetup_sqldata  (recreates from scratch)

-- ── Read Model Tables ─────────────────────────────────────────────────────────

CREATE TABLE ProductReadModel (
    ProductId    INT           NOT NULL PRIMARY KEY,
    ProductName  NVARCHAR(40)  NOT NULL,
    UnitPrice    MONEY         NULL,
    Discontinued BIT           NOT NULL DEFAULT 0,
    CategoryId   INT           NULL,          -- kept for Contains/GroupBy demos
    CategoryName NVARCHAR(15)  NULL,          -- denormalized from Categories (pre-joined)
    SupplierId   INT           NULL,
    UnitsInStock SMALLINT      NULL
);

CREATE TABLE OrderReadModel (
    OrderId      INT           NOT NULL PRIMARY KEY,
    OrderDate    DATETIME      NULL,
    CustomerName NVARCHAR(40)  NULL,          -- denormalized from Customers (pre-joined)
    CustomerId   NCHAR(5)      NULL,
    Freight      MONEY         NULL,
    ShipCountry  NVARCHAR(15)  NULL,
    EmployeeId   INT           NULL
);

CREATE TABLE OrderLineReadModel (
    Id          INT           NOT NULL IDENTITY PRIMARY KEY,
    OrderId     INT           NOT NULL,
    ProductName NVARCHAR(40)  NULL,           -- denormalized from Products (pre-joined)
    Quantity    SMALLINT      NULL,
    UnitPrice   MONEY         NULL,
    LineTotal   AS CAST(UnitPrice * Quantity AS MONEY)  -- computed column, no storage needed
);

CREATE TABLE CustomerReadModel (
    CustomerId  NCHAR(5)      NOT NULL PRIMARY KEY,
    CompanyName NVARCHAR(40)  NULL,
    City        NVARCHAR(15)  NULL,
    Country     NVARCHAR(15)  NULL
);

-- ── Populate from Northwind (cross-database INSERT SELECT) ────────────────────
-- Northwind and NorthwindRead share the same SQL Server instance,
-- so cross-database queries work with three-part names: [DB].[schema].[table]

INSERT INTO ProductReadModel (ProductId, ProductName, UnitPrice, Discontinued, CategoryId, CategoryName, SupplierId, UnitsInStock)
SELECT
    p.ProductID,
    p.ProductName,
    p.UnitPrice,
    p.Discontinued,
    p.CategoryID,
    c.CategoryName,       -- pre-join: CategoryName copied at seed time
    p.SupplierID,
    p.UnitsInStock
FROM Northwind.dbo.Products p
LEFT JOIN Northwind.dbo.Categories c ON p.CategoryID = c.CategoryID;

INSERT INTO OrderReadModel (OrderId, OrderDate, CustomerName, CustomerId, Freight, ShipCountry, EmployeeId)
SELECT
    o.OrderID,
    o.OrderDate,
    cu.CompanyName,       -- pre-join: CustomerName copied at seed time
    o.CustomerID,
    o.Freight,
    o.ShipCountry,
    o.EmployeeID
FROM Northwind.dbo.Orders o
LEFT JOIN Northwind.dbo.Customers cu ON o.CustomerID = cu.CustomerID;

INSERT INTO OrderLineReadModel (OrderId, ProductName, Quantity, UnitPrice)
SELECT
    od.OrderID,
    p.ProductName,        -- pre-join: ProductName copied at seed time
    od.Quantity,
    od.UnitPrice
    -- LineTotal is a computed column, do not insert it
FROM [Northwind].[dbo].[Order Details] od
LEFT JOIN Northwind.dbo.Products p ON od.ProductID = p.ProductID;

INSERT INTO CustomerReadModel (CustomerId, CompanyName, City, Country)
SELECT CustomerID, CompanyName, City, Country
FROM Northwind.dbo.Customers;

PRINT 'NorthwindRead seeded successfully.';
