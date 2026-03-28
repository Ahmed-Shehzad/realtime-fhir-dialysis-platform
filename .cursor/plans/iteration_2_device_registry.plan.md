---
name: Iteration 2 — Device Registry Service (vertical slice)
overview: First bounded-context service with Clean Architecture folders, register-device flow, EF + outbox/inbox host wiring, trust lookup port, API v1, and Infrastructure migration.
todos:
  - id: projects
    content: Domain, Application, Infrastructure, Api projects + solution entries
    status: completed
  - id: domain-app
    content: Device aggregate, TrustState, commands/handlers, integration event, validators
    status: completed
  - id: infra-api
    content: DbContext, repository, trust lookup, EF migration, Program + DevicesController
    status: completed
isProject: true
---

# Iteration 2 — Device Registry

Delivers Milestone 1 `DeviceRegistryService` baseline: register device, persist, outbox integration event, `ITrustLookup` for downstream.
