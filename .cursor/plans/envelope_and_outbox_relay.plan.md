---
name: Envelope v1 + outbox relay
overview: Align serialized outbox/broker payloads with integration_event_catalog.md §1 and add a background relay that publishes pending rows via ITransportPublisher.
todos:
  - id: envelope-metadata
    content: Extend IntegrationEvent with catalog envelope fields (init); add IntegrationEventTransportSerializer (catalog JSON + payload strip)
    status: completed
  - id: interceptor-envelope
    content: Use transport serializer in IntegrationEventDispatchInterceptor for outbox payload bytes
    status: completed
  - id: outbox-relay
    content: Add OutboxRelayOptions, ITransportPublisher, LoggingTransportPublisher, OutboxRelayBackgroundService<TDbContext>, DI extension
    status: completed
  - id: wire-hosts
    content: Register relay + appsettings in DeviceRegistry.Api and MeasurementAcquisition.Api
    status: completed
isProject: true
---

## Context

- Catalog §1 defines a single JSON envelope (`eventId`, `eventType`, `eventVersion`, `occurredUtc`, correlation/causation/workflow/saga, facility/session/patient/device/partition, `payload`).
- Today `SystemTextJsonMessageSerializer` writes the flat CLR record; relay does not exist.

## Approach

1. **Envelope**: Add optional `init` metadata on `IntegrationEvent`. New `IntegrationEventTransportSerializer` in BuildingBlocks builds §1 JSON: top-level metadata from the instance, `payload` = serialized record minus metadata property names (camelCase). `RoutingDeviceId` maps to envelope `deviceId` when set; else copy from `payload.deviceId` if present.
2. **Interceptor**: For `IntegrationEvent`, write `Payload` using transport serializer; otherwise keep `IMessageSerializer`.
3. **Relay**: `RealtimePlatform.Outbox` gains hosted service polling `OutboxMessageEntity` where `SentAt == null`, `ITransportPublisher` (default logger), then sets `SentAt`. Uses `IServiceScopeFactory` + scoped `TDbContext`.

## Files

- `BuildingBlocks/IntegrationEvent.cs`, new `BuildingBlocks/IntegrationEvents/IntegrationEventTransportSerializer.cs`, `BuildingBlocks/Interceptors/IntegrationEventDispatchInterceptor.cs`
- `platform/shared/RealtimePlatform.Outbox/`* (options, publisher interface, hosted service, DI)
- `DeviceRegistry.Api/Program.cs`, `MeasurementAcquisition.Api/Program.cs`, appsettings

## Risks

- Per-service DBs: each host runs its own relay instance (same pattern as colocated outbox).

