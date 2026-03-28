import { type ReactElement } from 'react'
import type { UseQueryResult } from '@tanstack/react-query'
import { gatewayRequestOriginKey, isGatewayClientConfigured, isViteDevelopment, viteMode } from '../../config/runtimeConfig'
import { getDevStackAlignment } from '../../lib/devStackAlignment'
import { type GatewayHealthResponse } from '../../types/gatewayHealth'

type HealthCardProps = {
  query: UseQueryResult<GatewayHealthResponse>
}

export function HealthCard({ query }: HealthCardProps): ReactElement {
  const alignment = query.isSuccess ? getDevStackAlignment(query.data) : null

  return (
    <section
      className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm lg:col-span-4"
      aria-labelledby="dash-health-title"
    >
      <h2 id="dash-health-title" className="text-base font-semibold text-slate-800">
        API health
      </h2>
      <p className="mt-1 text-sm text-slate-600">
        TanStack Query retries and background refetch. In development,{' '}
        <code className="rounded bg-slate-100 px-1">/health</code> is proxied to YARP unless{' '}
        <code className="rounded bg-slate-100 px-1">VITE_API_BASE_URL</code> is set.
      </p>
      <p className="mt-2 text-xs text-slate-500">
        Client: <span className="font-medium text-slate-700">{isViteDevelopment ? 'development' : viteMode}</span>
        {' · '}
        API origin key:{' '}
        <span className="font-mono text-slate-700">{gatewayRequestOriginKey || '—'}</span>
      </p>
      <div className="mt-4 text-sm">
        {!isGatewayClientConfigured && (
          <p className="text-amber-800">
            Production build has no gateway URL — set <code className="font-mono">VITE_API_BASE_URL</code>{' '}
            (HTTPS YARP origin).
          </p>
        )}
        {query.isLoading && <p className="text-slate-600">Loading health…</p>}
        {query.isError && (
          <p className="text-red-700" role="alert">
            Health check failed. Verify HTTPS, CORS, and network.
          </p>
        )}
        {alignment && (
          <p
            className={
              alignment.ok
                ? 'mb-3 rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-emerald-900'
                : 'mb-3 rounded-lg border border-amber-200 bg-amber-50 px-3 py-2 text-amber-950'
            }
            role="status"
          >
            <span className="font-medium">Environment check: </span>
            {alignment.summary}
            {alignment.detail ? (
              <>
                <br />
                <span className="mt-1 block text-sm opacity-90">{alignment.detail}</span>
              </>
            ) : null}
          </p>
        )}
        {query.isSuccess && (
          <pre className="max-h-40 overflow-auto rounded-lg bg-slate-900 p-3 text-xs text-slate-100">
            {JSON.stringify(query.data, null, 2)}
          </pre>
        )}
      </div>
    </section>
  )
}
