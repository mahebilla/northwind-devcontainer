# CLAUDE.md — Project Context for Claude Code

## Project Summary

Full-stack **EF Core learning app** using the Northwind database. Each EF Core concept has a matching API controller + frontend page. **Three frontend options** — React, Angular, and Blazor — run independently against the same API.

- **Backend**: ASP.NET Core 8 Web API + Entity Framework Core 8 + SQL Server (`.\\SQLEXPRESS`)
- **Frontend (React)**: React 19 + TypeScript + Vite 7 + React Router 7 → `northwind-client/`
- **Frontend (Angular)**: Angular 19 + TypeScript + Standalone Components + Signals → `northwind-angular/`
- **Frontend (Blazor)**: Blazor WebAssembly .NET 8 + Razor Components + CSS Isolation → `northwind-blazor/`
- **Database**: Northwind on SQL Server Express (local dev) / Azure SQL Free tier (production)
- **Azure Deployment**: App Service F1 (Free) + Azure SQL Serverless (Free offer)
- **CI/CD**: GitHub Actions → auto-deploy to Azure on PR merge to `main`

## Folder Structure

```
.github/workflows/
  deploy.yml                → GitHub Actions CI/CD (auto-deploy to Azure on PR merge)

NorthwindApi/               → .NET 8 Web API
  Program.cs                → Entry point, DI, middleware, CORS, SPA fallback
  Models/                   → EF Core entity classes (27 files, tables + views)
  Data/                     → DbContext classes
    NorthwindContext.cs      → Primary context (standard tracking)
    NorthwindLazyContext.cs  → Context with lazy loading proxies
  DTOs/                     → Data transfer objects
  Controllers/              → 12 API controllers, one per EF Core concept

northwind-client/           → React 19 SPA (Vite) — port 5173
  src/
    main.tsx                → Root render with BrowserRouter
    App.tsx                 → Route definitions (12 routes)
    layout/                 → MainLayout.tsx, Sidebar.tsx
    components/             → DemoSection.tsx, DataTable.tsx, CodeSnippet.tsx
    pages/                  → 12 page files, one per EF Core concept

northwind-angular/          → Angular 19 SPA (CLI) — port 4200
  src/app/
    app.component.ts        → Root component (MainLayout + RouterOutlet)
    app.config.ts           → Providers (HttpClient, Router, Zone)
    app.routes.ts           → 12 lazy-loaded routes
    models/                 → EndpointDemo interface
    services/               → ApiService (HttpClient wrapper)
    layout/                 → main-layout/, sidebar/
    components/             → demo-section/, data-table/, code-snippet/
    pages/                  → 12 page folders, one per EF Core concept
  proxy.conf.json           → Dev proxy: /api → localhost:5009
  ANGULAR_CONCEPTS.md       → Angular learning guide with React comparisons

northwind-blazor/           → Blazor WebAssembly .NET 8 — port 5200
  Program.cs                → Entry point, HttpClient DI, ApiService registration
  App.razor                 → Router component (auto-discovers @page routes)
  _Imports.razor            → Global @using directives
  Models/
    EndpointDemo.cs         → C# record (same shape as TS interface)
  Services/
    ApiService.cs           → HttpClient wrapper returning JsonElement
  Layout/
    MainLayout.razor/.css   → Sidebar + @Body layout (CSS isolation)
    NavMenu.razor/.css      → Dark sidebar nav with NavLink
  Shared/
    DemoSection.razor/.css  → Core demo runner (state, API calls, results)
    DataTable.razor/.css    → Generic table from JsonElement arrays
    CodeSnippet.razor/.css  → Dark-theme code block
  Pages/                    → 12 page components + Home redirect
  wwwroot/
    css/app.css             → Global styles
    index.html              → SPA entry point (loads Blazor WASM runtime)
  BLAZOR_CONCEPTS.md        → Blazor learning guide with React comparisons
```

## Key Conventions

### Adding a New EF Core Concept

Every new concept requires changes in **4 places** (API + all three frontends):

#### 1. API Controller → `NorthwindApi/Controllers/{ConceptName}Controller.cs`
- Namespace: `NorthwindApi.Controllers`
- Route: `[Route("api/[controller]")]` → becomes `/api/conceptname`
- Inject `NorthwindContext` (or `NorthwindLazyContext` for lazy loading demos)
- Each action returns: `Ok(new { Method, Description, Data })`

