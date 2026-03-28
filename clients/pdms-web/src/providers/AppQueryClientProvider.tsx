import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import axios from 'axios'
import { type ReactElement, type ReactNode, useState } from 'react'

function shouldRetry(failureCount: number, error: unknown): boolean {
  if (failureCount >= 3) {
    return false
  }
  if (!axios.isAxiosError(error)) {
    return true
  }
  const status = error.response?.status
  if (status === 401 || status === 403) {
    return false
  }
  if (status !== undefined && status < 500) {
    return false
  }
  return true
}

function createQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 30_000,
        gcTime: 5 * 60_000,
        retry: shouldRetry,
        refetchOnWindowFocus: true,
        refetchOnReconnect: true,
      },
      mutations: {
        retry: shouldRetry,
      },
    },
  })
}

export function AppQueryClientProvider({ children }: { children: ReactNode }): ReactElement {
  const [client] = useState(createQueryClient)
  return <QueryClientProvider client={client}>{children}</QueryClientProvider>
}
