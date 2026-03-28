import type { UseQueryResult } from '@tanstack/react-query'
import { type ReactElement } from 'react'
import { type FinancialSessionTimelineDto } from '../../types/financialTimeline'
import { ClaimStatusMiniChart } from './ClaimStatusMiniChart'

type FinancialTimelinePanelProps = {
  sessionIdInput: string
  query: UseQueryResult<FinancialSessionTimelineDto>
}

export function FinancialTimelinePanel({ sessionIdInput, query }: FinancialTimelinePanelProps): ReactElement {
  return (
    <section
      className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
      aria-labelledby="dash-financial-title"
    >
      <h2 id="dash-financial-title" className="text-base font-semibold text-slate-800">
        Financial journey
      </h2>
      <p className="mt-1 text-sm text-slate-600">
        Coverage, eligibility, claims, and EOB for the session (FHIR-aligned read model).
      </p>
      {!sessionIdInput.trim() && (
        <p className="mt-4 text-sm text-slate-500">Enter a session id to load the financial timeline.</p>
      )}
      {sessionIdInput.trim() && query.isLoading && <p className="mt-4 text-sm text-slate-600">Loading…</p>}
      {sessionIdInput.trim() && query.isError && (
        <p className="mt-4 text-sm text-red-700" role="alert">
          Failed to load financial data. Check Financial.Read scope and gateway routing.
        </p>
      )}
      {query.isSuccess && query.data && (
        <div className="mt-4 space-y-6">
          <div>
            <h3 className="text-sm font-medium text-slate-700">Resolved patient</h3>
            <p className="text-sm text-slate-600">{query.data.resolvedPatientId ?? '— (from first claim or query)'}</p>
          </div>
          <div>
            <h3 className="text-sm font-medium text-slate-700">Coverages ({query.data.coverages.length})</h3>
            <ul className="mt-2 max-h-32 space-y-1 overflow-auto text-xs text-slate-700">
              {query.data.coverages.length === 0 && <li className="text-slate-500">None</li>}
              {query.data.coverages.map((c) => (
                <li key={c.id}>
                  {c.planDisplayName} — {c.payorDisplayName}
                </li>
              ))}
            </ul>
          </div>
          <div>
            <h3 className="text-sm font-medium text-slate-700">Eligibility ({query.data.eligibilityInquiries.length})</h3>
            <ul className="mt-2 max-h-28 space-y-1 overflow-auto text-xs text-slate-700">
              {query.data.eligibilityInquiries.length === 0 && <li className="text-slate-500">None</li>}
              {query.data.eligibilityInquiries.map((e) => (
                <li key={e.id}>
                  {e.outcomeCode} ({e.status})
                </li>
              ))}
            </ul>
          </div>
          <div>
            <h3 className="text-sm font-medium text-slate-700">Claims</h3>
            {query.data.claims.length === 0 ? (
              <p className="mt-2 text-sm text-slate-500">No claims for this session.</p>
            ) : (
              <ul className="mt-2 space-y-4">
                {query.data.claims.map((c) => {
                  const eob = query.data.explanationOfBenefits.find((x) => x.dialysisFinancialClaimId === c.id)
                  return (
                    <li key={c.id} className="rounded-lg border border-slate-100 bg-slate-50 p-3">
                      <div className="flex flex-wrap items-center justify-between gap-2 text-xs">
                        <span className="font-mono">{c.id}</span>
                        <span>
                          {c.status} · {c.claimUse}
                        </span>
                      </div>
                      <ClaimStatusMiniChart
                        claimStatus={c.status}
                        hasEob={Boolean(eob)}
                        label={`Claim ${c.id}`}
                      />
                      {eob && (
                        <p className="mt-2 text-xs text-slate-600">
                          EOB ref: <span className="font-mono break-all">{eob.fhirExplanationOfBenefitReference}</span>
                          {eob.patientResponsibilityAmount != null && (
                            <> · patient responsibility: {eob.patientResponsibilityAmount}</>
                          )}
                        </p>
                      )}
                    </li>
                  )
                })}
              </ul>
            )}
          </div>
        </div>
      )}
    </section>
  )
}
