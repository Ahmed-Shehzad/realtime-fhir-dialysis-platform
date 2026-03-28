# Host configuration (messaging persistence)

## C5 — Entra ID, scopes, correlation, outbox DLQ signal

| Piece | Location |
|--------|----------|
| JWT + policies + correlation accessor + audit + tenant DI | [`AddDialysisPlatformC5WebApi`](../../BuildingBlocks/DependencyInjection/C5WebApiServiceCollectionExtensions.cs) |
| Pipeline (X-Correlation-Id → `HttpContext.Items`, `X-Tenant-Id` → `ITenantContext`, authZ) | [`UseDialysisPlatformC5`](../../BuildingBlocks/WebApplicationC5Extensions.cs) |
| OpenAPI: optional `X-Correlation-Id` + `X-Tenant-Id`; `components.securitySchemes.Bearer` (JWT); `security` on `[Authorize]` actions (OAS 3.1 may use `#/components/securitySchemes/Bearer` as key; value array lists Entra scope strings from policy) | [`AddDialysisPlatformOpenApi`](../../BuildingBlocks/OpenApi/DialysisOpenApiServiceCollectionExtensions.cs); headers: [`CorrelationAndTenantHeadersOperationTransformer`](../../BuildingBlocks/OpenApi/CorrelationAndTenantHeadersOperationTransformer.cs); op security: [`BearerSecurityRequirementOperationTransformer`](../../BuildingBlocks/OpenApi/BearerSecurityRequirementOperationTransformer.cs); JWT: [`JwtBearerSecurityDocumentTransformer`](../../BuildingBlocks/OpenApi/JwtBearerSecurityDocumentTransformer.cs) |
| Scope policies | [`PlatformAuthorizationPolicies`](../../BuildingBlocks/Authorization/PlatformAuthorizationPolicies.cs) + [`AuthorizationScopesOptions`](../../BuildingBlocks/Options/AuthorizationScopesOptions.cs) (`Authorization:Scopes`) |
| Dev-only policy bypass | `Authentication:JwtBearer:DevelopmentBypass` = `true` **and** host environment Development (see [`ScopeOrBypassHandler`](../../BuildingBlocks/Authorization/ScopeOrBypassHandler.cs)) |

`Authority` must be set for non-Development environments ([`JwtBearerStartupOptions`](../../BuildingBlocks/Options/JwtBearerStartupOptions.cs)) — e.g. `https://login.microsoftonline.com/{tenantId}/v2.0`. Map OpenAPI and `/health` with `.AllowAnonymous()` so probes stay unauthenticated.

When the outbox relay [quarantines](../../platform/shared/RealtimePlatform.Outbox/OutboxRelayBackgroundService.cs) a message (logical **DLQ** after repeated publish failures), it calls [`IOutboxQuarantineNotifier`](../../platform/shared/RealtimePlatform.Outbox/IOutboxQuarantineNotifier.cs), implemented by [`CompositeOutboxQuarantineNotifier`](../../platform/shared/RealtimePlatform.Outbox/CompositeOutboxQuarantineNotifier.cs) over all registered [`IOutboxQuarantineHandler`](../../platform/shared/RealtimePlatform.Outbox/IOutboxQuarantineHandler.cs) instances. The default handler is [`LoggingOutboxQuarantineHandler`](../../platform/shared/RealtimePlatform.Outbox/LoggingOutboxQuarantineHandler.cs). Add cross-cutting behavior with `TryAddEnumerable(ServiceDescriptor.Singleton<IOutboxQuarantineHandler, YourHandler>())` **before** `AddRealtimePlatformOutboxRelay`. Measurement Acquisition additionally registers [`MeasurementQuarantineIntegrationEventOutboxHandler`](../../platform/services/MeasurementAcquisition/MeasurementAcquisition.Infrastructure/Outbox/MeasurementQuarantineIntegrationEventOutboxHandler.cs) to enqueue [`MeasurementQuarantinedIntegrationEvent`](../../platform/shared/RealtimePlatform.IntegrationEventCatalog/MeasurementQuarantinedIntegrationEvent.cs). To replace the entire notifier, register a custom `IOutboxQuarantineNotifier` before `AddRealtimePlatformOutboxRelay` (you then bypass the composite and must forward to handlers yourself if needed).

