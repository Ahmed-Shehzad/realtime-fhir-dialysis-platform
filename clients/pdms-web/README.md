# PDMS Web

Vite + React + TanStack Query + Axios + Tailwind + D3. Calls the **RealtimePlatform API Gateway (YARP)** as a single `VITE_API_BASE_URL` origin.

## Environment

Copy `.env.example` to `.env.local`. Recommended for local realtime + REST:

```bash
VITE_API_BASE_URL=http://localhost:5100
VITE_APP_TENANT_ID=default
VITE_APP_ROLES=readmodel.read,delivery.read,admin
```

If `VITE_API_BASE_URL` is unset in dev, the Vite dev server **proxies** `/health`, `/api`, and **`/hubs` (WebSocket)** to `http://localhost:5100`; the gateway must still be running for those paths to work.

PostgreSQL for read-model APIs: run **`RealtimePlatform.AppHost`** (see [docs/DEVELOPMENT-ENVIRONMENT.md](../../docs/DEVELOPMENT-ENVIRONMENT.md)), then from the **repository root** apply EF migrations (e.g. `./scripts/dev-database-setup.sh` or `./scripts/ef-database-update-query-read-model.sh`). Use `PGHOST` / `PGPORT` from the Aspire dashboard if Postgres is not on `127.0.0.1:5432`.

To push test data through the **gateway** (sessions, measurements, broadcast, etc.), use **`tools/Simulation.GatewayCli`** — see [tools/Simulation.GatewayCli/README.md](../../tools/Simulation.GatewayCli/README.md).

## Roles and scopes

`RoleGate` is **view-layer only**; APIs enforce JWT scopes. For local UI:

- **`VITE_APP_ROLES`**: comma-separated UI roles (e.g. `readmodel.read`, `financial.read`, `clinical.read`, `admin`).
- **`VITE_DIALYSIS_SCOPES`**: comma-separated Dialysis scopes mapped in `src/auth/dialysisScopeMap.ts` (e.g. `Dialysis.ReadModel.Read` → `readmodel.read`).

## Content-Security-Policy

The **Vite dev server** sets a relaxed CSP in `vite.config.ts` for HMR. **Production** static hosting should use a stricter policy at the CDN or ingress: script and connect sources should allow only your gateway origin and identity provider; avoid `unsafe-eval`. Align `connect-src` with `VITE_API_BASE_URL`.

## Dependency audits

Run `npm audit` (or integrate Snyk / OWASP dependency-check) in CI to satisfy organizational security review; fix or document accepted risks for healthcare deployments.

## Verifying Development frontend ↔ Development gateway

With **`npm run dev`**, the client runs in Vite **development** mode (`import.meta.env.DEV`). The dashboard **API health** card calls gateway **`GET /health`**, which includes **`environment`** (ASP.NET `ASPNETCORE_ENVIRONMENT`). A green **Environment check** means both are **Development**. Amber means a mismatch (e.g. UI dev but gateway Production) or missing `environment` on an older gateway build.

## Dashboard route

- **`/`** — dashboard with health, session overview (read model), demo vitals chart, financial timeline, PHI preview (gated).

**Context selectors** (read-model role): severity filter and session dropdown load **`GET /api/v…/alerts`** and **`GET /api/v…/dashboard/summary`**; choosing a session updates `?sessionId=` so Session overview, financial timeline, SignalR session group, and PHI strip stay aligned. Patient id uses **Apply** for `?patientId=` (financial query).

Query parameters: `?sessionId=...&patientId=...` (deep-linkable; selectors keep them in sync).

## Realtime updates (SignalR)

Live UI uses **`ClinicalFeedHub`** on the gateway path **`/hubs/clinical-feed`** (YARP → **RealtimeDelivery**). In **`npm run dev`**, the SignalR URL is always **`/hubs/clinical-feed` on the Vite host** (proxied to `localhost:5100`) so negotiation stays **same-origin** even if **`VITE_API_BASE_URL`** points at the gateway for REST.

| Mechanism | Purpose |
|----------|---------|
| `src/realtime/ClinicalFeedBridge.tsx` | One SignalR client; joins **tenant alerts** and optional **session** group from `?sessionId=` |
| TanStack Query `setQueryData` | **Tenant alerts** → `useTenantAlertsFeedQuery`; **session feed** + overview touch → `useSessionFeedTailQuery` / `sessionOverview` |
| Dashboard | **Tenant alerts (live)** panel; **Session overview → Recent session feed (SignalR)** when `sessionId` is set |

**Backend:** Run **RealtimePlatform.ApiGateway** and **RealtimeDelivery.Api** (port **5011** in default YARP clusters). In **Development**, `Authentication:JwtBearer:DevelopmentBypass` on services allows hub authorization without a real JWT.

**Try it:** from the repo root run `SIMULATION_GATEWAY_TENANT=default ./scripts/run-simulation-gateway-cli.sh` (optional: `SIMULATION_SCENARIO_PREFIX=demo`), copy the printed `treatmentSessionId`, open `http://localhost:5173/?sessionId=<that-id>`, and watch the session feed and tenant alerts panels update.

## TanStack Query retry policy

Global defaults in `src/providers/AppQueryClientProvider.tsx`: retries skip 401/403 and most 4xx; health query uses shorter `staleTime` / `refetchInterval` in `useBackendHealthQuery`.
