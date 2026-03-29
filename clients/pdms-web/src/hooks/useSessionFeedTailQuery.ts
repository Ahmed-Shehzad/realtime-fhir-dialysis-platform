import { useQuery } from '@tanstack/react-query'
import { useMemo } from 'react'
import { queryKeys } from '../lib/queryKeys'
import { normalizeTreatmentSessionId } from '../lib/normalizeTreatmentSessionId'
import { readSessionFeedTailFromStorage } from '../realtime/sessionFeedTailStorage'
import { type SessionFeedPayload } from '../types/clinicalFeed'

const emptyFeed: SessionFeedPayload[] = []

/**
 * Updates come only from SignalR (`ClinicalFeedBridge` → `setQueryData`), which also mirrors the tail to
 * sessionStorage. `initialData` is seeded from storage (per session id) so vitals trend survives refresh without
 * an async refetch that could clobber newer hub events.
 */
export function useSessionFeedTailQuery(treatmentSessionId: string) {
  const sid = normalizeTreatmentSessionId(treatmentSessionId)
  const initialFromStorage = useMemo(
    () => (sid.length > 0 ? readSessionFeedTailFromStorage(sid) : emptyFeed),
    [sid],
  )
  return useQuery({
    queryKey: queryKeys.sessionFeedTail(sid.length > 0 ? sid : '_'),
    queryFn: () => Promise.resolve(emptyFeed),
    enabled: false,
    initialData: initialFromStorage,
    staleTime: Number.POSITIVE_INFINITY,
    structuralSharing: false,
  })
}
