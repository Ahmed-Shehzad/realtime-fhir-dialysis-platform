# ADR-0001: Persistence stack — PostgreSQL, Redis, EF Core

## Status

Accepted

## Context

The real-time FHIR dialysis implementation plan ([`realtime_fhir_dialysis_implementation_plan.md`](../../.cursor/plans/realtime_fhir_dialysis_implementation_plan.md)) names **Azure SQL** among Azure platform services for service-owned transactional state.

Workspace rules ([`data-persistence.mdc`](../../.cursor/rules/data-persistence.mdc)) specify **Redis**, **EF Core**, and **PostgreSQL** (cache-aside, ORM, primary transactional store).

## Decision

For **this repository**, the **source of truth** for application persistence is:

- **PostgreSQL** — primary transactional database per service (or per tenant, per multi-tenancy rules).
- **EF Core** — object-relational mapping and migrations against PostgreSQL.
- **Redis** — caching (cache-aside and related patterns per `data-persistence.mdc`).

**Azure SQL** in the large program plan is **not** the target database for implementations in this repo unless a future ADR revokes this decision.

## Consequences

- Service templates, connection configuration, local development database setup, and tests should assume **PostgreSQL** and **Redis**, not SQL Server / Azure SQL.
- Documentation, IaC, and runbooks for this codebase should describe the Postgres + Redis topology; Azure landing-zone diagrams may still show other Azure services (FHIR, Service Bus, etc.) without requiring Azure SQL here.
- Any need to move to Azure SQL (e.g. managed instance) would require a new ADR and explicit migration strategy.
- **Redis host wiring** for cache-aside (and optional future SignalR backplane) is implemented in [`RealtimePlatform.Redis`](../../platform/shared/RealtimePlatform.Redis/) and documented in [`host-configuration.md`](../platform/host-configuration.md) (`RealtimePlatform:Redis`). APIs call `AddRealtimePlatformRedis` / `AddRealtimePlatformRedisHealthCheck` when enabling Redis.

## .NET SDK

Development and CI use **.NET SDK 10**, pinned via repo root [`global.json`](../../global.json).

## Shared implementation (Iteration 1)

Transactional outbox for PostgreSQL is implemented in [`RealtimePlatform.Outbox`](../../platform/shared/RealtimePlatform.Outbox/) and wired from [`BuildingBlocks` `IntegrationEventDispatchInterceptor`](../../BuildingBlocks/Interceptors/IntegrationEventDispatchInterceptor.cs). Application hosts must register `IOptions<OutboxPublisherOptions>`, `IOptions<OutboxStorageOptions>`, and `IOptions<InboxStorageOptions>` (see [`docs/platform/host-configuration.md`](../platform/host-configuration.md)); map `outbox_messages` and `inbox_states` via `OutboxModelConfiguration` / `InboxModelConfiguration` on the app `DbContext`, or apply migrations from [`RealtimePlatform.Persistence`](../../platform/shared/RealtimePlatform.Persistence/).
