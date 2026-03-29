/** Canonical form for treatment session identifiers in URL, SignalR groups, and TanStack keys (Ulids are case-insensitive). */
export function normalizeTreatmentSessionId(raw: string | null | undefined): string {
  if (raw == null) return ''
  return raw.trim().toUpperCase()
}
