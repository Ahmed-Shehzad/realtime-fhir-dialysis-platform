import { useEffect, useMemo, useState, type ReactElement, type SubmitEvent } from 'react'
import { useSearchParams } from 'react-router'
import { useSession } from '../../auth/useSession'
import { sessionOverviewRoles } from '../../auth/rolePolicies'
import { useAlertProjectionsQuery } from '../../hooks/useAlertProjectionsQuery'
import { useDashboardSummaryQuery } from '../../hooks/useDashboardSummaryQuery'

const SEVERITY_FILTER_OPTIONS: readonly { value: string; label: string }[] = [
  { value: '', label: 'All severities' },
  { value: 'Critical', label: 'Critical' },
  { value: 'High', label: 'High' },
  { value: 'Medium', label: 'Medium' },
  { value: 'Low', label: 'Low' },
  { value: 'Info', label: 'Info' },
] as const

/**
 * Read-model backed dropdowns: severity filter refetches alert projections; session options come from
 * distinct treatmentSessionId values. Selection updates URL query so SessionSummary, financial, SignalR, etc. stay in sync.
 */
export function DashboardContextSelectors(): ReactElement {
  const { roles } = useSession()
  const [searchParams, setSearchParams] = useSearchParams()
  const sessionIdFromUrl = searchParams.get('sessionId') ?? ''
  const patientIdFromUrl = searchParams.get('patientId') ?? ''

  const hasReadModelRole = sessionOverviewRoles.some((r) => roles.includes(r))
  const [severityFilter, setSeverityFilter] = useState('')
  const [patientDraft, setPatientDraft] = useState(patientIdFromUrl)

  useEffect(() => {
    setPatientDraft(patientIdFromUrl)
  }, [patientIdFromUrl])

  const summaryQuery = useDashboardSummaryQuery(hasReadModelRole)
  const alertsQuery = useAlertProjectionsQuery(severityFilter, hasReadModelRole)

  const sessionOptions = useMemo(() => {
    const ids = new Set<string>()
    for (const row of alertsQuery.data ?? []) {
      const sid = row.treatmentSessionId?.trim()
      if (sid) {
        ids.add(sid)
      }
    }
    if (sessionIdFromUrl.trim()) {
      ids.add(sessionIdFromUrl.trim())
    }
    return [...ids].sort((a, b) => a.localeCompare(b))
  }, [alertsQuery.data, sessionIdFromUrl])

  const onSessionChange = (nextSessionId: string) => {
    const next = new URLSearchParams(searchParams)
    if (nextSessionId.trim()) {
      next.set('sessionId', nextSessionId.trim())
    } else {
      next.delete('sessionId')
    }
    setSearchParams(next, { replace: true })
  }

  const onSeverityChange = (value: string) => {
    setSeverityFilter(value)
  }

  const onApplyPatient = (event: SubmitEvent<HTMLFormElement>) => {
    event.preventDefault()
    const next = new URLSearchParams(searchParams)
    const pid = patientDraft.trim()
    if (pid) {
      next.set('patientId', pid)
    } else {
      next.delete('patientId')
    }
    setSearchParams(next, { replace: true })
  }

  if (!hasReadModelRole) {
    return (
      <div className="rounded-lg border border-dashed border-slate-300 bg-slate-50 p-3 text-sm text-slate-600">
        Load read-model dropdowns with{' '}
        <code className="rounded bg-white px-1">VITE_APP_ROLES=readmodel.read</code> (or mapped Dialysis scopes).
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-3 rounded-lg border border-slate-200 bg-slate-50 p-3">
      <div className="flex flex-wrap items-center gap-3 text-sm">
        <span className="font-medium text-slate-700">Read model</span>
        {summaryQuery.isLoading && <span className="text-slate-500">Loading summary…</span>}
        {summaryQuery.isError && <span className="text-amber-800">Summary unavailable</span>}
        {summaryQuery.isSuccess && (
          <span className="text-slate-600">
            <span className="rounded bg-white px-2 py-0.5 ring-1 ring-slate-200">
              Active sessions: {summaryQuery.data.activeSessionCount}
            </span>
            <span className="mx-2 text-slate-400">·</span>
            <span className="rounded bg-white px-2 py-0.5 ring-1 ring-slate-200">
              Open alerts: {summaryQuery.data.openAlertCount}
            </span>
          </span>
        )}
      </div>

      <div className="flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-end">
        <label className="flex min-w-40 flex-1 flex-col gap-1" htmlFor="dash-severity-filter">
          <span className="text-xs font-medium text-slate-600">Alert severity filter</span>
          <select
            id="dash-severity-filter"
            value={severityFilter}
            onChange={(e) => {
              onSeverityChange(e.target.value)
            }}
            className="rounded border border-slate-300 bg-white px-2 py-1.5 text-sm"
          >
            {SEVERITY_FILTER_OPTIONS.map((opt) => (
              <option key={opt.value || 'all'} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </select>
          <span className="text-xs text-slate-500">Refines which alerts load; session list is derived from matching rows.</span>
        </label>

        <label className="flex min-w-56 flex-1 flex-col gap-1" htmlFor="dash-session-select">
          <span className="text-xs font-medium text-slate-600">Treatment session</span>
          <select
            id="dash-session-select"
            value={sessionIdFromUrl}
            onChange={(e) => {
              onSessionChange(e.target.value)
            }}
            disabled={alertsQuery.isLoading}
            className="rounded border border-slate-300 bg-white px-2 py-1.5 font-mono text-xs disabled:opacity-60"
          >
            <option value="">— Select session —</option>
            {sessionOptions.map((sid) => (
              <option key={sid} value={sid}>
                {sid.length > 28 ? `${sid.slice(0, 14)}…${sid.slice(-10)}` : sid}
              </option>
            ))}
          </select>
          {alertsQuery.isLoading && <span className="text-xs text-slate-500">Loading sessions from alerts…</span>}
          {alertsQuery.isError && <span className="text-xs text-red-700">Could not load alerts for dropdown.</span>}
        </label>

        <form
          className="flex min-w-48 flex-1 flex-col gap-1 sm:max-w-xs"
          onSubmit={onApplyPatient}
        >
          <label htmlFor="dash-patient-context" className="text-xs font-medium text-slate-600">
            Patient id (financial timeline)
          </label>
          <div className="flex gap-2">
            <input
              id="dash-patient-context"
              value={patientDraft}
              onChange={(e) => {
                setPatientDraft(e.target.value)
              }}
              className="min-w-0 flex-1 rounded border border-slate-300 bg-white px-2 py-1.5 text-sm"
              placeholder="MRN / platform id"
              autoComplete="off"
            />
            <button
              type="submit"
              className="shrink-0 rounded bg-slate-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-slate-800"
            >
              Apply
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
