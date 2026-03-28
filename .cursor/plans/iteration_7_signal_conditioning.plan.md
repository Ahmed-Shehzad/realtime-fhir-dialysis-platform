---
name: Iteration 7 Signal Conditioning
overview: "Vertical slice for SignalConditioning—MVP conditioning job with quality/drift/dropout stubs, catalog events, Conditioning scopes, C5 API, EF, tests."
todos:
  - id: i7-catalog-blocks
    content: Tier1 signal events + Dialysis.Conditioning scopes + appsettings + OpenAPI scan
    status: completed
  - id: i7-service
    content: SignalConditioning projects + slnx + EF migration
    status: completed
  - id: i7-tests
    content: Unit + integration + architecture tests
    status: completed
isProject: false
---

# Iteration 7 — Signal Conditioning Service

Blueprint §8.5 and §1745. **MVP:** single `ConditioningResult` aggregate, HTTP condition + read-latest, deterministic rules, outbox + audit; defer streaming consumer and full `SignalWindow` persistence.
