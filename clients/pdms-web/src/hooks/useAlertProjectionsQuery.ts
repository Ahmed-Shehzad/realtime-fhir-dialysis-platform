import { useQuery } from '@tanstack/react-query'
import { fetchAlertProjections } from '../api/alerts'
import { isGatewayClientConfigured } from '../config/runtimeConfig'
import { queryKeys } from '../lib/queryKeys'
import { type AlertProjectionReadDto } from '../types/alertProjection'

/** GET /api/v…/alerts — drives session dropdown options (treatmentSessionId from projections). */
export function useAlertProjectionsQuery(severityFilter: string, enabledByRole: boolean) {
  const key = severityFilter.trim()
  return useQuery({
    queryKey: queryKeys.alertProjections(key),
    queryFn: async (): Promise<AlertProjectionReadDto[]> =>
      fetchAlertProjections(key.length > 0 ? key : null),
    enabled: Boolean(isGatewayClientConfigured && enabledByRole),
    staleTime: 30_000,
  })
}
