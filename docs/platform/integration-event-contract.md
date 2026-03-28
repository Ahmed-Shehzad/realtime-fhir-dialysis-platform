# Integration event contract (baseline)

## Authoritative inventory

**Before adding or renaming an integration event in code**, align with [Integration Event Catalog](../../.cursor/plans/integration_event_catalog.md): that document is the **single source of truth** for event names, bounded-context grouping, tiers (§6), saga-critical sets (§3), and external exposure (§4). Governance rules live in [integration events — plan considerations](../../.cursor/plans/integration_events_plan_considerations.plan.md). Service-to-context mapping: [bounded-context-catalog.md](bounded-context-catalog.md). Automated Tier 1 transport checks: [`tests/RealtimePlatform.ContractTests`](../../tests/RealtimePlatform.ContractTests).

Tier 1 (§6) shared CLR contracts are implemented under [`RealtimePlatform.IntegrationEventCatalog`](../../platform/shared/RealtimePlatform.IntegrationEventCatalog/), referencing this contract for envelope fields. Domain-specific events not yet promoted to the catalog assembly remain in their service `*.Domain` projects until they appear in the catalog.

---

Cross-service integration events MUST include the following metadata (see `realtime_fhir_dialysis_implementation_plan.md` §11.3). Implementations should use a common envelope or base record (for example `BuildingBlocks.IntegrationEvent`) so these fields are always populated.

The canonical **on-the-wire JSON** shape is defined in [Integration Event Catalog §1](../../.cursor/plans/integration_event_catalog.md) (envelope fields + nested `payload`). For types inheriting `IntegrationEvent`, outbox and transport bytes are produced by [`IntegrationEventTransportSerializer`](../../BuildingBlocks/IntegrationEvents/IntegrationEventTransportSerializer.cs) (catalog-style envelope, not a flat CLR record).

| Field | Purpose |
|--------|--------|
| Message id | Unique idempotency key (for example `EventId` as `Ulid`) |
| Message type | CLR or contract name + version |
| Version | Schema or contract version for forward compatibility |
| Correlation id | Distributed tracing / request chain |
| Causation id | Optional upstream message that caused this event |
| Produced timestamp | UTC time of publication |
| Tenant / site context | Multi-tenant isolation (`TenantId`, facility, etc.) |
| Session id | When the event belongs to a treatment session |
| Patient id | Only when required and policy allows |
| Device id | When device-scoped |
| Validation state | Last known validation outcome when relevant |
| Payload | Serialized body (`IMessageSerializer`; default JSON) |

Inbound consumers MUST treat processing as idempotent using the message id (inbox pattern). Host and EF setup: [`host-configuration.md`](host-configuration.md). Saga step-to-event mapping: [`integration-event-saga-map.md`](integration-event-saga-map.md).
