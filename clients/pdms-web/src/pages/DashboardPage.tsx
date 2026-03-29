import { useMemo, type ReactElement } from 'react'
import { Link, useSearchParams } from 'react-router'
import { useSession } from '../auth/useSession'
import {
  clinicalPhiRoles,
  financialTimelineRoles,
  realtimeFeedRoles,
  sessionOverviewRoles,
} from '../auth/rolePolicies'
import { RoleGate } from '../components/RoleGate'
import { DashboardContextSelectors } from '../components/dashboard/DashboardContextSelectors'
import { DashboardErrorBoundary } from '../components/dashboard/DashboardErrorBoundary'
import { FinancialTimelinePanel } from '../components/dashboard/FinancialTimelinePanel'
import { HealthCard } from '../components/dashboard/HealthCard'
import { RealtimeHubBanner } from '../components/dashboard/RealtimeHubBanner'
import { SessionSummaryCard } from '../components/dashboard/SessionSummaryCard'
import { SessionFeedTailPanel } from '../components/dashboard/SessionFeedTailPanel'
import { TenantAlertsFeedPanel } from '../components/dashboard/TenantAlertsFeedPanel'
import {
  vitalSeriesWithDefaultColors,
  VitalsTrendChart,
  type VitalSeriesDefinition,
} from '../components/VitalsTrendChart'
import type { VitalPoint } from '../components/VitalsLineChart'
import { runtimeConfig } from '../config/runtimeConfig'
import { normalizeTreatmentSessionId } from '../lib/normalizeTreatmentSessionId'
import { useBackendHealthQuery } from '../hooks/useBackendHealthQuery'
import { useFinancialTimelineQuery } from '../hooks/useFinancialTimelineQuery'
import { useLiveVitalsTrendFromSessionFeed } from '../hooks/useLiveVitalsTrendFromSessionFeed'
import { useSessionFeedTailQuery } from '../hooks/useSessionFeedTailQuery'
import { useSessionOverviewQuery } from '../hooks/useSessionOverviewQuery'

function buildDemoVitalsMultiSeries(): VitalSeriesDefinition[] {
  const now = Date.now()
  const n = 36
  const times = Array.from({ length: n }, (_, index) => new Date(now - (n - 1 - index) * 2_400_000))

  const mapPoints: VitalPoint[] = times.map((t, index) => ({
    t,
    value: 72 + Math.sin(index / 4) * 10 + index * 0.12 + (index > 22 ? (index - 22) * 0.8 : 0),
  }))

  const hrPoints: VitalPoint[] = times.map((t, index) => ({
    t,
    value: 68 + Math.sin(index / 2.5) * 12 + (index % 5) * 2,
  }))

  const spo2Points: VitalPoint[] = times.map((t, index) => ({
    t,
    value: 97 + Math.sin(index / 6) * 1.2 - (index > 26 ? (index - 26) * 0.15 : 0),
  }))

  return vitalSeriesWithDefaultColors([
    { id: 'map', label: 'Mean arterial pressure', unit: 'mmHg', points: mapPoints },
    { id: 'hr', label: 'Heart rate', unit: '/min', points: hrPoints },
    { id: 'spo2', label: 'SpO₂ (estimated)', unit: '%', points: spo2Points },
  ])
}

