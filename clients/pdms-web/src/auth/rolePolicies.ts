/**
 * UI roles for RoleGate; align dev env with API scopes via dialysisScopeMap.
 * PHI preview: `Dialysis.Interoperability.Read` → `clinical.read`, `...Write` → `clinical.write`.
 */
export const clinicalPhiRoles = ['clinical.read', 'clinical.write', 'admin'] as const

/** Read-model projection only; `Dialysis.Sessions.Read` maps to `sessions.read` and is not sufficient here. */
export const sessionOverviewRoles = ['readmodel.read', 'admin'] as const

/** `Dialysis.Financial.Write` maps to `financial.write` only; timeline UI expects read scope or admin. */
export const financialTimelineRoles = ['financial.read', 'admin'] as const

/** Clinical SignalR hub (`ClinicalFeedHub`) requires `Dialysis.Delivery.Read` (or admin). */
export const realtimeFeedRoles = ['delivery.read', 'admin'] as const
