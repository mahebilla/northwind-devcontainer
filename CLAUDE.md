# CLAUDE.md — Northwind Dev Container

> **Scope**: Two Web APIs + React 19 (Vite) + SQL Server 2022 (Linux container).
> Angular and Blazor frontends are NOT part of this project.
> Original full project context is in `CLAUDE_BACKUP.md`.

---

## What This Project Is

A full-stack learning app using the Northwind database — two backend APIs side-by-side so you can compare patterns:

| Layer | Tech | Folder | Dev Port |
|-------|------|--------|----------|
| EF Core API | ASP.NET Core 8 + EF Core 8 | `NorthwindApi/` | `5009` |
| CQRS API | ASP.NET Core 8 + MediatR + Clean Architecture | `NorthwindCqrs/` | `5010` |
| Frontend | React 19 + TypeScript + Vite 7 | `northwind-client/` | `5173` |
| Write DB | SQL Server 2022 — `Northwind` (normalized) | — | `1433` |
| Read DB | SQL Server 2022 — `NorthwindRead` (denormalized) | — | `1433` |

Both APIs expose **identical routes** so the React frontend can target either one.

---

## NorthwindApi (port 5009) — EF Core patterns

Direct DbContext access. Each controller demonstrates a different EF Core concept.
Single database: `Northwind`.

## NorthwindCqrs (port 5010) — CQRS + Clean Architecture

MediatR dispatch through 4 layers: Domain → Application → Infrastructure → Api.

**Two databases, one SQL Server instance:**
- `Northwind` — Write DB (commands write here, normalized)
- `NorthwindRead` — Read DB (queries read here, denormalized projections, zero joins at query time)

**Architecture layers:**

```
NorthwindCqrs/
  NorthwindCqrs.Domain/          ← Plain C# entities, no EF attributes
  NorthwindCqrs.Application/     ← MediatR handlers, IWriteDbContext, IReadDbContext, ReadModels
  NorthwindCqrs.Infrastructure/  ← WriteDbContext (Northwind), ReadDbContext (NorthwindRead)
  NorthwindCqrs.Api/             ← Controllers inject IMediator only; Program.cs = composition root
```

**Phase 1 controllers (26 endpoints):**
- `BasicQueriesController` — 16 GET endpoints (Where, Select, OrderBy, Join, GroupBy, etc.)
- `CrudOperationsController` — 6 write + 1 read endpoint (Add, Update, Delete, Attach demo)
- `PaginationController` — 3 GET endpoints (Offset, Keyset, Orders paged)

**Read model sync pattern (commands):**
1. Write to `Northwind` via `WriteDbContext`
2. Sync projection to `NorthwindRead` via `ReadDbContext`

**`NorthwindRead` seeding:**
- First container start: `post-create.sh` step 5 creates the DB and runs `seed-read-db.sql`
- `seed-read-db.sql` does cross-database `INSERT SELECT` from `Northwind` tables (three-part names)
- To re-seed from scratch: `docker volume rm devconainersetup_sqldata`

---

## What Is a Dev Container?

A dev container is a Docker-based development environment defined in the `.devcontainer/` folder.
Instead of installing .NET, Node.js, and SQL tools on your machine, everything runs inside a Linux container.
VS Code connects to that container — you edit files, but builds and terminals run inside it.

**You only need two things installed on your Windows machine:**
1. Docker Desktop (WSL 2 backend)
2. VS Code + Dev Containers extension (`ms-vscode-remote.remote-containers`)

---

## Host Machine Prerequisites (One-Time Per New PC)

### 1. Docker Desktop
- Download: https://www.docker.com/products/docker-desktop/
- During install: choose **"Use WSL 2 backend"**
- Verify: `docker --version`

### 2. VS Code + Dev Containers Extension
- Download: https://code.visualstudio.com/
- Extension ID: `ms-vscode-remote.remote-containers`
- Install: `Ctrl+Shift+X` → search "Dev Containers" → Install

### 3. WSL 2 (if not installed)
```powershell
# Run in PowerShell as Administrator
wsl --install
# Restart when prompted, then verify:
wsl --version
```

---

## How to Open and Run

### Step 1 — Open in Dev Container
1. Open VS Code → **File → Open Folder** → select this folder
2. VS Code shows popup: **"Reopen in Container"** → click it
3. First time takes 5–10 minutes (downloads .NET 8 + SQL Server images)
4. `post-create.sh` runs automatically — installs tools, restores Northwind DB, creates NorthwindRead DB

### Step 2 — Start EF Core API (Terminal 1)
```bash
cd /workspace/NorthwindApi
dotnet run
```
Wait for: `Now listening on: http://[::]:5009`

### Step 3 — Start CQRS API (Terminal 2)
```bash
cd /workspace/NorthwindCqrs/NorthwindCqrs.Api
ASPNETCORE_URLS="http://+:5010" dotnet run
```
Wait for: `Now listening on: http://[::]:5010`

### Step 4 — Start React (Terminal 3)
```bash
cd /workspace/northwind-client
npm run dev
```
Wait for: `VITE ready`

### Step 5 — Open in Browser
| URL | What |
|-----|------|
| `http://localhost:5173` | React frontend |
| `http://localhost:5009/swagger` | EF Core API Swagger |
| `http://localhost:5010/swagger` | CQRS API Swagger |

