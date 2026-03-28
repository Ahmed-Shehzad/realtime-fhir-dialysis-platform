---
name: C5 audit — command handler unit tests
overview: Unit-test RegisterDevice and IngestMeasurement handlers to assert IAuditRecorder receives correct AuditRecordRequest (and duplicate register skips audit).
todos:
  - id: device-registry-tests
    content: RegisterDeviceCommandHandler tests (success + duplicate skips audit)
    status: completed
  - id: measurement-ingest-tests
    content: IngestMeasurementPayloadCommandHandler tests (accepted + rejected JSON audit)
    status: completed
isProject: false
---

## Scope

- Fakes for `IDeviceRepository` / `IAcquisitionRepository`, `IUnitOfWork`, `IAuditRecorder`, `ITenantContext`.
- Reference `BuildingBlocks` from unit test projects for `AuditRecordRequest` / enums.

## Verification

`dotnet test` for `DeviceRegistry.UnitTests` and `MeasurementAcquisition.UnitTests`.
