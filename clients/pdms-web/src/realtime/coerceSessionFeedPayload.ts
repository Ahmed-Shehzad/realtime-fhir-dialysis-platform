import type { SessionFeedPayload } from '../types/clinicalFeed'

type Loose = Record<string, unknown>

function pickString(...candidates: unknown[]): string {
  for (const c of candidates) {
    if (typeof c === 'string') return c
  }
  return ''
}

function pickOccurredAtUtc(...candidates: unknown[]): string {
  for (const c of candidates) {
    if (typeof c === 'string' && c.trim().length > 0) return c.trim()
    if (c instanceof Date && !Number.isNaN(c.getTime())) return c.toISOString()
    if (typeof c === 'number' && Number.isFinite(c)) return new Date(c).toISOString()
  }
  return ''
}

/** Normalizes SignalR hub payloads (camelCase or PascalCase) for ClinicalFeed consumers. */
export function coerceSessionFeedPayload(raw: unknown): SessionFeedPayload | null {
  if (raw === null || typeof raw !== 'object') return null
  const o = raw as Loose
  const treatmentSessionId = pickString(o.treatmentSessionId, o.TreatmentSessionId).trim()
  const eventType = pickString(o.eventType, o.EventType).trim()
  const summary = pickString(o.summary, o.Summary).trim()
  const occurredAtUtc = pickOccurredAtUtc(o.occurredAtUtc, o.OccurredAtUtc)
  if (!treatmentSessionId || !eventType || !summary || !occurredAtUtc) {
    return null
  }

  const vitalsRaw = o.vitalsByChannel ?? o.VitalsByChannel
  let vitalsByChannel: Record<string, number> | undefined
  if (vitalsRaw !== null && typeof vitalsRaw === 'object' && !Array.isArray(vitalsRaw)) {
    const entries: [string, number][] = []
    for (const [k, v] of Object.entries(vitalsRaw as Record<string, unknown>)) {
      if (typeof v === 'number' && Number.isFinite(v)) entries.push([k, v])
    }
    if (entries.length > 0) vitalsByChannel = Object.fromEntries(entries)
  }

  const patientDisplayLabel = pickString(o.patientDisplayLabel, o.PatientDisplayLabel).trim()
  const sessionStateHint = pickString(o.sessionStateHint, o.SessionStateHint).trim()
  const linkedDeviceIdHint = pickString(o.linkedDeviceIdHint, o.LinkedDeviceIdHint).trim()

  return {
    eventType,
    treatmentSessionId,
    summary,
    occurredAtUtc,
    ...(vitalsByChannel ? { vitalsByChannel } : {}),
    ...(patientDisplayLabel.length > 0 ? { patientDisplayLabel } : {}),
    ...(sessionStateHint.length > 0 ? { sessionStateHint } : {}),
    ...(linkedDeviceIdHint.length > 0 ? { linkedDeviceIdHint } : {}),
  }
}
