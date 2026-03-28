import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/react-query'
import { useEffect, useRef, type ReactElement } from 'react'
import { useSearchParams } from 'react-router'
import { getHttpAccessToken } from '../api/httpClient'
import { useSession } from '../auth/useSession'
import { realtimeFeedRoles } from '../auth/rolePolicies'
import { isGatewayClientConfigured, runtimeConfig } from '../config/runtimeConfig'
import { queryKeys } from '../lib/queryKeys'
import { type AlertFeedPayload, type SessionFeedPayload } from '../types/clinicalFeed'
import { type SessionOverviewReadDto } from '../types/sessionOverview'
import { useRealtimeHubConnectionState } from './RealtimeHubContext'

const SESSION_FEED_TAIL_MAX = 20
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
  const sessionIdFromUrl = searchParams.get('sessionId')?.trim() ?? ''

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

    const onSessionFeed = (payload: SessionFeedPayload) => {
      const sid = payload.treatmentSessionId?.trim() ?? ''
      if (!sid) {
        return
      }

      queryClient.setQueryData<SessionOverviewReadDto>(queryKeys.sessionOverview(sid), (prev) => {
        if (!prev || prev.treatmentSessionId !== sid) {
          return prev
        }
        return {
          ...prev,
          projectionUpdatedAtUtc: payload.occurredAtUtc,
        }
      })

      queryClient.setQueryData<SessionFeedPayload[]>(queryKeys.sessionFeedTail(sid), (prev) => {
        const list = prev ?? []
        return [...list, payload].slice(-SESSION_FEED_TAIL_MAX)
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
        await connection.invoke('JoinTenantAlerts')
      }
      if (opts.sessionId.length > 0) {
        await connection.invoke('JoinSessionFeed', opts.sessionId)
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
