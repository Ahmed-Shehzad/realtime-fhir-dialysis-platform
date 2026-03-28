---
name: Iteration 19 Contract tests and bounded context catalog
overview: Add automated transport serialization checks for all Tier 1 integration event CLR types and a bounded context catalog document for operators and implementers.
todos:
  - id: i19-plan
    content: Plan file (this) + slnx + blueprint Iteration 19 section
    status: completed
  - id: i19-contract-tests
    content: RealtimePlatform.ContractTests — reflect Tier1 types, IntegrationEventTransportSerializer
    status: completed
  - id: i19-docs
    content: docs/platform/bounded-context-catalog.md
    status: completed
isProject: true
---

## Rationale

Roadmap Iterations 1–18 are implemented; section **20.3 Contract tests** and Milestone 0 **bounded context catalog** are the next durable artifacts.

## Scope

- **Contract tests**: every concrete `IntegrationEvent` in `RealtimePlatform.IntegrationEventCatalog` must serialize via `IntegrationEventTransportSerializer` to JSON with non-empty `payload`.
- **Docs**: single table of bounded contexts, services, default local ports, primary responsibilities.
