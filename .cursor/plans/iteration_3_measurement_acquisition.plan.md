---
name: Iteration 3 — Measurement Acquisition Service
overview: Vertical slice for raw telemetry ingest with schema validation, persistence, outbox integration events aligned to integration_event_catalog.md (Device context + Measurement Acquisition), following DeviceRegistry host patterns.
todos:
  - id: plan-md
    content: This plan file
    status: completed
  - id: domain
    content: RawMeasurementEnvelope aggregate, VOs, domain/integration events
    status: completed
  - id: application
    content: Ingest command + handler + validator
    status: completed
  - id: infrastructure-api
    content: DbContext, repository, migration, API v1, Program wiring
    status: completed
isProject: true
---

# Iteration 3 — Measurement Acquisition

## Scope (MVP)

- HTTP ingest `POST /api/v1/measurements` (and optional alias `POST /api/v1/measurements/ingest`) with device id, channel, measurement type, schema version, raw JSON payload.
- Validate payload as JSON; reject with reason if invalid.
- Persist envelope row; emit integration events via outbox:
  - `MeasurementReceivedIntegrationEvent` (catalog §2 Measurement Acquisition)
  - `MeasurementAcceptedIntegrationEvent` **or** `MeasurementRejectedIntegrationEvent` in the same transaction.
- Broker/DLQ and full quarantine store are **follow-ups** (align with catalog `MeasurementQuarantinedIntegrationEvent` later).

## Catalog alignment

See `[integration_event_catalog.md](integration_event_catalog.md)` and `[integration_events_plan_considerations.plan.md](integration_events_plan_considerations.plan.md)`.

## Dependencies

Independent PostgreSQL database from Device Registry (`ConnectionStrings:Default` → `measurement_acquisition_dev` by default).
