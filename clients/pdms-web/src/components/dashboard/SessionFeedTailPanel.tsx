import { type ReactElement } from 'react'
import { useSessionFeedTailQuery } from '../../hooks/useSessionFeedTailQuery'

type SessionFeedTailPanelProps = {
  treatmentSessionId: string
}

/** Live session feed events from ClinicalFeedBridge (`sessionFeed` → TanStack Query). */
export function SessionFeedTailPanel(props: SessionFeedTailPanelProps): ReactElement {
  const sid = props.treatmentSessionId.trim()
  const feedQuery = useSessionFeedTailQuery(sid)

  if (!sid) {
    return (
      <section
        className="rounded-xl border border-dashed border-slate-300 bg-white p-6 text-sm text-slate-600 lg:col-span-12"
        aria-label="Session feed idle"
      >
        Select or set <span className="font-mono">?sessionId=</span> to join the session SignalR group and see live feed
        events here.
      </section>
    )
  }

  return (
    <section
      className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm lg:col-span-12"
      aria-labelledby="dash-session-feed-title"
    >
      <h2 id="dash-session-feed-title" className="text-base font-semibold text-slate-800">
        Session feed (live)
      </h2>
      <p className="mt-1 text-sm text-slate-600">
        Events for <span className="font-mono text-xs">{sid.length > 36 ? `${sid.slice(0, 18)}…${sid.slice(-12)}` : sid}</span>{' '}
        from <code className="rounded bg-slate-100 px-1">sessionFeed</code>; requires hub connected and{' '}
        <code className="rounded bg-slate-100 px-1">JoinSessionFeed</code>.
      </p>
      {feedQuery.data.length === 0 ? (
        <p className="mt-4 text-sm text-slate-500">
          No session events yet. Run <code className="rounded bg-slate-100 px-1">scenario run</code> or{' '}
          <code className="rounded bg-slate-100 px-1">broadcast session</code> for this session id.
        </p>
      ) : (
        <ul className="mt-4 max-h-48 space-y-2 overflow-auto text-sm">
          {[...feedQuery.data]
            .reverse()
            .slice(0, 12)
            .map((e, index) => (
              <li
                key={`${e.occurredAtUtc}-${e.eventType}-${index}`}
                className="rounded border border-slate-100 bg-slate-50 px-3 py-2"
              >
                <div className="flex flex-wrap items-baseline justify-between gap-2">
                  <span className="font-medium text-slate-800">{e.eventType}</span>
                  <time className="text-xs text-slate-500" dateTime={e.occurredAtUtc}>
                    {new Date(e.occurredAtUtc).toISOString()}
                  </time>
                </div>
                <p className="mt-1 text-slate-700">{e.summary}</p>
              </li>
            ))}
        </ul>
      )}
    </section>
  )
}