> Use the **PORTS tab** in VS Code to open forwarded ports in your browser.

---

## Dev Container Structure

```
.devcontainer/
  devcontainer.json     → VS Code config: base image, extensions, port forwards (5009, 5010, 5173, 1433)
  docker-compose.yml    → Two containers: app (.NET + Node) + db (SQL Server 2022)
  post-create.sh        → Runs once on first start: installs tools, restores Northwind, creates NorthwindRead
  seed-read-db.sql      → Creates + populates NorthwindRead read model tables (run by post-create.sh)
  .env.example          → Copy to .env to override the SA password
```

### Containers

| Container | Image | Purpose |
|-----------|-------|---------|
| `app` | `mcr.microsoft.com/devcontainers/dotnet:1-8.0` | .NET 8 SDK + Node 20 |
| `db` | `mcr.microsoft.com/mssql/server:2022-latest` | SQL Server 2022 Linux |

### Ports Forwarded to Windows

| Port | Service |
|------|---------|
| `5009` | NorthwindApi (EF Core) |
| `5010` | NorthwindCqrs (CQRS) |
| `5173` | React / Vite dev server |
| `1433` | SQL Server (for Azure Data Studio / mssql extension) |

---

## Databases

The dev container uses SQL Server 2022 on Linux (not Windows SQL Express).
`post-create.sh` creates and populates both databases automatically on first start.

### Northwind (Write DB)
```
Server=db,1433;Database=Northwind;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
```

### NorthwindRead (Read DB — CQRS projections)
```
Server=db,1433;Database=NorthwindRead;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
```

**Connect via VS Code mssql extension:**
- Server: `db,1433`
- Authentication: SQL Login
- User: `sa`
- Password: `YourStrong!Passw0rd`
- Trust server certificate: checked
- Encrypt: Optional

**Data persists** between container restarts via a named Docker volume (`sqldata`).
To reset everything: `docker volume rm devconainersetup_sqldata`

### Change the SA Password
```bash
cp .devcontainer/.env.example .devcontainer/.env
# Edit SA_PASSWORD in .env — never commit .env to git
```

---

## Key Config Files

| File | Purpose |
|------|---------|
| `NorthwindApi/Properties/launchSettings.json` | `applicationUrl: http://+:5009` — binds to all interfaces |
| `NorthwindCqrs/NorthwindCqrs.Api/Properties/launchSettings.json` | `applicationUrl: http://+:5010` |
| `NorthwindCqrs/NorthwindCqrs.Api/appsettings.Development.json` | Dev container connection strings for CQRS |
| `northwind-client/vite.config.ts` | `host: true` — binds Vite to all interfaces |
| `.devcontainer/docker-compose.yml` | Injects connection strings as env vars into the app container |
| `.devcontainer/seed-read-db.sql` | Creates NorthwindRead tables + cross-database seed from Northwind |

**Connection string priority for NorthwindCqrs:**
`appsettings.Development.json` (committed, has `Server=db,1433`) overrides `appsettings.json` (Windows fallback).
After a container rebuild, docker-compose env vars also inject the correct strings.

---

## Azure Deployment (Production)

Production uses **code deployment** (not containers) to Azure App Service F1 Free tier.
The dev container is a local development tool only — Azure runs the compiled app directly.
Only `NorthwindApi` is deployed to Azure. `NorthwindCqrs` is dev-container-only for now.

### Existing Azure Resources

| Resource | Name | Cost |
|----------|------|------|
| Resource Group | `rg-efcore-app` (East Asia) | $0 |
| SQL Server | `efcore-nw-sql-mahi` | $0 |
| SQL Database | `Northwind` (free serverless) | $0 |
| App Service Plan | `plan-efcore-app` (F1 Free) | $0 |
| Web App | `efcore-northwind-app-mahi` (.NET 8 Linux) | $0 |

**Production URL**: `https://efcore-northwind-app-mahi.azurewebsites.net`

### CI/CD — GitHub Actions
```
dev branch → Pull Request → main → GitHub Actions → Azure App Service
```
Workflow: `.github/workflows/deploy.yml`
Required secret: `AZURE_CREDENTIALS` (set in GitHub repo Settings → Secrets)

---

## Git Identity (Shared Machine)

Identity auto-applied via `includeIf` rule — no manual setup needed per repo.
- Name: `mahebilla`
- Email: `mahendran.apec@gmail.com`

Verify: `git config user.name && git config user.email`

---

## Notes for Claude

- **Do NOT modify existing source code** unless explicitly asked
- Both APIs have identical routes — changes to one do not automatically apply to the other
- `NorthwindCqrs` uses `.slnx` solution format (dotnet 10 CLI default), not `.sln`
- Connection strings for CQRS: `appsettings.Development.json` (not env var, not `appsettings.json`)
- `ASPNETCORE_URLS` in docker-compose is set to `http://+:5009` — always override for CQRS: `ASPNETCORE_URLS="http://+:5010" dotnet run`
- `NorthwindRead` DB: only created on first `post-create.sh` run; if container already running, create manually or rebuild container
- `az login` is always interactive (browser) — cannot be automated
- Full original project context (all 3 frontends) is in `CLAUDE_BACKUP.md`
