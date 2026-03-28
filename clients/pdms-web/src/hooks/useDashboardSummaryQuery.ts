import { useQuery } from '@tanstack/react-query'
import { fetchDashboardSummary } from '../api/dashboard'
import { isGatewayClientConfigured } from '../config/runtimeConfig'
import { queryKeys } from '../lib/queryKeys'

/** GET /api/v…/dashboard/summary (read model). */
export function useDashboardSummaryQuery(enabledByRole: boolean) {
  return useQuery({
    queryKey: queryKeys.dashboardSummary(),
    queryFn: fetchDashboardSummary,
    enabled: Boolean(isGatewayClientConfigured && enabledByRole),
    staleTime: 60_000,
  })
}