export default function DashboardPage(): ReactElement {
  const { roles } = useSession()
  const [searchParams] = useSearchParams()
  const sessionIdFromUrl = normalizeTreatmentSessionId(searchParams.get('sessionId'))
  const patientIdFromUrl = searchParams.get('patientId') ?? ''

  const healthQuery = useBackendHealthQuery()
  const showRealtimeFeedUi =
    import.meta.env.DEV || realtimeFeedRoles.some((r) => roles.includes(r))
  const hasSessionOverviewRole = sessionOverviewRoles.some((r) => roles.includes(r))
  const hasClinicalPhiRole = clinicalPhiRoles.some((r) => roles.includes(r))
  const sessionOverviewQueryEnabled = hasSessionOverviewRole || hasClinicalPhiRole
  const sessionOverviewQuery = useSessionOverviewQuery(sessionIdFromUrl || null, sessionOverviewQueryEnabled)

  const hasFinancialRole = financialTimelineRoles.some((r) => roles.includes(r))
  const financialQuery = useFinancialTimelineQuery(
    sessionIdFromUrl || null,
    patientIdFromUrl || null,
    hasFinancialRole,
  )

  const demoVitalsSeries = useMemo(() => buildDemoVitalsMultiSeries(), [])
  const sessionFeedForVitalsQuery = useSessionFeedTailQuery(sessionIdFromUrl)
  const liveVitalsSeries = useLiveVitalsTrendFromSessionFeed(sessionIdFromUrl, sessionFeedForVitalsQuery.data)
  const vitalsSeries = liveVitalsSeries ?? demoVitalsSeries

  const envBadge = import.meta.env.PROD ? 'production' : 'development'

  return (
    <div className="min-h-screen bg-slate-50 text-slate-900">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-6xl flex-col gap-4 px-6 py-4">
          <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <nav aria-label="Primary" className="mb-1">
                <Link to="/" className="text-sm font-medium text-blue-700 hover:underline">
                  Dashboard
                </Link>
              </nav>
              <h1 className="text-lg font-semibold tracking-tight">PDMS dashboard</h1>
              <p className="text-sm text-slate-600">
                Single YARP gateway. Development: Vite proxies <code className="rounded bg-slate-100 px-1">/health</code>{' '}
                and <code className="rounded bg-slate-100 px-1">/api</code> to{' '}
                <code className="rounded bg-slate-100 px-1">http://localhost:5100</code> (run the gateway). Set{' '}
                <code className="rounded bg-slate-100 px-1">VITE_API_BASE_URL</code> to call the gateway directly
                instead. Production builds require that variable (HTTPS).
              </p>
            </div>
            <div className="flex flex-col items-start gap-1 text-xs text-slate-500 sm:items-end">
              <span
                className="rounded-full bg-slate-200 px-2 py-0.5 font-medium text-slate-700"
                title="Build environment"
              >
                {envBadge}
              </span>
              <span aria-live="polite">UI roles: {roles.length ? roles.join(', ') : 'none'}</span>
            </div>
          </div>
          <DashboardContextSelectors />
        </div>
      </header>

      <main id="main-content" className="mx-auto max-w-6xl px-6 py-8">
        <DashboardErrorBoundary>
          <div className="grid grid-cols-1 gap-6 lg:grid-cols-12">
            <HealthCard query={healthQuery} />

            <RealtimeHubBanner show={showRealtimeFeedUi} />

            {showRealtimeFeedUi ? (
              <>
                <SessionFeedTailPanel treatmentSessionId={sessionIdFromUrl} />
                <TenantAlertsFeedPanel />
              </>
            ) : (
              <section
                className="rounded-xl border border-dashed border-slate-300 bg-white p-6 text-sm text-slate-600 lg:col-span-12"
                aria-label="Realtime feed restricted"
              >
                Live SignalR panels match hub auth: use{' '}
                <code className="rounded bg-slate-100 px-1">VITE_APP_ROLES=delivery.read</code> or{' '}
                <code className="rounded bg-slate-100 px-1">VITE_DIALYSIS_SCOPES=Dialysis.Delivery.Read</code> in
                production builds.
              </section>
            )}

            <RoleGate
              anyOf={[...sessionOverviewRoles]}
              fallback={
                <section
                  className="rounded-xl border border-dashed border-slate-300 bg-white p-6 text-sm text-slate-600 lg:col-span-8"
                  aria-label="Session overview restricted"
                >
                  Session overview requires read model access. Use{' '}
                  <code className="rounded bg-slate-100 px-1">VITE_APP_ROLES=readmodel.read</code> or{' '}
                  <code className="rounded bg-slate-100 px-1">VITE_DIALYSIS_SCOPES=Dialysis.ReadModel.Read</code>.
                </section>
              }
            >
              <SessionSummaryCard sessionIdInput={sessionIdFromUrl} query={sessionOverviewQuery} />
            </RoleGate>

            <section
              className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm lg:col-span-12"
              aria-labelledby="dash-vitals-title"
            >
              <h2 id="dash-vitals-title" className="text-base font-semibold text-slate-800">
                Vitals trend
              </h2>
              <p className="mt-1 max-w-3xl text-sm text-slate-600">
                Multi-series (Bokeh-style): dual Y-axes when units differ, grid, zoom/pan, hover crosshair + tooltip,
                and a toggleable legend. Continuous ingest: each simulator tick POSTs measurements and one{' '}
                <span className="font-mono text-xs">sessionFeed</span> event (<span className="font-mono text-xs">
                  Simulation.VitalsTrend
                </span>
                , <span className="font-mono text-xs">vitalsByChannel</span>) — run{' '}
                <code className="rounded bg-slate-100 px-1">simulate-gateway</code> with{' '}
                <code className="rounded bg-slate-100 px-1">SIMULATION_VITALS_STREAM_INTERVAL_MS</code> or{' '}
                <code className="rounded bg-slate-100 px-1">SIMULATION_CONTINUOUS_LIVE_STREAM=1</code>, paste{' '}
                <span className="font-mono text-xs">treatmentSessionId</span> into <span className="font-mono text-xs">
                  ?sessionId=
                </span>
                . SVG exposes <code className="rounded bg-slate-100 px-1">role=&quot;img&quot;</code> and{' '}
                <code className="rounded bg-slate-100 px-1">aria-label</code>.
              </p>
              <p className="mt-2 text-xs text-slate-500">
                {liveVitalsSeries
                  ? 'Live data from ClinicalFeedHub session feed (vitalsByChannel).'
                  : 'Sample data until vitals stream events arrive for the selected session.'}
                {sessionIdFromUrl ? (
                  <>
                    {' '}
                    Active context: <span className="font-mono text-slate-700">{sessionIdFromUrl}</span>
                  </>
                ) : null}
              </p>
              <div className="mt-4">
                <VitalsTrendChart
                  key={`vitals-${sessionIdFromUrl}-${liveVitalsSeries ? 'live' : 'demo'}`}
                  series={vitalsSeries}
                  resetZoomWhenTimeExtentGrows={liveVitalsSeries != null}
                  title="Hemodynamics & oxygenation (simulated session)"
                  subtitle={
                    liveVitalsSeries
                      ? 'Realtime points from SignalR sessionFeed (MAP, heart rate, SpO₂). Scroll/pan as needed.'
                      : 'With three unit types visible, one shared Y scale spans all traces (read units in the tooltip). Exactly two unit types use independent left and right axes.'
                  }
                />
              </div>
            </section>

            <RoleGate
              anyOf={[...financialTimelineRoles]}
              fallback={
                <section
                  className="rounded-xl border border-dashed border-slate-300 bg-white p-6 text-sm text-slate-600 lg:col-span-6"
                  aria-label="Financial timeline restricted"
                >
                  Financial timeline requires Financial.Read. Use{' '}
                  <code className="rounded bg-slate-100 px-1">VITE_APP_ROLES=financial.read</code> or{' '}
                  <code className="rounded bg-slate-100 px-1">VITE_DIALYSIS_SCOPES=Dialysis.Financial.Read</code>.
                </section>
              }
            >
              <div className="lg:col-span-6">
                <FinancialTimelinePanel sessionIdInput={sessionIdFromUrl} query={financialQuery} />
              </div>
            </RoleGate>

            <RoleGate
              anyOf={[...clinicalPhiRoles]}
              fallback={
                <section
                  className="rounded-xl border border-dashed border-slate-300 bg-white p-6 text-sm text-slate-600 lg:col-span-12"
                  aria-label="PHI gated"
                >
                  PHI strip hidden without clinical roles. Use{' '}
                  <code className="rounded bg-slate-100 px-1">VITE_APP_ROLES=clinical.read</code> or{' '}
                  <code className="rounded bg-slate-100 px-1">VITE_DIALYSIS_SCOPES=Dialysis.Interoperability.Read</code>.
                  Authorization remains server-side.
                </section>
              }
            >
              <section
                className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm lg:col-span-12"
                aria-labelledby="dash-phi-title"
              >
                <h2 id="dash-phi-title" className="text-base font-semibold text-slate-800">
                  Patient context (preview)
                </h2>
                <p className="mt-2 text-sm text-slate-700">
                  Wire to FHIR <code className="rounded bg-slate-100 px-1">Patient</code> after auth. The same continuous
                  simulator tick sends <code className="rounded bg-slate-100 px-1">patientDisplayLabel</code>,{' '}
                  <code className="rounded bg-slate-100 px-1">sessionStateHint</code>, and{' '}
                  <code className="rounded bg-slate-100 px-1">linkedDeviceIdHint</code> on{' '}
                  <code className="rounded bg-slate-100 px-1">sessionFeed</code> — merged into this strip in realtime (plus
                  read-model session overview when available).
                </p>
                <dl className="mt-4 grid gap-3 text-sm sm:grid-cols-2">
                  <div>
                    <dt className="text-slate-500">MRN</dt>
                    <dd className="font-medium">
                      {sessionOverviewQuery.data?.patientDisplayLabel
                        ? sessionOverviewQuery.data.patientDisplayLabel
                        : '**** (masked)'}
                    </dd>
                  </div>
                  <div>
                    <dt className="text-slate-500">Active session</dt>
                    <dd className="font-mono text-xs font-medium break-all">
                      {sessionIdFromUrl || '—'}
                    </dd>
                  </div>
                  <div>
                    <dt className="text-slate-500">Session state</dt>
                    <dd className="font-medium">{sessionOverviewQuery.data?.sessionState ?? '—'}</dd>
                  </div>
                  <div>
                    <dt className="text-slate-500">Linked device</dt>
                    <dd className="font-mono text-xs font-medium break-all">
                      {sessionOverviewQuery.data?.linkedDeviceId ?? '—'}
                    </dd>
                  </div>
                  <div className="sm:col-span-2">
                    <dt className="text-slate-500">Last feed merge (UTC)</dt>
                    <dd className="font-mono text-xs text-slate-700">
                      {sessionOverviewQuery.data?.projectionUpdatedAtUtc
                        ? new Date(sessionOverviewQuery.data.projectionUpdatedAtUtc).toISOString()
                        : '—'}
                    </dd>
                  </div>
                </dl>
              </section>
            </RoleGate>
          </div>
          {import.meta.env.PROD && !runtimeConfig.apiBaseUrl && (
            <p className="mt-6 text-center text-xs text-slate-500">
              Configure <code className="font-mono">VITE_API_BASE_URL</code> and optional{' '}
              <code className="font-mono">VITE_APP_TENANT_ID</code> for X-Tenant-Id.
            </p>
          )}
        </DashboardErrorBoundary>
      </main>
    </div>
  )
}
