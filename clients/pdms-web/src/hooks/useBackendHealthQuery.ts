import { useQuery } from '@tanstack/react-query'
import { fetchGatewayHealth } from '../api/health'
import { isGatewayClientConfigured } from '../config/runtimeConfig'
import { queryKeys } from '../lib/queryKeys'

/** Gateway /health — refresh often; safe for shorter stale window. */
export function useBackendHealthQuery() {
  return useQuery({
    queryKey: queryKeys.health(),
    enabled: isGatewayClientConfigured,
    staleTime: 10_000,
    refetchInterval: 60_000,
    queryFn: fetchGatewayHealth,
  })
}
