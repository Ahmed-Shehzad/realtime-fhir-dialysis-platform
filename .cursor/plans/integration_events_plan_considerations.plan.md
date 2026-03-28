---
name: Integration events — plan considerations
overview: Codifies how domain events relate to integration events, ties all feature work to the integration event catalog and envelope standard, and lists implementation alignment tasks for outbox, versioning, and saga-critical flows.
todos:
  - id: catalog-link
    content: Treat integration_event_catalog.md as authoritative inventory for new/changed integration events
    status: completed
  - id: envelope-alignment
    content: Align serialization/envelope (eventVersion, causationId, workflow/saga ids, facility context) with catalog §1
    status: completed
  - id: tier1-events
    content: Implement Tier 1 catalog events before deep analytics/reporting (see catalog §6)
    status: completed
  - id: saga-mapping
    content: Map each new saga step to explicit catalog events (catalog §3) and failure/compensation events
    status: completed
isProject: true
---

# Integration events — plan considerations

## Authoritative catalog

All **integration event names, grouping by bounded context, saga-critical sets, external exposure sets, and implementation tiers** are defined in:

`[integration_event_catalog.md](integration_event_catalog.md)`

Any new service, saga, or cross-context flow in this repo **must** be checked against that catalog before implementation: add or reuse an event there first, then implement in code.

## Domain events vs integration events (planning rule)


| Concern     | Domain events                                                          | Integration events                                                                                                     |
| ----------- | ---------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| Scope       | Within one bounded context; internal cohesion.                         | Cross-context, broker, and **external** consumers.                                                                     |
| Contract    | May reflect internal model.                                            | **Stable, versioned, immutable business facts**; do not leak internal domain logic (catalog §5).                       |
| Consistency | Same transaction as aggregate changes; dispatch pattern as documented. | **Outbox** in the **same DB transaction** as committed state; then async publish + **inbox** idempotency at consumers. |


**Planning implication:** When a use case is planned, explicitly list (1) domain events raised on aggregates, (2) which catalog integration events must be emitted and when, and (3) which sagas or external systems consume them.

## Transaction and external consistency

- **Local atomicity:** Integration events that must reflect a commit must be staged via the existing outbox pattern with the application `DbContext` transaction (see `[docs/platform/host-configuration.md](../../docs/platform/host-configuration.md)`).
- **Cross-service:** No distributed transactions; orchestration uses **versioned integration events** and **idempotent** handlers (saga steps in catalog §3).
- **Failure:** Catalog §3 lists failure/compensation-related events; plans for new workflows must include which of these apply and how operators/audit see them.

## Envelope standard (catalog §1)

Future implementation work should converge typed payloads and headers/metadata toward the catalog envelope fields (`eventId`, `eventType`, `eventVersion`, `occurredUtc`, `correlationId`, `causationId`, optional `workflowId` / `sagaId`, tenant/session/patient/device/`partitionKey`, `payload`).

**Current code** (`BuildingBlocks.IntegrationEvent`, `IMessageSerializer`) is a subset; planning for features should note **envelope gaps** (e.g. missing `causationId`, `eventVersion`) and schedule them before widening external consumption.

## Priority for roadmaps

When prioritizing milestones (e.g. Measurement Acquisition, Session, FHIR, Surveillance), use **catalog §6 Tier 1** as the default sequencing spine before Tier 2/3 work unless an ADR documents a deliberate exception.

## External exposure (catalog §4)

Planned features that touch **EHR/FHIR**, **operations/monitoring**, or **compliance/audit** must use or extend the events listed in §4 so contracts stay coherent across teams and integrators.

## Alignment with existing docs

- `[docs/platform/integration-event-contract.md](../../docs/platform/integration-event-contract.md)` — field checklist; keep in sync with catalog §1 as the envelope evolves.
- `[realtime_fhir_dialysis_implementation_plan.md](realtime_fhir_dialysis_implementation_plan.md)` — milestones and contexts; integration event names should match the catalog.

## Summary

**Integration events are planning artifacts first:** the catalog plus this document define what cross-boundary facts exist, how they participate in consistency (outbox/inbox/sagas), and in what order they are built. Feature plans should reference the catalog by name and tier; implementation follows without inventing parallel event vocabularies.
