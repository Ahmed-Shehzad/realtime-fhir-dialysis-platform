---
name: Outbox quarantine (DLQ-style)
overview: Add publish attempt tracking and quarantine on repeated transport failures; exclude quarantined rows from relay polling.
todos:
  - id: entity-options-relay
    content: Extend OutboxMessageEntity + EF config; OutboxRelayOptions.MaxPublishAttempts; relay failure path
    status: completed
  - id: migrations
    content: Add EF migrations for DeviceRegistry, MeasurementAcquisition, RealtimePlatform.Persistence schema
    status: completed
  - id: appsettings
    content: Document MaxPublishAttempts in service appsettings
    status: completed
isProject: true
---

## Behaviour

- Pending: `SentAt == null && QuarantinedAt == null`.
- On publish failure: increment `PublishAttemptCount`, set `LastPublishError` (truncated). If `PublishAttemptCount >= MaxPublishAttempts`, set `QuarantinedAt = UtcNow` (manual replay / tooling later).
