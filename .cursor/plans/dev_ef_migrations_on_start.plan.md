---
name: Dev EF migrations on start
overview: Apply pending EF Core migrations automatically when an API host runs in Development, immediately after WebApplication Build.
todos:
  - id: extension
    content: Add WebApplication extension ApplyPendingMigrationsInDevelopmentAsync<TDbContext> in BuildingBlocks
    status: completed
  - id: wire-apis
    content: Call extension from each platform API that registers a domain DbContext (exclude RealtimeDelivery)
    status: completed
  - id: build
    content: dotnet build solution
    status: completed
isProject: true
---

## Design

- **When:** `IWebHostEnvironment.IsDevelopment()` only (no automatic migrate in Staging/Production).
- **What:** `Database.MigrateAsync()` — applies pending migrations only; no-op when DB is current.
- **Where:** Right after `WebApplicationBuilder.Build()`, before HTTP middleware, via a scoped `TDbContext` from DI.

## Files

- New: `BuildingBlocks/Persistence/EfCoreDevelopmentMigrationExtensions.cs`
- Update: each `platform/services/*/*.Api/Program.cs` that calls `AddDbContext<>` for the service database (16 services).

## Out of scope

- `RealtimeDelivery.Api` (no EF `DbContext`).
- `RealtimePlatform.ApiGateway` (no EF).

```mermaid
sequenceDiagram
  participant Host as API Host
  participant DI as DI scope
  participant DB as Postgres
  Host->>Host: Build()
  alt Development
    Host->>DI: CreateAsyncScope + TDbContext
    DI->>DB: MigrateAsync (pending only)
  end
  Host->>Host: Use* / RunAsync
```
