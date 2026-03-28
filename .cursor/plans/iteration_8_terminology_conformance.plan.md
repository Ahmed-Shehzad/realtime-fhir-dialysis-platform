---
name: Iteration 8 Terminology Conformance
overview: "Vertical slice for TerminologyConformance—semantic/code/unit + profile stub checks, catalog § Terminology events, Dialysis.Terminology scopes, C5 API, EF, tests."
todos:
  - id: i8-catalog-blocks
    content: Catalog integration events + Terminology scopes + appsettings + OpenAPI scan + JWT/OpenAPI transformers
    status: completed
  - id: i8-service
    content: TerminologyConformance Domain/Application/Infrastructure/Api + slnx + EF migration (port 5007)
    status: completed
  - id: i8-tests
    content: Unit + integration + architecture layering tests TerminologyConformance
    status: completed
isProject: false
---

# Iteration 8 — Terminology Conformance Service

Blueprint §8.11 and delivery plan Iteration 8. **MVP:** `ConformanceAssessment` aggregate, POST/GET semantic-conformance on `api/v1/resources/{resourceId}/…`, deterministic terminology + FHIR profile URL rules, outbox + audit; defer full ValueSetBinding registry and FHIR instance validation.

## Workflows

```mermaid
sequenceDiagram
  participant Client
  participant API as TerminologyConformance.Api
  participant Handler as ValidateSemanticConformanceCommandHandler
  participant Domain as ConformanceAssessment
  participant DB as PostgreSQL
  Client->>API: POST .../semantic-conformance (code, unit, profile URL)
  API->>Handler: command
  Handler->>Domain: Run(rules)
  Domain-->>Handler: integration events (0–4)
  Handler->>DB: SaveChanges + outbox
```

## Files

- `RealtimePlatform.IntegrationEventCatalog/Tier1IntegrationEvents.cs` (or append) — four terminology events
- `BuildingBlocks` — policies, scopes, `ConfigureDialysisAuthorizationOptions`, OpenAPI transformers
- `tests/Shared/OpenApiBearerSecurityScan.cs`
- All sibling `appsettings.json` under `platform/services/*/Api`
- New `platform/services/TerminologyConformance/**`
- `RealtimeFhirDialysisPlatform.slnx`, architecture + unit + integration test projects

## Risks

- Scope sprawl; keep MVP payloads aligned with `integration_event_catalog.md` names only.
