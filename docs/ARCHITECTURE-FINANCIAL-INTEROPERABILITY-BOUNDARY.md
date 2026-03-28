# Financial Interoperability bounded context

## Decision

**Financial FHIR and revenue-cycle workflow artifacts live in a dedicated bounded context** — **FinancialInteroperability** — separate from **ClinicalInteroperability**.

## Rationale

- **Least privilege (C5)**: financial APIs and data stores can use different scopes (`Dialysis.Financial.Read/Write`), retention, and audit categories than clinical vitals and Observation publication.
- **ClinicalInteroperability** remains responsible for canonical **clinical** FHIR writes (e.g. Observation) and retry/quarantine patterns.
- **FinancialInteroperability** owns coverage snapshots, eligibility outcomes, claim lifecycle, adjudication, and EOB linkage to **Treatment Session** and optional **FHIR Encounter** reference.
- Shared **BuildingBlocks** patterns: JWT, tenant resolution, correlation id, `IAuditRecorder`, MassTransit outbox on the financial `DbContext`.

## References

- Plan: `.cursor/plans/fhir_financial_workflow_coverage_claims_eob.plan.md` (do not duplicate; implementation tracked in repo).
- Domain/integration inventory: `docs/DOMAIN-EVENTS-FINANCIAL-INTEROP.md`.
- Profiles: `docs/FHIR-FINANCIAL-PROFILES.md`.
