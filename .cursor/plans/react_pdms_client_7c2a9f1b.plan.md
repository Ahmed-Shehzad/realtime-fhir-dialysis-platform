---
name: React PDMS client (Vite)
overview: Add a TypeScript React SPA under clients/ with Vite, TanStack Query, Axios, Tailwind, and D3, plus baseline healthcare-oriented security patterns (HTTPS enforcement, RBAC hooks, CSP for dev, minimal PHI exposure).
todos:
  - id: scaffold
    content: Scaffold Vite React-TS app in clients/pdms-web and install deps
    status: completed
  - id: security-core
    content: Axios client, env validation, Query defaults (retry/stale), RBAC helpers
    status: completed
  - id: ui-d3
    content: Tailwind layout, D3 sample chart, README/.env.example
    status: completed
isProject: true
---

# React client (PDMS learning UI)

## Context

Bounded-context APIs are .NET; this client is a separate SPA for dashboards and FHIR-oriented views. Compliance is enforced at API (auth, audit); the UI adds transport safety (HTTPS), role-gated rendering, and avoids persisting secrets in `localStorage` by default.

## Workflows

```mermaid
flowchart LR
  Browser[Vite SPA] -->|HTTPS + JWT| Api[.NET APIs]
  Browser --> TanStackQuery[TanStack Query]
  TanStackQuery --> Axios[Axios instance]
  Axios --> Api
```

## Files

- `clients/pdms-web/` — new Vite project
- `.env.example` — `VITE_API_BASE_URL` only; no secrets in repo

## Risks

- True E2E payload encryption beyond TLS requires key management not in scope; document TLS + server-side encryption at rest.
