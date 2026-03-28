---
name: Tenant id on Tier-1 integration events
overview: Set IntegrationEvent.TenantId from request tenant context when staging Device Registry and Measurement Acquisition outbox messages.
todos:
  - id: domain-device-register
    content: Add tenantId to Device.Register; set on DeviceRegisteredIntegrationEvent
    status: completed
  - id: domain-measurement-ingest
    content: Add tenantId to RawMeasurementEnvelope.Ingest; set on all integration events
    status: completed
  - id: handlers-wire
    content: Pass _tenant.TenantId from RegisterDevice and IngestMeasurement handlers
    status: completed
isProject: false
---

## Files

- `DeviceRegistry.Domain/Device.cs`
- `MeasurementAcquisition.Domain/RawMeasurementEnvelope.cs`
- `DeviceRegistry.Application/.../RegisterDeviceCommandHandler.cs`
- `MeasurementAcquisition.Application/.../IngestMeasurementPayloadCommandHandler.cs`
