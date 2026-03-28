---
name: C5 hardening + outbox DLQ observability
overview: Entra JWT + scope policies on business APIs, X-Correlation-Id middleware, RegisterDevice audit, and quarantine DLQ observability hook after relay.
todos:
  - id: c5-web-extensions
    content: BuildingBlocks JWT bearer, scope policies, correlation middleware/accessor, auth options binding
    status: completed
  - id: outbox-quarantine-notifier
    content: IOutboxQuarantineNotifier + relay invocation + structured DLQ log (incl. MeasurementAcquisition catalog hint)
    status: completed
  - id: register-device-audit
    content: RegisterDeviceCommand principal id + IAuditRecorder in handler; controller policies + correlation
    status: completed
  - id: measurement-api-auth
    content: MeasurementAcquisition policies + Program wiring
    status: completed
  - id: docs
    content: host-configuration.md C5 / Entra / scopes / correlation
    status: completed
isProject: true
---

## Notes

- Health and OpenAPI remain unauthenticated. Business controllers use `ScopeOrBypassRequirement` with `Authentication:JwtBearer:DevelopmentBypass` in Development when needed.
- Quarantine rows are the **logical DLQ** after publish failures; notifier provides durable observability until a broker-native DLQ exists.
