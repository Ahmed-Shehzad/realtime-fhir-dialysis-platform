---
name: Post–iterations 1–3 follow-ups
overview: Backlog deferred from iterations 1–3—Redis, C5 auth/audit, per-service test projects, and quarantine integration event emission—planned as separate delivery slices.
todos:
  - id: redis-host
    content: "ADR alignment: Redis connection + DI (caching or SignalR backplane per ADR-0001), config sections, health if applicable"
    status: completed
  - id: c5-entra-jwt
    content: "C5: Microsoft Entra ID JWT bearer, scope policies (Read/Write/Admin), least-privilege on business controllers; health remains anonymous"
    status: completed
  - id: c5-audit-register-device
    content: "C5: Audit recorder for security-relevant actions (e.g. RegisterDevice)—persist audit trail, correlation id"
    status: completed
  - id: service-test-projects
    content: "Template: DeviceRegistry.*Tests / MeasurementAcquisition.*Tests (unit + integration scaffolds), CI matrix optional"
    status: completed
  - id: measurement-quarantined-event
    content: Emit MeasurementQuarantinedIntegrationEvent (catalog) when outbox row is quarantined—design relay hook or outbox observer to avoid coupling RealtimePlatform.Outbox to domain assemblies
    status: completed
isProject: true
---

## Context

These items were explicitly **out of scope** for iterations 1–3 but are required for production hardening and catalog completeness.

## Dependency hints

```mermaid
flowchart TD
  Redis[Redis host wiring] --> CachePolicy[Cache key / tenant rules]
  JWT[Entra JWT + scopes] --> Audit[Audit on mutations]
  Relay[Outbox relay quarantine] --> QuarEvent[MeasurementQuarantined or generic hook]
  Tests[Test projects] --> CI[PR checks]
```



## Notes

- **Quarantine event:** Today `[OutboxRelayBackgroundService](../../platform/shared/RealtimePlatform.Outbox/OutboxRelayBackgroundService.cs)` sets `QuarantinedAt` only. Emitting catalog integration events should use an abstraction (for example `IQuarantineNotifier` / `IOutboxLifecycleObserver`) implemented in the host or a thin BuildingBlocks adapter so `RealtimePlatform.Outbox` does not reference `MeasurementAcquisition.Domain`.
- **C5:** Align with workspace `.cursor/rules/c5-compliance.mdc` and existing ADRs under `docs/adr/`.

