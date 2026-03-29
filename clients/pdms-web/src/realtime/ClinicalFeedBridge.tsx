import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/react-query'
import { useEffect, useRef, type ReactElement } from 'react'
import { useSearchParams } from 'react-router'
import { getHttpAccessToken } from '../api/httpClient'
import { useSession } from '../auth/useSession'
import { realtimeFeedRoles } from '../auth/rolePolicies'
import { isGatewayClientConfigured, runtimeConfig } from '../config/runtimeConfig'
import { normalizeTreatmentSessionId } from '../lib/normalizeTreatmentSessionId'
import { queryKeys } from '../lib/queryKeys'
import { type AlertFeedPayload, type SessionFeedPayload } from '../types/clinicalFeed'
import { coerceSessionFeedPayload } from './coerceSessionFeedPayload'
import {
  SESSION_FEED_TAIL_MAX_ITEMS,
  writeSessionFeedTailToStorage,
} from './sessionFeedTailStorage'
import { type SessionOverviewReadDto } from '../types/sessionOverview'
import { useRealtimeHubConnectionState } from './RealtimeHubContext'

const TENANT_ALERTS_FEED_MAX = 50

function clinicalFeedHubUrl(): string {
  // In dev, always use same-origin /hubs so Vite proxies to the gateway (vite.config.ts). Otherwise a direct
  // http://localhost:5100 hub URL cross-origin negotiate can fail with "Failed to fetch" even when REST works.
  if (import.meta.env.DEV) {
    return '/hubs/clinical-feed'
  }
  const base = runtimeConfig.apiBaseUrl.replace(/\/$/, '')
  if (base.length > 0) {
    return `${base}/hubs/clinical-feed`
  }
  return '/hubs/clinical-feed'
}

/**
 * Singleton connection lifecycle per mount: joins session + tenant groups and pushes SignalR payloads into TanStack Query via setQueryData.
 */
