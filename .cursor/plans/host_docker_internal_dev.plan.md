---
name: host.docker.internal dev DB
overview: Point Development PostgreSQL connection strings at host.docker.internal while keeping base appsettings on 127.0.0.1 for CI and overrides.
todos:
  - id: dev-json-apis
    content: Add or update appsettings.Development.json ConnectionStrings (host.docker.internal) for all platform *Api projects that use Postgres
    status: completed
isProject: false
---

## Context

Docker Desktop exposes `host.docker.internal` so processes (including containers reaching the host) can target the host. For local Development, connection strings use `Host=host.docker.internal` so the same settings work when the API runs on the host or when tooling expects that hostname. Base `appsettings.json` stays `127.0.0.1` so `dotnet test` on Linux without Docker DNS special-cases keeps working for tests that do not require live DB access.

## Files

- New `appsettings.Development.json` per `*Api` with `ConnectionStrings:Default` mirroring each `appsettings.json` database name, `Host=host.docker.internal`.
- Update existing `DeviceRegistry.Api` and `MeasurementAcquisition.Api` `appsettings.Development.json`.
