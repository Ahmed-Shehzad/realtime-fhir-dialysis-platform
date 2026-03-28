---
name: React Query + SignalR bridge
overview: Wire pdms-web to ClinicalFeedHub via @microsoft/signalr, proxy hubs through the gateway in dev, and update React Query cache with setQueryData for session overview, session feed tail, and tenant alerts.
todos:
  - { id: gateway-hubs-route, content: Add YARP route /hubs → realtime-delivery cluster, status: completed }
  - { id: vite-hubs-proxy, content: Proxy /hubs (incl. WebSocket) to gateway in Vite dev, status: completed }
  - { id: client-deps-types, content: Add @microsoft/signalr, feed payload types, query keys, getHttpAccessToken, status: completed }
  - { id: bridge-ui, content: ClinicalFeedBridge + SessionSummaryCard live strip + optional tenant alerts strip on dashboard, status: completed }
isProject: true
---

# React Query + SignalR (Clinical feed)

## Context

- Backend: `ClinicalFeedHub` at `/hubs/clinical-feed`; server methods `JoinSessionFeed`, `LeaveSessionFeed`, `JoinTenantAlerts`, `LeaveTenantAlerts`; client receives `sessionFeed` and `alertFeed` (`SessionFeedPayload`, `AlertFeedPayload`).
- Gateway did not route `/hubs` → added route to `realtime-delivery` so the same origin as `/api` works.

## Client behavior

- `HubConnection` with `withAutomaticReconnect`; JWT via `accessTokenFactory` (gateway forwards Bearer; hub accepts `access_token` query from SignalR client).
- On `sessionFeed`: immutable `setQueryData` for `sessionOverview` when cache exists (bump `projectionUpdatedAtUtc`); append to `sessionFeedTail` query (cap 20).
- On `alertFeed`: append to `tenantAlertsFeed` (cap 50).
- On `reconnected`: invalidate current session overview (active) to cover missed events.
- `ClinicalFeedBridge` inside `BrowserRouter` reads `sessionId` from search params; subscribes to tenant alerts when user has `delivery.read` or `admin` (or try in DEV for local testing).

## Files

- `platform/gateway/RealtimePlatform.ApiGateway/appsettings.json`
- `clients/pdms-web/vite.config.ts`
- `clients/pdms-web/package.json`
- `clients/pdms-web/src/api/httpClient.ts`
- `clients/pdms-web/src/types/clinicalFeed.ts`
- `clients/pdms-web/src/lib/queryKeys.ts`
- `clients/pdms-web/src/auth/dialysisScopeMap.ts`, `rolePolicies.ts`
- `clients/pdms-web/src/realtime/ClinicalFeedBridge.tsx`
- `clients/pdms-web/src/hooks/useSessionFeedTailQuery.ts`, `useTenantAlertsFeedQuery.ts`
- `clients/pdms-web/src/App.tsx`
- `clients/pdms-web/src/components/dashboard/SessionSummaryCard.tsx`
- `clients/pdms-web/src/pages/DashboardPage.tsx` (alerts strip)
