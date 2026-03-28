import { useQuery } from '@tanstack/react-query'
import { queryKeys } from '../lib/queryKeys'
import { type AlertFeedPayload } from '../types/clinicalFeed'

const emptyAlerts: AlertFeedPayload[] = []

/** Populated only via SignalR + setQueryData; `queryFn` satisfies TanStack Query v5 when `enabled` is false. */
export function useTenantAlertsFeedQuery() {
  return useQuery({
    queryKey: queryKeys.tenantAlertsFeed(),
    queryFn: () => Promise.resolve(emptyAlerts),
    enabled: false,
    initialData: emptyAlerts,
    staleTime: Number.POSITIVE_INFINITY,
  })
}
