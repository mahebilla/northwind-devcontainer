# Northwind EF Core App — Dev Container

A full-stack learning app demonstrating **Entity Framework Core 8** concepts using the classic Northwind database.
Each API controller covers a different EF Core concept with a matching React page.

| Layer | Tech | Port |
|-------|------|------|
| Backend API | ASP.NET Core 8 + EF Core 8 | `5009` |
| Frontend | React 19 + TypeScript + Vite 7 | `5173` |
| Database | SQL Server 2022 (Linux) | `1433` |

---

## What Is a Dev Container?

Instead of manually installing .NET, Node.js, and SQL Server on your machine, everything runs inside a Docker container. VS Code connects to it transparently — you edit files locally, builds and terminals run inside Linux.

**You only need two things on your machine:**
- Docker Desktop
- VS Code + Dev Containers extension

---

## Prerequisites (One-Time Per Machine)

### 1. Docker Desktop
- Download: https://www.docker.com/products/docker-desktop/
- During install: select **"Use WSL 2 based engine"** (Windows 11 recommended)
- After install: open Docker Desktop and wait until it shows **"Docker is running"**
- Verify in terminal: `docker --version`

### 2. WSL 2 (Windows only — if not already installed)
Open **PowerShell as Administrator**:
```powershell
wsl --install
# Restart your machine when prompted
wsl --version   # verify after restart
```

### 3. VS Code + Dev Containers Extension
- Download VS Code: https://code.visualstudio.com/
- Install the extension: open VS Code → `Ctrl+Shift+X` → search **Dev Containers** → Install
- Extension ID: `ms-vscode-remote.remote-containers`

---

## Getting Started

### Step 1 — Clone the repo
```bash
git clone https://github.com/mahebilla/northwind-devcontainer.git
cd northwind-devcontainer
```

### Step 2 — Open in VS Code
```bash
code .
```
VS Code shows a popup in the bottom-right corner:

> **"Folder contains a Dev Container configuration file. Reopen in Container?"**

Click **"Reopen in Container"**.

> If the popup doesn't appear: press `Ctrl+Shift+P` → type **Reopen in Container** → Enter

### Step 3 — Wait for first-time setup
The first time takes **5–10 minutes** while Docker:
- Downloads the .NET 8 + SQL Server 2022 images (~2 GB)
- Installs dotnet-ef tools and npm packages
- Creates and populates the Northwind database automatically

You'll see a terminal running `post-create.sh`. When it finishes you'll see:
```
┌─────────────────────────────────────────────┐
│   Setup complete! Container is ready.        │
└─────────────────────────────────────────────┘
```

> Subsequent opens take only a few seconds — images are cached locally.

### Step 4 — Start the API
Open a terminal inside VS Code (`Ctrl+`` `) and run:
```bash
cd /workspace/NorthwindApi
dotnet run
```
Wait until you see:
```
Now listening on: http://[::]:5009
Application started.
```

### Step 5 — Start React
Click the **`+`** button in the terminal panel to open a second terminal:
```bash
cd /workspace/northwind-client
npm run dev
```
Wait until you see:
```
VITE v7.x.x  ready in xxx ms
➜  Local:   http://localhost:5173/
```

### Step 6 — Open in browser
Go to the **PORTS** tab in VS Code (bottom panel, next to TERMINAL) and click the globe icon next to port **5173**.

| URL | What you'll see |
|-----|-----------------|
| `http://localhost:5173` | React frontend — EF Core concept demos |
| `http://localhost:5009/swagger` | API Swagger UI — test endpoints directly |

---

## Project Structure

```
northwind-devcontainer/
  .devcontainer/
    devcontainer.json       ← VS Code dev container config (extensions, ports, post-create)
    docker-compose.yml      ← Defines two containers: app + SQL Server
    post-create.sh          ← Runs once on first start (installs tools, restores DB)
    .env.example            ← Template to override the SA password
  NorthwindApi/
    Controllers/            ← 12 controllers, one per EF Core concept
    Models/                 ← EF Core entity classes (Northwind tables)
    Data/                   ← DbContext (standard + lazy loading)
    DTOs/                   ← Data transfer objects
    Program.cs              ← App setup, CORS, SPA fallback
  northwind-client/
    src/pages/              ← 12 React pages, one per EF Core concept
    src/components/         ← Shared UI: DemoSection, DataTable, CodeSnippet
    src/layout/             ← Sidebar + MainLayout
  CLAUDE.md                 ← Full project context for Claude Code AI assistant
```

---

## EF Core Concepts Covered

| Concept | API Controller | React Page |
|---------|---------------|------------|
| Basic Queries | `BasicQueriesController` | Basic Queries |
| CRUD Operations | `CrudOperationsController` | CRUD Operations |
| Related Data | `RelatedDataController` | Related Data |
| Pagination | `PaginationController` | Pagination |
| Tracking | `TrackingController` | Change Tracking |
| Change Tracker | `ChangeTrackerController` | Change Tracker |
| Raw SQL | `RawSqlController` | Raw SQL |
| Stored Procedures | `StoredProceduresController` | Stored Procedures |
| Transactions | `TransactionsController` | Transactions |
| Bulk Operations | `BulkOperationsController` | Bulk Operations |
| Global Filters | `GlobalFiltersController` | Global Filters |
| Compiled Queries | `CompiledQueriesController` | Compiled Queries |

---

## Database

SQL Server 2022 runs as a sidecar container. The Northwind database is restored automatically on first start.

**Connection details (inside container):**
```
Server=db,1433;Database=Northwind;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
```

**Connect via VS Code mssql extension** (already installed in container):
- Server: `db,1433`
- Authentication: SQL Login
- User: `sa` / Password: `YourStrong!Passw0rd`
- Trust server certificate: checked
- Encrypt: Optional

**Data persists** between container restarts (named Docker volume).
To reset the database from scratch:
```bash
# Run on your Windows machine (not inside the container)
docker volume rm northwind-devcontainer_sqldata
# Then reopen in container — it will recreate everything
```

### Change the SA Password
```bash
cp .devcontainer/.env.example .devcontainer/.env
# Edit .env and set your own SA_PASSWORD
# Never commit .env to git — it's in .gitignore
```

---

## Troubleshooting

### "Reopen in Container" popup doesn't appear
Press `Ctrl+Shift+P` → type **Reopen in Container** → Enter

### API not reachable at localhost:5009
Use the **PORTS tab** in VS Code to open forwarded ports — do not type the URL manually in the browser until the port is listed there.
Navigate to `http://localhost:5009/swagger` (not the root `/`).

### Database connection error on first API call
The post-create setup may still be running. Check the terminal for the setup completion message, then restart `dotnet run`.

### Port already in use
Another process on your machine is using port 5009, 5173, or 1433.
Stop the conflicting process or change the port in `docker-compose.yml` and `launchSettings.json`.

---

## Tech Stack

| Component | Version |
|-----------|---------|
| .NET SDK | 8.0 |
| ASP.NET Core | 8.0 |
| Entity Framework Core | 8.x |
| Node.js | 20 LTS |
| React | 19 |
| Vite | 7 |
| TypeScript | 5.9 |
| SQL Server | 2022 (Linux) |
| Base container | Debian 12 (bookworm) |
