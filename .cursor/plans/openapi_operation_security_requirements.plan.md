---
name: OpenAPI operation security (Bearer)
overview: Add IOpenApiOperationTransformer that sets operation.Security for MVC actions with IAuthorizeData and no IAllowAnonymous; integration test scans paths JSON.
todos:
  - id: transformer
    content: BearerSecurityRequirementOperationTransformer + register in AddDialysisPlatformOpenApi
    status: completed
  - id: tests-docs
    content: OpenApiBearerSecurityScan + integration assertion; host-configuration one-liner
    status: completed
isProject: false
---
