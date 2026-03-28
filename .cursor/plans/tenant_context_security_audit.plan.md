---
name: Tenant context in C5 pipeline and audit
overview: Register X-Tenant-Id resolution on platform APIs and persist tenant id on security audit rows.
todos:
  - id: c5-tenant-di-pipeline
    content: Add AddTenantResolution to C5 DI; UseTenantResolution after correlation middleware
    status: completed
  - id: audit-tenant-handlers
    content: Inject ITenantContext into RegisterDevice and IngestMeasurement handlers; set AuditRecordRequest.TenantId
    status: completed
isProject: false
---

## Scope

Align with `BuildingBlocks.Tenancy` and C5 multi-tenancy: middleware was implemented but not wired; audit rows had `TenantId` null.

## Files

- `BuildingBlocks/DependencyInjection/C5WebApiServiceCollectionExtensions.cs`
- `BuildingBlocks/WebApplicationC5Extensions.cs`
- `DeviceRegistry.Application/Commands/RegisterDevice/RegisterDeviceCommandHandler.cs`
- `MeasurementAcquisition.Application/Commands/IngestMeasurement/IngestMeasurementPayloadCommandHandler.cs`