## Redis (ADR-0001)

Shared wiring lives in [`RealtimePlatform.Redis`](../../platform/shared/RealtimePlatform.Redis/). From the API host: `AddRealtimePlatformRedis(IConfiguration)` and chain `AddRealtimePlatformRedisHealthCheck` on `IHealthChecksBuilder`. Configuration under `RealtimePlatform:Redis`:

| Property | Meaning |
|----------|---------|
| `Enabled` | When `true`, registers `IConnectionMultiplexer` and stack-exchange distributed cache. |
| `ConnectionString` | StackExchange.Redis configuration; if empty, `ConnectionStrings:Redis` is used. |
| `InstanceName` | Prefix for distributed cache keys. |

When `Enabled` is `false` (default in local appsettings), Redis types are not registered and no Redis health check is added.

## Configuration sections

Under `RealtimePlatform` (override path via `AddRealtimePlatformMessagingPersistence`):

| Section | Binds to |
|---------|-----------|
| `OutboxPublisher` | [`OutboxPublisherOptions`](../../platform/shared/RealtimePlatform.Outbox/OutboxPublisherOptions.cs) (`SourceAddress`) |
| `OutboxStorage` | [`OutboxStorageOptions`](../../platform/shared/RealtimePlatform.Outbox/OutboxStorageOptions.cs) (`Schema`, `TableName`) |
| `InboxStorage` | [`InboxStorageOptions`](../../platform/shared/RealtimePlatform.Outbox/InboxStorageOptions.cs) (`Schema`, `TableName`) |
| `OutboxRelay` | [`OutboxRelayOptions`](../../platform/shared/RealtimePlatform.Outbox/OutboxRelayOptions.cs) (`Enabled`, `PollInterval`, `MaxBatchSize`, `MaxPublishAttempts`) |
| `Redis` | [`RedisPlatformOptions`](../../platform/shared/RealtimePlatform.Redis/RedisPlatformOptions.cs) (`Enabled`, `ConnectionString`, `InstanceName`) |

### Example `appsettings.json`

```json
{
  "RealtimePlatform": {
    "OutboxPublisher": {
      "SourceAddress": "https://localhost:7001/"
    },
    "OutboxStorage": {
      "Schema": "public",
      "TableName": "outbox_messages"
    },
    "InboxStorage": {
      "Schema": "public",
      "TableName": "inbox_states"
    },
    "OutboxRelay": {
      "Enabled": true,
      "PollInterval": "00:00:01",
      "MaxBatchSize": 100,
      "MaxPublishAttempts": 10
    },
    "Redis": {
      "Enabled": false,
      "ConnectionString": "",
      "InstanceName": "service-name:"
    }
  }
}
```

## Registration (Program.cs)

Platform APIs should call `AddDialysisPlatformOpenApi()` instead of bare `AddOpenApi()` so optional `X-Correlation-Id` and `X-Tenant-Id` appear on every operation in `/openapi/v1.json`.

1. **Persistence helpers** (JSON serializer, options, `IntegrationEventDispatchInterceptor`):

```csharp
builder.Services.AddRealtimePlatformMessagingPersistence(builder.Configuration);
```

2. **Intercessor / Mediator** with `IPublisher` (required by interceptors): register your pipeline before or with the above so `IntegrationEventDispatchInterceptor` resolves.

3. **Domain events** (same transactional boundary as outbox staging): register `DomainEventDispatcherInterceptor` and add it to the same `DbContext`:

```csharp
builder.Services.AddSingleton<DomainEventDispatcherInterceptor>();
```

4. **Application DbContext** (Npgsql + shared interceptors):

```csharp
builder.Services.AddDbContext<YourAppDbContext>((sp, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")!);
    options.AddInterceptors(
        sp.GetRequiredService<IntegrationEventDispatchInterceptor>(),
        sp.GetRequiredService<DomainEventDispatcherInterceptor>());
});
```