#### 2. React Frontend (4 files):
- **Page** → `northwind-client/src/pages/{ConceptName}.tsx`
  - Define `demos: EndpointDemo[]` array, render `<DemoSection>`
- **Route** → `northwind-client/src/App.tsx` — add `<Route>` + import
- **Sidebar** → `northwind-client/src/layout/Sidebar.tsx` — add to `navItems`

#### 3. Angular Frontend (3 files):
- **Page** → `northwind-angular/src/app/pages/{concept-name}/{concept-name}.component.ts`
  - Define `demos: EndpointDemo[]` array, render `<app-demo-section>`
- **Route** → `northwind-angular/src/app/app.routes.ts` — add `loadComponent` entry
- **Sidebar** → `northwind-angular/src/app/layout/sidebar/sidebar.component.ts` — add to `navItems`

#### 4. Blazor Frontend (2 files):
- **Page** → `northwind-blazor/Pages/{ConceptName}.razor`
  - Add `@page "/route"` directive, define `List<EndpointDemo> demos`, render `<DemoSection>`
- **Sidebar** → `northwind-blazor/Layout/NavMenu.razor` — add to `navItems`
  - No separate route file needed — Blazor discovers routes from `@page` directives

### API Response Pattern

All controllers return a consistent shape:
```json
{ "method": "MethodName()", "description": "What this demonstrates", "data": [...] }
```

### DemoSection Component Pattern (shared across all three frontends)

The `EndpointDemo` interface drives all pages:
```ts
{ name: string, endpoint: string, method: 'GET'|'POST'|'PUT'|'DELETE', code: string, description: string, body?: unknown }
```
- Each demo gets a "Run" button that calls the endpoint
- Results render as DataTable (arrays) or JSON (objects)
- React uses axios; Angular uses HttpClient + Observables; Blazor uses HttpClient + async/await

## Existing EF Core Concepts (12 total)

| Controller                  | Route Prefix                 | React Page           | Angular Page                  | Blazor Page              | Concept                          |
|-----------------------------|------------------------------|----------------------|-------------------------------|--------------------------|----------------------------------|
| BasicQueriesController      | /api/basicqueries            | BasicQueries.tsx     | basic-queries.component.ts    | BasicQueries.razor       | Where, Find, First, Count, GroupBy, Join, TagWith |
| RawSqlController            | /api/rawsql                  | RawSql.tsx           | raw-sql.component.ts          | RawSql.razor             | FromSql, FromSqlRaw, SqlQuery, ExecuteSql |
| RelatedDataController       | /api/relateddata             | RelatedData.tsx      | related-data.component.ts     | RelatedData.razor        | Include, ThenInclude, Explicit, Lazy Loading |
| TrackingController          | /api/tracking                | Tracking.tsx         | tracking.component.ts         | Tracking.razor           | AsNoTracking, IdentityResolution |
| CrudOperationsController    | /api/crudoperations          | CrudOperations.tsx   | crud-operations.component.ts  | CrudOperations.razor     | Add, Update, Remove, SaveChanges |
| BulkOperationsController    | /api/bulkoperations          | BulkOperations.tsx   | bulk-operations.component.ts  | BulkOperations.razor     | ExecuteUpdate, ExecuteDelete     |
| TransactionsController      | /api/transactions            | Transactions.tsx     | transactions.component.ts     | Transactions.razor       | BeginTransaction, Savepoints     |
| CompiledQueriesController   | /api/compiledqueries         | CompiledQueries.tsx  | compiled-queries.component.ts | CompiledQueries.razor    | CompileQuery, CompileAsyncQuery  |
| GlobalFiltersController     | /api/globalfilters           | GlobalFilters.tsx    | global-filters.component.ts   | GlobalFilters.razor      | HasQueryFilter, IgnoreQueryFilters |
| ChangeTrackerController     | /api/changetracker           | ChangeTracker.tsx    | change-tracker.component.ts   | ChangeTracker.razor      | Entries, EntityState, OriginalValues |
| StoredProceduresController  | /api/storedprocedures        | StoredProcedures.tsx | stored-procedures.component.ts| StoredProcedures.razor   | FromSqlRaw with stored procs     |
| PaginationController        | /api/pagination              | Pagination.tsx       | pagination.component.ts       | Pagination.razor         | Skip/Take, Keyset pagination     |

