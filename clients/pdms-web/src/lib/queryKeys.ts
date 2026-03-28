import { gatewayRequestOriginKey, runtimeConfig } from '../config/runtimeConfig'

const base = ['pdms', runtimeConfig.tenantId ?? 'no-tenant'] as const

export const queryKeys = {
  health: () => [...base, 'health', gatewayRequestOriginKey] as const,
  dashboardSummary: () => [...base, 'dashboard', 'summary', gatewayRequestOriginKey] as const,
  alertProjections: (severityFilter: string) =>
    [...base, 'alerts', 'projections', severityFilter || 'all', gatewayRequestOriginKey] as const,
  sessionOverview: (sessionId: string) => [...base, 'sessions', sessionId, 'overview'] as const,
  sessionFeedTail: (sessionId: string) => [...base, 'sessions', sessionId, 'feed-tail'] as const,
  tenantAlertsFeed: () => [...base, 'tenant-alerts-feed', gatewayRequestOriginKey] as const,
  financialTimeline: (sessionId: string, patientId: string | null) =>
    [...base, 'financial', 'timeline', sessionId, patientId ?? ''] as const,
} as const
