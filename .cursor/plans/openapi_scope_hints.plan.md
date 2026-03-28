---
name: OpenAPI authorization scope hints
overview: Append AuthorizationScopesOptions values to Bearer security scheme description; assert in shared OpenAPI scan + integration tests.
todos:
  - id: transformer
    content: JwtBearerSecurityDocumentTransformer injects IOptions<AuthorizationScopesOptions> and lists scopes
    status: completed
  - id: tests
    content: OpenApiBearerSecurityScan + extend bearer integration test
    status: completed
isProject: false
---
