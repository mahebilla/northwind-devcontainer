# CLAUDE.md — Northwind EF Core App (Dev Container)

> **Scope**: Web API (ASP.NET Core 8 + EF Core 8) + React 19 (Vite) + SQL Server 2022 (Linux container).
> Angular and Blazor frontends are NOT part of this project.
> Original full project context is in `CLAUDE_BACKUP.md`.

---

## What This Project Is

A full-stack EF Core learning app using the Northwind database.
Each controller demonstrates a different EF Core concept with a matching React page.

| Layer | Tech | Folder | Dev Port |
|-------|------|--------|----------|
| Backend API | ASP.NET Core 8 + EF Core 8 | `NorthwindApi/` | `5009` |
| Frontend | React 19 + TypeScript + Vite 7 | `northwind-client/` | `5173` |
| Database | SQL Server 2022 (Linux sidecar) | — | `1433` |

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
4. `post-create.sh` runs automatically — installs tools and restores the Northwind database

### Step 2 — Start the API (Terminal 1)
```bash
cd /workspace/NorthwindApi
dotnet run
```
Wait for: `Now listening on: http://[::]:5009`

### Step 3 — Start React (Terminal 2)
```bash
cd /workspace/northwind-client
npm run dev
```
Wait for: `VITE ready`

### Step 4 — Open in Browser
| URL | What |
|-----|------|
| `http://localhost:5173` | React frontend |
| `http://localhost:5009/swagger` | API Swagger UI |

> Use the **PORTS tab** in VS Code to open forwarded ports in your browser.

---

## Dev Container Structure

```
.devcontainer/
  devcontainer.json     → VS Code config: base image, extensions, port forwards
  docker-compose.yml    → Two containers: app (.NET + Node) + db (SQL Server 2022)
  post-create.sh        → Runs once on first start: installs tools + restores Northwind DB
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
| `5009` | ASP.NET Core API |
| `5173` | React / Vite dev server |
| `1433` | SQL Server (for Azure Data Studio / mssql extension) |

---

## Database

The dev container uses SQL Server 2022 on Linux (not Windows SQL Express).
The `post-create.sh` script creates and populates the Northwind database automatically on first start.

**Connection details (inside container):**
```
Server=db,1433;Database=Northwind;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
```

**Connect via VS Code mssql extension:**
- Server: `db,1433`
- Authentication: SQL Login
- User: `sa`
- Password: `YourStrong!Passw0rd`
- Trust server certificate: checked
- Encrypt: Optional

**Data persists** between container restarts via a named Docker volume (`sqldata`).
To reset the database: `docker volume rm devconainersetup_sqldata`

### Change the SA Password
```bash
cp .devcontainer/.env.example .devcontainer/.env
# Edit SA_PASSWORD in .env — never commit .env to git
```

---

## Key Config Changes Made for Dev Container

These files were modified from the original to work inside the container:

| File | Change | Reason |
|------|--------|--------|
| `NorthwindApi/Properties/launchSettings.json` | `applicationUrl: http://+:5009` | Bind to all interfaces so VS Code port forwarding works |
| `northwind-client/vite.config.ts` | Added `host: true` | Bind to all interfaces so VS Code port forwarding works |

The connection string is injected via environment variable in `docker-compose.yml` — no changes to `appsettings.json`.

---

## Azure Deployment (Production)

Production uses **code deployment** (not containers) to Azure App Service F1 Free tier.
The dev container is a local development tool only — Azure runs the compiled app directly.

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
- Files intentionally changed from original: `launchSettings.json` (API binding) and `vite.config.ts` (Vite binding)
- Connection string override is via env var in `docker-compose.yml` — no `appsettings.json` changes
- `az login` is always interactive (browser) — cannot be automated
- Dev container work = `.devcontainer/` folder only
- Full original project context (all 3 frontends) is in `CLAUDE_BACKUP.md`
