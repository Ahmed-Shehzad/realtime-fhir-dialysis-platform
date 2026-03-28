---
name: OpenAPI platform headers
overview: Document X-Correlation-Id and X-Tenant-Id on API operations via a shared IOpenApiOperationTransformer; assert in integration tests.
todos:
  - id: transformer
    content: BuildingBlocks transformer + AddDialysisPlatformOpenApi helper + package ref
    status: completed
  - id: hosts
    content: DeviceRegistry.Api and MeasurementAcquisition.Api use AddDialysisPlatformOpenApi
    status: completed
  - id: tests
    content: Integration tests assert OpenAPI JSON includes both header names on an operation
    status: completed
isProject: false
---
