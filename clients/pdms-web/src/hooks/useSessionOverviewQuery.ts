import { useQuery } from '@tanstack/react-query'
import axios from 'axios'
import { fetchSessionOverview } from '../api/sessions'
import { isGatewayClientConfigured } from '../config/runtimeConfig'
import { queryKeys } from '../lib/queryKeys'
import { type SessionOverviewReadDto } from '../types/sessionOverview'

/** Read-model session overview — conservative cache for PHI-adjacent data. */
export function useSessionOverviewQuery(treatmentSessionId: string | null, enabledByRole: boolean) {
  const sid = treatmentSessionId?.trim() ?? ''
  return useQuery({
    queryKey: queryKeys.sessionOverview(sid),
    enabled: Boolean(isGatewayClientConfigured && sid.length > 0 && enabledByRole),
    staleTime: 120_000,
    gcTime: 10 * 60_000,
    queryFn: async (): Promise<SessionOverviewReadDto> => fetchSessionOverview(sid),
    retry: (failureCount, error) => {
      if (failureCount >= 2) return false
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        return false
      }
      return true
    },
  })
}
