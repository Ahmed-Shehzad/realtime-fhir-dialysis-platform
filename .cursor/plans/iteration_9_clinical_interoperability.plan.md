---
name: Iteration 9 Clinical Interoperability
overview: "Vertical slice for ClinicalInteroperability—stub FHIR canonical observation publish + retry, catalog publish/fail events, Dialysis.Interoperability scopes, C5 API, EF, tests."
todos:
  - id: i9-catalog-blocks
    content: CanonicalObservationPublicationFailedIntegrationEvent + Interoperability scopes + appsettings + OpenAPI scan + transformers
    status: completed
  - id: i9-service
    content: ClinicalInteroperability Domain/Application/Infrastructure/Api + slnx + EF migration (port 5008)
    status: completed
  - id: i9-tests
    content: Unit + integration + architecture layering tests
    status: completed
isProject: false
---

# Iteration 9 — Clinical Interoperability Service

Blueprint §8.8. **MVP:** `CanonicalObservationPublication` aggregate (stub FHIR write, Published vs Failed), POST publish + POST retry + GET latest; `CanonicalObservationPublishedIntegrationEvent` / `CanonicalObservationPublicationFailedIntegrationEvent`; defer real Firely client and bundle assembly.

## State machine (MVP)

```mermaid
stateDiagram-v2
    [*] --> Published: stub success
    [*] --> Failed: stub FHIR fault
    Failed --> Published: retry success
    Failed --> Failed: retry still failing
```

## Files

- `Tier1IntegrationEvents.cs` — failure event record
- BuildingBlocks authorization + OpenAPI
- `platform/services/ClinicalInteroperability/**`
- `RealtimeFhirDialysisPlatform.slnx`, tests, sibling `appsettings.json`
