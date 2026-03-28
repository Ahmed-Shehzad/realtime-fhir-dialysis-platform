---
name: OpenAPI per-operation Entra scopes
overview: Populate Bearer security requirement scope arrays from Authorize policy names via AuthorizationScopesOptions; extend integration scan/tests.
todos:
  - id: transformer-scopes
    content: BearerSecurityRequirementOperationTransformer resolves PlatformAuthorizationPolicies → scope strings
    status: completed
  - id: tests-scan
    content: OpenApiBearerSecurityScan + assert Dialysis.Devices.Write appears under a path security entry
    status: completed
isProject: false
---
