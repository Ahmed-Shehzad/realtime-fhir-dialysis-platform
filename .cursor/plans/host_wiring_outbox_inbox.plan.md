---
name: Host wiring — outbox, inbox, migrations
overview: Add DI extensions for JSON serializer and outbox/inbox options + interceptors, inbox entity and EF store with idempotent receive, unify PostgreSQL messaging DbContext, and ship a design-time schema DbContext with an initial EF migration.
todos:
  - id: inbox-entity-store
    content: InboxState entity, InboxStorageOptions, model config, EntityFrameworkInboxStore
    status: completed
  - id: messaging-dbcontext
    content: Rename/extend outbox DbContext for shared messaging schema; update interceptor
    status: completed
  - id: di-extensions
    content: BuildingBlocks DI AddRealtimePlatformMessagingPersistence etc.
    status: completed
  - id: persistence-migrations
    content: RealtimePlatform.Persistence + design-time factory + InitialCreate migration
    status: completed
  - id: docs-config
    content: appsettings example + ADR/plan note
    status: completed
isProject: true
---

# Host wiring — outbox & inbox

## Context

Iteration 1 delivered outbox entities and interceptor; hosts must register options and serializer. This iteration adds **inbox** idempotency persistence and **reference migrations** per ADR-0001 (PostgreSQL).

## Design

- **Single physical schema**: `outbox_messages` and `inbox_states` tables; `PostgreSqlMessagingDbContext` used by the interceptor for outbox rows (same connection + transaction as app `DbService`).
- **Application DbContext**: services call `OutboxModelConfiguration` and `InboxModelConfiguration` in `OnModelCreating` with `IOptions`-backed table/schema names, or inherit a shared base later.
- **Reference migration**: `RealtimePlatform.Persistence` holds `SchemaOnlyDbContext` + design-time factory + migrations for teams that want a SQL baseline without a service yet.

## Configuration keys (default)

Section `RealtimePlatform`:

- `OutboxPublisher` → `OutboxPublisherOptions`
- `OutboxStorage` → `OutboxStorageOptions`
- `InboxStorage` → `InboxStorageOptions`
