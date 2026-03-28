import { type ReactElement } from 'react'
import { useTenantAlertsFeedQuery } from '../../hooks/useTenantAlertsFeedQuery'

export function TenantAlertsFeedPanel(): ReactElement {
  const alertsQuery = useTenantAlertsFeedQuery()

  return (
    <section
      className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm lg:col-span-12"
      aria-labelledby="dash-tenant-alerts-title"
    >
      <h2 id="dash-tenant-alerts-title" className="text-base font-semibold text-slate-800">
        Tenant alerts (live)
      </h2>
      <p className="mt-1 text-sm text-slate-600">
        Push stream from <code className="rounded bg-slate-100 px-1">ClinicalFeedHub</code>; cache updated via{' '}
        <code className="rounded bg-slate-100 px-1">setQueryData</code> (no extra HTTP for each event).
      </p>
      {alertsQuery.data.length === 0 ? (
        <p className="mt-4 text-sm text-slate-500">No alert events yet for this browser session.</p>
      ) : (
        <ul className="mt-4 max-h-48 space-y-2 overflow-auto text-sm">
          {[...alertsQuery.data]
            .reverse()
            .slice(0, 12)
            .map((a) => (
              <li key={`${a.alertId}-${a.occurredAtUtc}`} className="rounded border border-slate-100 bg-slate-50 px-3 py-2">
                <div className="flex flex-wrap items-baseline justify-between gap-2">
                  <span className="font-medium text-slate-800">{a.severity}</span>
                  <time className="text-xs text-slate-500" dateTime={a.occurredAtUtc}>
                    {new Date(a.occurredAtUtc).toISOString()}
                  </time>
                </div>
                <p className="mt-1 text-slate-700">{a.eventType}</p>
                <p className="text-xs text-slate-500">
                  {a.lifecycleState}
                  {a.treatmentSessionId ? (
                    <>
                      {' '}
                      · session <span className="font-mono">{a.treatmentSessionId}</span>
                    </>
                  ) : null}
                </p>
              </li>
            ))}
        </ul>
      )}
    </section>
  )
}
