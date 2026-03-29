---
name: Vitals trend refresh + payload coercion
overview: Persist session feed tail to sessionStorage so vitals trend survives refresh; coerce occurredAtUtc when not a string.
todos:
  - { id: storage, content: Add sessionFeedTail sessionStorage helper + wire ClinicalFeedBridge persist, status: completed }
  - { id: query, content: Seed useSessionFeedTailQuery initialData from storage (no async refetch race), status: completed }
  - { id: coerce, content: Harden coerceSessionFeedPayload occurredAtUtc (Date/number), status: completed }
  - { id: verify, content: npm run build + eslint touched files, status: completed }
isProject: true
---

## Context

`useSessionFeedTailQuery` was SignalR-only (`enabled: false`); full page reload cleared TanStack cache so vitals trend fell back to demo data until new hub events arrived.

## Approach

1. **sessionStorage** — On each `sessionFeed` append, persist trimmed tail (max 20) keyed by tenant + `treatmentSessionId`.
2. **Query** — `enabled: !!sid`, `queryFn` returns parsed storage (or `[]`), `staleTime: Infinity`; SignalR remains source of truth via `setQueryData`.
3. **coerceSessionFeedPayload** — Accept `occurredAtUtc` as ISO string, `Date`, or finite epoch number.

## Files

- `clients/pdms-web/src/realtime/sessionFeedTailStorage.ts` (new)
- `clients/pdms-web/src/realtime/ClinicalFeedBridge.tsx`
- `clients/pdms-web/src/hooks/useSessionFeedTailQuery.ts`
- `clients/pdms-web/src/realtime/coerceSessionFeedPayload.ts`
