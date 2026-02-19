#!/bin/bash
# post-create.sh — runs once after the dev container is created
# Installs tools, React dependencies, and restores the Northwind database
set -e

echo ""
echo "┌─────────────────────────────────────────────┐"
echo "│   Northwind Dev Container — First-Time Setup │"
echo "└─────────────────────────────────────────────┘"
echo ""

# ── Step 1: dotnet-ef CLI tool ────────────────────────────────────────────
echo "▶ [1/4] Installing dotnet-ef tool..."
dotnet tool install --global dotnet-ef --version 8.* 2>/dev/null || echo "  (already installed)"
echo 'export PATH="$PATH:$HOME/.dotnet/tools"' >> ~/.bashrc
export PATH="$PATH:$HOME/.dotnet/tools"
echo "  ✓ dotnet-ef ready"

# ── Step 2: React / Node dependencies ────────────────────────────────────
echo ""
echo "▶ [2/4] Installing React dependencies (npm install)..."
cd /workspace/northwind-client
npm install
echo "  ✓ node_modules ready"

# ── Step 3: Install sqlcmd (needed to restore Northwind DB) ──────────────
echo ""
echo "▶ [3/4] Installing sqlcmd..."
# The base image is Debian 12 (bookworm)
curl -sSL https://packages.microsoft.com/keys/microsoft.asc \
  | gpg --dearmor \
  | sudo tee /usr/share/keyrings/microsoft-prod.gpg > /dev/null

curl -sSL https://packages.microsoft.com/config/debian/12/prod.list \
  | sudo tee /etc/apt/sources.list.d/mssql-release.list > /dev/null

sudo apt-get update -qq 2>/dev/null || true   # ignore errors from unrelated repos (e.g. yarn)
ACCEPT_EULA=Y sudo apt-get install -y -q mssql-tools18 unixodbc-dev
echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> ~/.bashrc
export PATH="$PATH:/opt/mssql-tools18/bin"
echo "  ✓ sqlcmd ready"

# ── Step 4: Restore Northwind database ───────────────────────────────────
echo ""
echo "▶ [4/5] Setting up Northwind database..."

SA_PASS="${SA_PASSWORD:-YourStrong!Passw0rd}"
SQLCMD="/opt/mssql-tools18/bin/sqlcmd"

# Wait until SQL Server is ready to accept connections
echo "  Waiting for SQL Server to start..."
for i in $(seq 1 30); do
  if $SQLCMD -S db,1433 -U sa -P "$SA_PASS" -Q "SELECT 1" -C -l 5 > /dev/null 2>&1; then
    echo "  SQL Server is ready."
    break
  fi
  if [ "$i" -eq 30 ]; then
    echo "  ERROR: SQL Server did not respond after 150 seconds. Check Docker logs."
    exit 1
  fi
  echo "  attempt $i/30 — retrying in 5s..."
  sleep 5
done

# Check if Northwind already exists (avoids re-running on container rebuild)
DB_EXISTS=$($SQLCMD -S db,1433 -U sa -P "$SA_PASS" \
  -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name='Northwind'" \
  -C -h -1 2>/dev/null | tr -d ' \r\n')

if [ "$DB_EXISTS" = "1" ]; then
  echo "  Northwind database already exists — skipping restore."
else
  echo "  Downloading Northwind SQL script from Microsoft..."
  curl -sSL \
    "https://raw.githubusercontent.com/microsoft/sql-server-samples/master/samples/databases/northwind-pubs/instnwnd.sql" \
    -o /tmp/instnwnd.sql
  # The instnwnd.sql script has no CREATE DATABASE — create the DB first, then run the script into it
  echo "  Creating Northwind database..."
  $SQLCMD -S db,1433 -U sa -P "$SA_PASS" -Q "CREATE DATABASE Northwind" -C
  echo "  Populating Northwind tables and data..."
  $SQLCMD -S db,1433 -U sa -P "$SA_PASS" -d Northwind -i /tmp/instnwnd.sql -C
  echo "  ✓ Northwind database restored."
fi

# ── Step 5: Create NorthwindRead database (CQRS read models) ─────────────
echo ""
echo "▶ [5/5] Setting up NorthwindRead database (CQRS read models)..."

READ_DB_EXISTS=$($SQLCMD -S db,1433 -U sa -P "$SA_PASS" \
  -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name='NorthwindRead'" \
  -C -h -1 2>/dev/null | tr -d ' \r\n')

if [ "$READ_DB_EXISTS" = "1" ]; then
  echo "  NorthwindRead database already exists — skipping seed."
else
  echo "  Creating NorthwindRead database..."
  $SQLCMD -S db,1433 -U sa -P "$SA_PASS" -Q "CREATE DATABASE NorthwindRead" -C
  echo "  Seeding read model tables from Northwind..."
  $SQLCMD -S db,1433 -U sa -P "$SA_PASS" -d NorthwindRead \
    -i /workspace/.devcontainer/seed-read-db.sql -C
  echo "  ✓ NorthwindRead database seeded."
fi

# ── Done ──────────────────────────────────────────────────────────────────
echo ""
echo "┌──────────────────────────────────────────────────────────┐"
echo "│   Setup complete! Container is ready.                     │"
echo "├──────────────────────────────────────────────────────────┤"
echo "│  Start NorthwindApi (EF Core):                            │"
echo "│    cd /workspace/NorthwindApi && dotnet run               │"
echo "│    → http://localhost:5009/swagger                        │"
echo "│                                                           │"
echo "│  Start NorthwindCqrs (CQRS + Clean Architecture):        │"
echo "│    cd /workspace/NorthwindCqrs/NorthwindCqrs.Api          │"
echo "│    ASPNETCORE_URLS=http://+:5010 dotnet run               │"
echo "│    → http://localhost:5010/swagger                        │"
echo "│                                                           │"
echo "│  Start React:                                             │"
echo "│    cd /workspace/northwind-client && npm run dev          │"
echo "│    → http://localhost:5173                                │"
echo "└──────────────────────────────────────────────────────────┘"
echo ""