export function ClinicalFeedBridge(): ReactElement | null {
  const queryClient = useQueryClient()
  const { setConnectionState } = useRealtimeHubConnectionState()
  const [searchParams] = useSearchParams()
  const { roles } = useSession()
  const sessionIdFromUrl = normalizeTreatmentSessionId(searchParams.get('sessionId'))

  const feedOptsRef = useRef({
    sessionId: '',
    joinTenantAlerts: false,
  })

  const mayUseFeed =
    isGatewayClientConfigured &&
    (import.meta.env.DEV || realtimeFeedRoles.some((r) => roles.includes(r)))

  const joinTenantAlerts =
    import.meta.env.DEV || realtimeFeedRoles.some((r) => roles.includes(r))

  useEffect(() => {
    feedOptsRef.current = {
      sessionId: sessionIdFromUrl,
      joinTenantAlerts,
    }
  }, [sessionIdFromUrl, joinTenantAlerts])

  useEffect(() => {
    if (!mayUseFeed) {
      setConnectionState({ kind: 'off' })
      return
    }

    setConnectionState({ kind: 'connecting' })

    const hubUrl = clinicalFeedHubUrl()
    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: async () => getHttpAccessToken() ?? '',
        ...(runtimeConfig.tenantId ? { headers: { 'X-Tenant-Id': runtimeConfig.tenantId } } : {}),
      })
      .withAutomaticReconnect()
      .build()

    const onSessionFeed = (raw: unknown) => {
      const payload = coerceSessionFeedPayload(raw)
      if (!payload) {
        return
      }

      const sid = normalizeTreatmentSessionId(payload.treatmentSessionId)
      if (!sid) {
        return
      }

      const hasNonEmpty = (value: string | null | undefined) =>
        value != null && value.trim().length > 0

      const hasPatientPreview =
        hasNonEmpty(payload.patientDisplayLabel) ||
        hasNonEmpty(payload.sessionStateHint) ||
        hasNonEmpty(payload.linkedDeviceIdHint)

      queryClient.setQueryData<SessionOverviewReadDto>(queryKeys.sessionOverview(sid), (prev) => {
        if (!prev) {
          if (!hasPatientPreview) {
            return prev
          }
          return {
            id: sid,
            treatmentSessionId: sid,
            sessionState: hasNonEmpty(payload.sessionStateHint) ? payload.sessionStateHint!.trim() : 'Active',
            patientDisplayLabel: hasNonEmpty(payload.patientDisplayLabel) ? payload.patientDisplayLabel!.trim() : null,
            linkedDeviceId: hasNonEmpty(payload.linkedDeviceIdHint) ? payload.linkedDeviceIdHint!.trim() : null,
            sessionStartedAtUtc: payload.occurredAtUtc,
            projectionUpdatedAtUtc: payload.occurredAtUtc,
          }
        }
        if (prev.treatmentSessionId !== sid) {
          return prev
        }
        return {
          ...prev,
          projectionUpdatedAtUtc: payload.occurredAtUtc,
          ...(hasNonEmpty(payload.patientDisplayLabel)
            ? { patientDisplayLabel: payload.patientDisplayLabel!.trim() }
            : {}),
          ...(hasNonEmpty(payload.sessionStateHint) ? { sessionState: payload.sessionStateHint!.trim() } : {}),
          ...(hasNonEmpty(payload.linkedDeviceIdHint)
            ? { linkedDeviceId: payload.linkedDeviceIdHint!.trim() }
            : {}),
        }
      })

      queryClient.setQueryData<SessionFeedPayload[]>(queryKeys.sessionFeedTail(sid), (prev) => {
        const list = prev ?? []
        const next = [...list, payload].slice(-SESSION_FEED_TAIL_MAX_ITEMS)
        writeSessionFeedTailToStorage(sid, next)
        return next
      })
    }

    const onAlertFeed = (payload: AlertFeedPayload) => {
      queryClient.setQueryData<AlertFeedPayload[]>(queryKeys.tenantAlertsFeed(), (prev) => {
        const list = prev ?? []
        return [...list, payload].slice(-TENANT_ALERTS_FEED_MAX)
      })
    }

    connection.on('sessionFeed', onSessionFeed)
    connection.on('alertFeed', onAlertFeed)

    const joinGroupsOnly = async (): Promise<void> => {
      const opts = feedOptsRef.current
      if (connection.state !== HubConnectionState.Connected) {
        return
      }
      if (opts.joinTenantAlerts) {
        try {
          await connection.invoke('JoinTenantAlerts')
        } catch (err) {
          console.warn('ClinicalFeedBridge: JoinTenantAlerts failed', err)
        }
      }
      if (opts.sessionId.length > 0) {
        try {
          await connection.invoke('JoinSessionFeed', opts.sessionId)
        } catch (err) {
          console.warn('ClinicalFeedBridge: JoinSessionFeed failed', err)
        }
      }
    }

    connection.onreconnecting(() => {
      setConnectionState({ kind: 'reconnecting' })
    })

    connection.onreconnected(() => {
      setConnectionState({ kind: 'connected' })
      void (async () => {
        await joinGroupsOnly()
        const opts = feedOptsRef.current
        if (opts.sessionId.length > 0) {
          await queryClient.invalidateQueries({ queryKey: queryKeys.sessionOverview(opts.sessionId), exact: true })
        }
      })()
    })

    let cancelled = false
    let connectFrameHandle: number | null = null

    const runConnect = async (): Promise<void> => {
      try {
        await connection.start()
        if (cancelled) {
          await connection.stop()
          return
        }
        await joinGroupsOnly()
        setConnectionState({ kind: 'connected' })
      } catch (error) {
        if (cancelled) {
          return
        }
        const message = error instanceof Error ? error.message : String(error)
        setConnectionState({ kind: 'failed', message })
        console.warn('ClinicalFeedBridge: hub connection failed', error)
      }
    }

    // React StrictMode runs effect cleanup before the next paint; deferring start avoids
    // calling stop() while negotiate is in flight (SignalR: "connection was stopped during negotiation").
    connectFrameHandle = window.requestAnimationFrame(() => {
      connectFrameHandle = null
      void runConnect()
    })

    return () => {
      cancelled = true
      if (connectFrameHandle !== null) {
        window.cancelAnimationFrame(connectFrameHandle)
        connectFrameHandle = null
      }
      setConnectionState({ kind: 'off' })
      connection.off('sessionFeed', onSessionFeed)
      connection.off('alertFeed', onAlertFeed)
      void connection.stop()
    }
  }, [queryClient, mayUseFeed, sessionIdFromUrl, joinTenantAlerts, setConnectionState])

  return null
}
