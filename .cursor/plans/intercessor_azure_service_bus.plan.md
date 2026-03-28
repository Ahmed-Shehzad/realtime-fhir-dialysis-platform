---
name: Intercessor + Azure Service Bus durability
overview: Queue Intercessor commands on Azure Service Bus via MassTransit so work survives microservice outages; consumers dispatch to ISender; ASB DLQ holds poison/failed messages for operational replay.
todos:
  - { id: envelope-consumer, content: Add IntercessorCommandEnvelope + consumer calling ISender with type-safe reflection, status: completed }
  - { id: publisher-client, content: Add IDurableIntercessorCommandClient (ISendEndpoint) + assembly allow-list options, status: completed }
  - { id: masstransit-wireup, content: Register consumer, fix ASB ConfigureEndpoints, in-memory parity, status: completed }
isProject: true
---

## Context

Intercessor `ISender` is in-process. HTTP handlers that call `SendAsync` lose work if the process dies before handling completes. MassTransit with Azure Service Bus provides broker persistence; failed deliveries surface as DLQ messages ([Azure Service Bus dead-letter](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dead-letter-queues)).

## Approach

1. **Envelope** – broker message carries assembly-qualified request type + JSON payload + optional correlation/tenant ids (API DTO boundary; not an `IRequest`).
2. **Publisher** – `IDurableIntercessorCommandClient` serializes an `IRequest`, sends to a dedicated queue (configured name, default `intercessor-commands`).
3. **Consumer** – `IConsumer<IntercessorCommandEnvelope>` resolves type, validates against allow-list, deserializes, invokes `ISender` via reflection for `IRequest` / `IRequest<TResponse>` (responses are discarded for durable enqueue).
4. **MassTransit** – `AddConsumer` with explicit endpoint queue name; **Azure** branch calls `ConfigureEndpoints(context)` (currently missing). Retries are MassTransit defaults; after exhaustion messages move per transport (ASB DLQ / error queue) for replay.
5. **Security (C5)** – optional `AllowedCommandAssemblyNamePrefixes`: when set, only types whose assembly short name starts with a configured prefix are deserialized.

## Mermaid

```mermaid
flowchart LR
  API[Api Controller] --> DC[IDurableIntercessorCommandClient]
  DC --> Q[ASB queue intercessor-commands]
  Q --> C[IntercessorCommandConsumer]
  C --> S[ISender]
  S --> H[Command Handler]
```

## Files

- `platform/shared/RealtimePlatform.MassTransit/` – envelope, consumer, client, options, `MassTransitServiceCollectionExtensions` updates, `RealtimePlatform.MassTransit.csproj` (Intercessor ref).

## Risks

- Assembly-qualified type names are a controlled deserialization surface; allow-list strongly recommended in non-dev environments.
- DLQ replay is operational (Portal, ServiceBusProcessor, or MassTransit tooling), not automatic on startup.
