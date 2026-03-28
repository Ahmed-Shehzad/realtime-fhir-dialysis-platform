# Bounded context catalog (v1)

Single-page map of implemented microservices, default **local** HTTP ports (`launchSettings.json`), and primary capability. Integration event groupings follow [integration_event_catalog.md](../../.cursor/plans/integration_event_catalog.md); Tier 1 contracts live in [`RealtimePlatform.IntegrationEventCatalog`](../../platform/shared/RealtimePlatform.IntegrationEventCatalog/).

| Bounded context | Projects under `platform/services/` | Local port | Responsibility summary |
|-----------------|--------------------------------------|------------|-------------------------|
| Device identity | `DeviceRegistry/` | 5001 | Register and manage dialysis device identity |
| Measurement ingest | `MeasurementAcquisition/` | 5002 | Ingest raw measurement envelopes |
| Treatment session | `TreatmentSession/` | 5003 | Dialysis session lifecycle, patient assignment |
| Audit & provenance | `AuditProvenance/` | 5004 | Platform audit facts and provenance links |
| Validation | `MeasurementValidation/` | 5005 | Validate measurements; quarantine / flag paths |
| Signal conditioning | `SignalConditioning/` | 5006 | Condition signals, quality / drift / dropout events |
| Terminology & profiles | `TerminologyConformance/` | 5007 | Terminology and profile conformance checks |
| Clinical interoperability | `ClinicalInteroperability/` | 5008 | Canonical FHIR observation publication |
| Query read model | `QueryReadModel/` | 5009 | Session/dashboard projections and rebuild hooks |
| Realtime surveillance | `RealtimeSurveillance/` | 5010 | Alerts and session risk signalling |
| Realtime delivery | `RealtimeDelivery/` | 5011 | Live feed delivery (SignalR-oriented domain) |
| Clinical analytics | `ClinicalAnalytics/` | 5012 | Session analysis, derived metrics, trends |
| Reporting | `Reporting/` | 5013 | Session reports, narrative, publication flow |
| Workflow orchestration | `WorkflowOrchestrator/` | 5014 | Sagas / workflow instances, compensation hooks |
| Replay & recovery | `ReplayRecovery/` | 5015 | Replay jobs, recovery plan execution |
| Administration & configuration | `AdministrationConfiguration/` | 5016 | Facility config, rule sets, thresholds, feature toggles |

## Shared platform libraries

| Area | Path |
|------|------|
| Messaging, outbox, persistence abstractions | `platform/shared/RealtimePlatform.*` |
| Integration event CLR (Tier 1) | `platform/shared/RealtimePlatform.IntegrationEventCatalog/` |
| Cross-cutting building blocks | `BuildingBlocks/` |

## Contract and layering tests

- Transport envelope contract (Tier 1 types): [`tests/RealtimePlatform.ContractTests`](../../tests/RealtimePlatform.ContractTests)
- Clean Architecture layering: [`tests/RealtimePlatform.ArchitectureTests`](../../tests/RealtimePlatform.ArchitectureTests)

## Related docs

- [Integration event contract](integration-event-contract.md)
- [Host configuration](host-configuration.md)
- [Saga / event map](integration-event-saga-map.md)
