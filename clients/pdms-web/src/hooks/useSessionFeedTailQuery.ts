import { useQuery } from '@tanstack/react-query'
import { queryKeys } from '../lib/queryKeys'
import { type SessionFeedPayload } from '../types/clinicalFeed'

const emptyFeed: SessionFeedPayload[] = []

/** Populated only via SignalR + setQueryData; `queryFn` satisfies TanStack Query v5 when `enabled` is false. */
export function useSessionFeedTailQuery(treatmentSessionId: string) {
  const sid = treatmentSessionId.trim()
  return useQuery({
    queryKey: queryKeys.sessionFeedTail(sid.length > 0 ? sid : '_'),
    queryFn: () => Promise.resolve(emptyFeed),
    enabled: false,
    initialData: emptyFeed,
    staleTime: Number.POSITIVE_INFINITY,
  })
}
