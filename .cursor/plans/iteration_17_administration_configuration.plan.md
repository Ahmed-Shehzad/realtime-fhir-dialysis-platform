---
name: Iteration 17 AdministrationConfiguration
overview: Add AdministrationConfigurationService for facility config, rule-set publication with pre-activation validation, threshold profiles, and feature toggles with Tier 1 integration events and C5 JWT scopes.
todos:
  - id: i17-catalog-auth
    content: Tier1 Configuration events + Dialysis.Configuration.Read/Write scopes/OpenAPI/auth
    status: completed
  - id: i17-domain-app
    content: Domain aggregates/VOs + Application commands/queries/handlers
    status: completed
  - id: i17-infra-api
    content: EF DbContext + repositories + audit + Api port 5016 + migration
    status: completed
  - id: i17-tests
    content: Unit + OpenAPI integration + architecture layering 2–17
    status: completed
isProject: true
---

## Context

Implements blueprint §8.14 and roadmap Iteration 17; aligns `integration_event_catalog.md` Configuration events.

## Approach

Four aggregates (`FacilityConfiguration`, `RuleSet`, `ThresholdProfile`, `FeatureToggle`) with outbox/inbox/Audit; REST under `api/v1/administration-configuration/*`; publish path validates rule JSON before `RuleSetPublishedIntegrationEvent`.
