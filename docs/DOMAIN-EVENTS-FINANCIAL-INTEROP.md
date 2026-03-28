# Domain and integration events — Financial Interoperability

Bounded context: **FinancialInteroperability** (see `docs/ARCHITECTURE-FINANCIAL-INTEROPERABILITY-BOUNDARY.md`).

## Domain events (in-process)

| Event | When |
|-------|------|
| `PatientCoverageRegisteredDomainEvent` | New patient coverage snapshot persisted |
| `CoverageEligibilityRecordedDomainEvent` | Eligibility / benefits outcome stored |
| `DialysisFinancialClaimSubmittedDomainEvent` | Claim created for a treatment session |
| `ClaimAdjudicationRecordedDomainEvent` | Payer claim response applied (idempotent by external id) |
| `ExplanationOfBenefitAttachedDomainEvent` | EOB linked to session and claim |

One handler per event type (platform event conventions).

## Integration events (catalog Tier 1)

Defined in `RealtimePlatform.IntegrationEventCatalog`:

- `PatientCoverageSnapshotRecordedIntegrationEvent`
- `CoverageEligibilityOutcomeRecordedIntegrationEvent`
- `DialysisFinancialClaimSubmittedIntegrationEvent`
- `ClaimAdjudicationRecordedIntegrationEvent`
- `ExplanationOfBenefitLinkedToSessionIntegrationEvent`

FHIR R4 alignment for the learning narrative: **Coverage**, **CoverageEligibilityRequest/Response**, **Claim**, **ClaimResponse**, **ExplanationOfBenefit** — see `docs/FHIR-FINANCIAL-PROFILES.md`.
