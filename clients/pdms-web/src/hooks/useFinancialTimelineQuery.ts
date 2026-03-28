import { useQuery } from '@tanstack/react-query'
import { fetchFinancialTimeline } from '../api/financial'
import { isGatewayClientConfigured } from '../config/runtimeConfig'
import { queryKeys } from '../lib/queryKeys'
import { type FinancialSessionTimelineDto } from '../types/financialTimeline'

/** Financial interoperability timeline (coverage → claim → EOB). */
export function useFinancialTimelineQuery(
  treatmentSessionId: string | null,
  patientId: string | null,
  enabledByRole: boolean,
) {
  const sid = treatmentSessionId?.trim() ?? ''
  const pid = patientId?.trim() ?? null
  return useQuery({
    queryKey: queryKeys.financialTimeline(sid, pid),
    enabled: Boolean(isGatewayClientConfigured && sid.length > 0 && enabledByRole),
    staleTime: 120_000,
    gcTime: 10 * 60_000,
    queryFn: async (): Promise<FinancialSessionTimelineDto> => fetchFinancialTimeline(sid, pid),
  })
}
