---
name: Iteration 12 Realtime Delivery
overview: "RealtimeDeliveryService—SignalR clinical feed hub, tenant-scoped session/alert groups, JWT (query access_token), broadcast commands + REST triggers, Dialysis.Delivery scopes, in-memory audit."
todos:
  - id: i12-auth-openapi
    content: DeliveryRead/DeliveryWrite scopes, OpenAPI transformer + shared scan + all appsettings
    status: completed
  - id: i12-service
    content: RealtimeDelivery Domain/Application/Api (hub + gateway + Program) + slnx
    status: completed
  - id: i12-tests
    content: Handler unit tests with fake gateway; OpenAPI integration tests; architecture layering
    status: completed
isProject: false
---

# Iteration 12 — Realtime Delivery

Blueprint [realtime_fhir_dialysis_implementation_plan.md](realtime_fhir_dialysis_implementation_plan.md) §8.12.

## MVP

- **Hub** `/hubs/clinical-feed`: `JoinSessionFeed` / `LeaveSessionFeed` / `JoinTenantAlerts` / `LeaveTenantAlerts` with groups `tenant:{tid}:session:{sid}` and `tenant:{tid}:alerts`.
- **Authorization:** `[Authorize(DeliveryRead)]` on hub; negotiate + WebSocket JWT via `access_token` query.
- **Push:** `IRealtimeFeedGateway` + `SignalRRealtimeFeedGateway` (`IHubContext<ClinicalFeedHub>`); commands `BroadcastSessionFeedCommand` / `BroadcastAlertFeedCommand`; REST `POST .../broadcast` (DeliveryWrite) for operators/integration placeholders.
- **Audit:** `FhirAuditRecorder` + `InMemoryAuditEventStore` (no EF/outbox for this edge service).
- **Port:** `5011`.

## Diagram

```mermaid
flowchart LR
  Client[SignalR Client] --> Hub[ClinicalFeedHub]
  API[Broadcast API] --> Handler[Command Handler]
  Handler --> Gateway[IRealtimeFeedGateway]
  Gateway --> Hub
```