## Dev Setup (Local)

- **API port**: `http://localhost:5009`
- **React client**: `http://localhost:5173` (Vite proxy → API)
- **Angular client**: `http://localhost:4200` (proxy.conf.json → API)
- **Blazor client**: `http://localhost:5200` (direct HTTP + CORS)
- **CORS**: Enabled in dev for `http://localhost:5173` and `http://localhost:5200`
- **DB connection**: `Server=.\SQLEXPRESS;Database=Northwind;Trusted_Connection=True;TrustServerCertificate=True;`
- **All three frontends** run independently — just pick one and start it alongside the API

## Azure Deployment (Production)

### Architecture
```
[Azure App Service F1 Free]  ←→  [Azure SQL Free Serverless]
  NorthwindApi + React SPA          Northwind DB
  (Linux, .NET 8)                   (Gen5, auto-pause)
```

### Azure Resources
| Resource | Name | SKU | Cost |
|----------|------|-----|------|
| Resource Group | `rg-efcore-app` (East Asia) | — | — |
| SQL Server | `efcore-nw-sql-mahi` | — | $0 |
| SQL Database | `Northwind` | Free serverless (100K vCore sec/mo, 32GB) | $0 |
| App Service Plan | `plan-efcore-app` | F1 Free (Linux) | $0 |
| Web App | `efcore-northwind-app-mahi` | .NET 8 | $0 |

### Production URL
- **Live app**: `https://efcore-northwind-app-mahi.azurewebsites.net`
- First load is slow (~10-30s) — free tier cold start + SQL auto-pause wake-up

### How Production Differs from Dev
- **SPA serving**: In dev, React is served from `../northwind-client/dist` via Vite. In production, `dotnet publish` copies React build to `wwwroot/` (via `PublishReactApp` MSBuild target in `.csproj`), and `Program.cs` falls back to `WebRootFileProvider`
- **Connection string**: Azure App Service overrides `ConnectionStrings:NorthwindConnection` via environment variable (set with `az webapp config connection-string set`). No changes to `appsettings.json` needed
- **CORS**: Not needed in production — React SPA and API are served from the same origin
- **Swagger**: Only enabled in Development environment (already configured)

### Manual Deploy Commands (if needed)
```bash
# Build (from project root)
cd NorthwindApi
dotnet publish -c Release -o ./publish

# Zip and deploy
cd publish
tar -acf ../deploy.zip *
cd ..
az webapp deployment source config-zip \
  --name efcore-northwind-app-mahi \
  --resource-group rg-efcore-app \
  --src deploy.zip
```
> Note: Manual deploy is rarely needed — CI/CD handles this automatically on PR merge.

## CI/CD — GitHub Actions

### Branch Strategy
```
dev (daily work) → Pull Request → main (production) → GitHub Actions → Azure
```
- **`dev`**: Default branch for development. Push freely here.
- **`main`**: Protected branch. Only updated via Pull Requests from `dev`.
- **Trigger**: GitHub Actions workflow runs automatically when a PR is merged into `main`.

### How It Works
1. Developer pushes code to `dev` branch
2. Creates Pull Request: `dev` → `main`
3. Merge the PR → triggers GitHub Actions workflow
4. Workflow builds .NET API + React SPA, deploys to Azure App Service
5. Live at `https://efcore-northwind-app-mahi.azurewebsites.net`

### Workflow File: `.github/workflows/deploy.yml`
The workflow does the following on every push to `main`:
1. **Checkout** code from repository
2. **Setup Node.js 20** (for React build) with npm cache
3. **Setup .NET 8 SDK** (for API build)
4. **Publish** — runs `dotnet publish` which triggers the `PublishReactApp` MSBuild target in `.csproj` (auto-runs `npm install && npm run build`, copies React dist to `wwwroot/`)
5. **Login to Azure** — uses service principal credentials stored in GitHub Secrets
6. **Deploy** — pushes the publish output to Azure App Service

### GitHub Secrets Required
| Secret Name | Description | How to Get |
|-------------|-------------|------------|
| `AZURE_CREDENTIALS` | Azure service principal JSON | `az ad sp create-for-rbac --name "github-deploy" --role contributor --scopes /subscriptions/{sub-id}/resourceGroups/rg-efcore-app --sdk-auth` |