5. **Model**: in `YourAppDbContext.OnModelCreating`, apply the same table names as configuration:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    OutboxModelConfiguration.ConfigureOutboxMessages(modelBuilder, _outboxStorage.Value);
    InboxModelConfiguration.ConfigureInboxStates(modelBuilder, _inboxStorage.Value);
    // …domain configuration
}
```

Inject `IOptions<OutboxStorageOptions>` and `IOptions<InboxStorageOptions>` into your DbContext constructor, or read a single `IOptionsMonitor` snapshot.

6. **Inbox store** (scoped, uses your DbContext):

```csharp
builder.Services.AddRealtimePlatformInboxStore<YourAppDbContext>();
```

7. **Outbox relay** (optional; background publish of `outbox_messages` via [`ITransportPublisher`](../../platform/shared/RealtimePlatform.Outbox/ITransportPublisher.cs), with quarantine after `MaxPublishAttempts` failures):

```csharp
builder.Services.AddRealtimePlatformOutboxRelay<YourAppDbContext>(builder.Configuration);
```

Register a custom `ITransportPublisher` *before* this call when replacing the default logging publisher.

## Reference migrations

[`RealtimePlatform.Persistence`](../../platform/shared/RealtimePlatform.Persistence/) contains `SchemaOnlyDbContext` and EF migrations for `outbox_messages` and `inbox_states` with default names.

- Local tool: `dotnet tool restore` (uses [`.config/dotnet-tools.json`](../../.config/dotnet-tools.json)).
- Design-time connection: set `REALTIME_PLATFORM_EF_CONNECTION` or rely on the default in [`SchemaOnlyDbContextFactory`](../../platform/shared/RealtimePlatform.Persistence/SchemaOnlyDbContextFactory.cs).
- Apply to a database: `dotnet ef database update --project platform/shared/RealtimePlatform.Persistence/RealtimePlatform.Persistence.csproj` (after `dotnet tool restore`).

Services may instead merge the generated SQL into their own migrations.

### Local PostgreSQL (all bounded-context databases)

Run [`scripts/postgres-init-databases.sql`](../../scripts/postgres-init-databases.sql) once (as a PostgreSQL superuser) to create every `*_dev` database name used by the APIs. Then apply each service’s EF migrations from the repo root with `dotnet ef database update --project …Infrastructure.csproj --startup-project …Api.csproj` and `ConnectionStrings__Default` pointing at `Host=127.0.0.1` and the matching `Database=` value. `RealtimeDelivery.Api` does not use a dedicated PostgreSQL database in this solution.

## Device Registry service

The first bounded-context API lives under `platform/services/DeviceRegistry/`. Apply its database schema (devices + outbox + inbox) with:

`dotnet ef database update --project platform/services/DeviceRegistry/DeviceRegistry.Infrastructure/DeviceRegistry.Infrastructure.csproj --startup-project platform/services/DeviceRegistry/DeviceRegistry.Api/DeviceRegistry.Api.csproj`

Then run the API (`DeviceRegistry.Api`, default URL `http://localhost:5001`): `GET /health`, `POST /api/v1/devices`, `GET /api/v1/devices/{id}/trust`.

## Measurement Acquisition service

Second bounded-context API: `platform/services/MeasurementAcquisition/`. Apply schema (envelopes + outbox + inbox + outbox quarantine columns) with:

`dotnet ef database update --project platform/services/MeasurementAcquisition/MeasurementAcquisition.Infrastructure/MeasurementAcquisition.Infrastructure.csproj`

Run `MeasurementAcquisition.Api` (default `http://localhost:5002`): `GET /health`, `POST /api/v1/measurements` (optional alias `POST /api/v1/measurements/ingest`), optional header `X-Correlation-Id` (ULID).

## Consumer flow (`IInboxStore`)

1. `TryReserveAsync(messageId, consumerId)` — returns `false` if already reserved (duplicate delivery).
2. Run handler.
3. `MarkProcessedAsync(messageId, consumerId, DateTimeOffset.UtcNow)` — or roll back the surrounding transaction on failure.

Use a stable `consumerId` per logical consumer (for example queue subscription name + host).
