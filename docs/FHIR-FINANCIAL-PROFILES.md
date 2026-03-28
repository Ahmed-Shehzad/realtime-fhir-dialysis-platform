# FHIR financial profiles and implementation guides

## Baseline (R4)

- **US Core** — align **Patient**, **Coverage**, **Organization**, **Encounter** instances with US Core where relevant for US-centric learning scenarios.
- **US Core FHIR and billing** — use US guidance as the default terminology and reference patterns for Coverage and payer Organization.

## Payer workflows (prior authorization)

- **Da Vinci** Implementation Guides when modeling payer-facing flows:
  - **CRD** (Coverage Requirements Discovery)
  - **DTR** (Documentation Templates and Rules) — often surfaced as **Task**
  - **PAS** (Prior Authorization Support) — **Claim**/`use=preauthorization`, **ClaimResponse**, **Task**

## Consumer / member access

- **CARIN BB** (Consumer Directed Payer Data Exchange) — for patient-access **ExplanationOfBenefit**-style APIs and alignment with consumer-facing EOB payloads.

## Platform note

This repository implements **internal** aggregates and APIs that **mirror** these FHIR resources; external FHIR server profiles and validation are introduced incrementally. Map internal IDs to FHIR logical references (`Coverage/id`, `Claim/id`, `ExplanationOfBenefit/id`) in persistence and integration layers.