### Authentication Method
Uses **Azure Service Principal** (`azure/login@v2` action) instead of Publish Profile:
- Service principal is scoped to the `rg-efcore-app` resource group only (least privilege)
- Created via `az ad sp create-for-rbac` with `contributor` role
- JSON credentials stored as `AZURE_CREDENTIALS` GitHub Secret (encrypted, never exposed in logs)
- Public users cannot see secrets even on public repos

### Cost
- **Public repos**: Unlimited GitHub Actions minutes — $0
- **Private repos**: 2,000 minutes/month free tier — $0 for this project

### Daily Workflow
```bash
# 1. Work on dev branch
git checkout dev
# ... make changes ...
git add . && git commit -m "your changes"
git push origin dev

# 2. Create PR on GitHub: dev → main
# 3. Merge PR → auto-deploys to Azure (~2-3 min)
# 4. Monitor: GitHub repo → Actions tab
```

### Troubleshooting CI/CD
- **View logs**: GitHub repo → Actions tab → click failed run → expand failed step
- **Re-run failed job**: Actions tab → click failed run → "Re-run all jobs"
- **Update Azure credentials**: Re-run `az ad sp create-for-rbac` command, update `AZURE_CREDENTIALS` secret

### Free Tier Limits
- **App Service F1**: 60 min CPU/day, 1GB RAM, 165 MiB data out/day, sleeps after ~20 min idle
- **Azure SQL Free**: 100K vCore seconds/month, 32GB storage, auto-pauses after 1 hour idle
- Monitor usage: Azure Portal → App Services → Quotas / SQL databases → Overview

### Cleanup (delete everything)
```bash
az group delete --name rg-efcore-app --yes --no-wait
```

## Build & Run

```bash
# API (always needed)
cd NorthwindApi && dotnet run

# React frontend (terminal 2)
cd northwind-client && npm run dev

# OR Angular frontend (terminal 2)
cd northwind-angular && npm start

# OR Blazor frontend (terminal 2)
cd northwind-blazor && dotnet run
```

## Angular-Specific Patterns

- **Standalone components** (no NgModules) — Angular 19 default
- **Signals** for reactive state (`signal()`, `.update()`) — replaces RxJS `BehaviorSubject` for component state
- **inject()** function for DI — modern alternative to constructor injection
- **@for / @if** control flow — Angular 19 built-in (replaces `*ngFor`, `*ngIf`)
- **loadComponent** lazy loading — each page is a separate chunk
- **Component-scoped styles** — sidebar uses `styles[]` instead of inline
- **<ng-content>** for content projection — like React's `{children}`
- Full learning docs: `northwind-angular/ANGULAR_CONCEPTS.md`

## Blazor-Specific Patterns

- **Razor Components** (.razor files) — Blazor's equivalent of React function components
- **@code blocks** — C# code (fields, methods) inside components, like function body in React
- **[Parameter]** for props — like React props or Angular @Input()
- **@inject** for DI — services injected into components (like Angular's inject())
- **CSS Isolation** (.razor.css) — scoped styles per component (like CSS Modules)
- **@page routing** — decentralized routes via directives on each component
- **NavLink** — auto-applies "active" CSS class on route match
- **async/await with Task<T>** — same pattern as React (no Observables)
- **JsonElement** — dynamic JSON handling (avoids needing 50+ DTO classes)
- **No proxy** — Blazor WASM calls API directly via HttpClient base address + CORS
- Full learning docs: `northwind-blazor/BLAZOR_CONCEPTS.md`

## Important Notes

- `ReferenceHandler.IgnoreCycles` is configured in `Program.cs` (Northwind has circular nav properties)
- `JsonIgnoreCondition.WhenWritingNull` keeps responses clean
- Two DbContexts registered: standard + lazy-loading (for RelatedData demos)
- Many controllers use `.IgnoreQueryFilters()` because `GlobalFiltersController` sets a global filter on Products
- The React SPA is served as static files via SPA fallback in `Program.cs` (dev: `../northwind-client/dist`, production: `wwwroot/`)
- Inline styles used throughout React; Angular sidebar uses component-scoped CSS
- All Angular source code has inline comments explaining concepts with React comparisons
- All Blazor source code has inline comments explaining concepts with React comparisons
