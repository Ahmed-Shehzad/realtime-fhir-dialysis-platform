import type { UseQueryResult } from '@tanstack/react-query'
import axios from 'axios'
import { type ReactElement } from 'react'
import { useSessionFeedTailQuery } from '../../hooks/useSessionFeedTailQuery'
import { type SessionOverviewReadDto } from '../../types/sessionOverview'

type SessionSummaryCardProps = {
  sessionIdInput: string
  query: UseQueryResult<SessionOverviewReadDto>
}

export function SessionSummaryCard({ sessionIdInput, query }: SessionSummaryCardProps): ReactElement {
  const feedTailQuery = useSessionFeedTailQuery(sessionIdInput)

  return (
    <section
      className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm lg:col-span-8"
      aria-labelledby="dash-session-title"
    >
      <h2 id="dash-session-title" className="text-base font-semibold text-slate-800">
        Session overview
      </h2>
      <p className="mt-1 text-sm text-slate-600">
        Read model: <code className="rounded bg-slate-100 px-1">GET /api/v1/sessions/&#123;id&#125;/overview</code>
      </p>
      {!sessionIdInput.trim() && (
        <p className="mt-4 text-sm text-slate-500">Enter a treatment session id in the header to load overview.</p>
      )}
      {sessionIdInput.trim() && query.isLoading && <p className="mt-4 text-sm text-slate-600">Loading…</p>}
      {sessionIdInput.trim() && query.isError && (
        <p className="mt-4 text-sm text-red-700" role="alert">
          {(() => {
            const err = query.error
            return axios.isAxiosError(err) && err.response?.status === 404
              ? 'No overview for this session.'
              : 'Failed to load overview.'
          })()}
        </p>
      )}
      {query.isSuccess && query.data && (
        <dl className="mt-4 grid gap-3 text-sm sm:grid-cols-2">
          <div>
            <dt className="text-slate-500">State</dt>
            <dd className="font-medium">{query.data.sessionState}</dd>
          </div>
          <div>
            <dt className="text-slate-500">Session id</dt>
            <dd className="font-mono text-xs font-medium break-all">{query.data.treatmentSessionId}</dd>
          </div>
          <div>
            <dt className="text-slate-500">Patient label</dt>
            <dd className="font-medium">{query.data.patientDisplayLabel ?? '—'}</dd>
          </div>
          <div>
            <dt className="text-slate-500">Device</dt>
            <dd className="font-mono text-xs">{query.data.linkedDeviceId ?? '—'}</dd>
          </div>
          <div>
            <dt className="text-slate-500">Started (UTC)</dt>
            <dd className="font-medium">{new Date(query.data.sessionStartedAtUtc).toISOString()}</dd>
          </div>
          <div>
            <dt className="text-slate-500">Projection updated</dt>
            <dd className="font-medium">{new Date(query.data.projectionUpdatedAtUtc).toISOString()}</dd>
          </div>
        </dl>
      )}
      {sessionIdInput.trim() && feedTailQuery.data.length > 0 && (
        <div className="mt-4 border-t border-slate-200 pt-4" aria-label="Session feed events">
          <h3 className="text-sm font-medium text-slate-800">Recent session feed (SignalR)</h3>
          <ul className="mt-2 max-h-36 space-y-2 overflow-auto text-sm text-slate-700">
            {[...feedTailQuery.data]
              .reverse()
              .slice(0, 6)
              .map((item, index) => (
                <li key={`${item.occurredAtUtc}-${item.eventType}-${index}`}>
                  <span className="text-xs text-slate-500">
                    {new Date(item.occurredAtUtc).toISOString()}
                  </span>
                  <p className="mt-0.5">
                    <span className="font-medium">{item.eventType}</span>: {item.summary}
                  </p>
                </li>
              ))}
          </ul>
        </div>
      )}
    </section>
  )
}
