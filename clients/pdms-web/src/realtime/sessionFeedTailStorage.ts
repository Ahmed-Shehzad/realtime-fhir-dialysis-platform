import { normalizeTreatmentSessionId } from '../lib/normalizeTreatmentSessionId'
import type { SessionFeedPayload } from '../types/clinicalFeed'
import { runtimeConfig } from '../config/runtimeConfig'

export const SESSION_FEED_TAIL_MAX_ITEMS = 20

const storageVersion = '1'

function tenantSegment(): string {
  return runtimeConfig.tenantId ?? 'default'
}

export function sessionFeedTailStorageKey(treatmentSessionId: string): string {
  const sid = normalizeTreatmentSessionId(treatmentSessionId)
  return `pdms.sessionFeedTail.v${storageVersion}:${tenantSegment()}:${sid}`
}

function isSessionFeedPayloadItem(value: unknown): value is SessionFeedPayload {
  if (value === null || typeof value !== 'object') return false
  const o = value as Record<string, unknown>
  return (
    typeof o.eventType === 'string' &&
    typeof o.treatmentSessionId === 'string' &&
    typeof o.summary === 'string' &&
    typeof o.occurredAtUtc === 'string'
  )
}

export function readSessionFeedTailFromStorage(treatmentSessionId: string): SessionFeedPayload[] {
  const sid = normalizeTreatmentSessionId(treatmentSessionId)
  if (sid.length === 0 || typeof sessionStorage === 'undefined') return []

  try {
    const raw = sessionStorage.getItem(sessionFeedTailStorageKey(sid))
    if (raw == null || raw.length === 0) return []

    const parsed: unknown = JSON.parse(raw)
    if (!Array.isArray(parsed)) return []

    const out: SessionFeedPayload[] = []
    for (const item of parsed) {
      if (isSessionFeedPayloadItem(item)) out.push(item)
    }
    return out
  } catch {
    return []
  }
}

export function writeSessionFeedTailToStorage(treatmentSessionId: string, tail: readonly SessionFeedPayload[]): void {
  const sid = normalizeTreatmentSessionId(treatmentSessionId)
  if (sid.length === 0 || typeof sessionStorage === 'undefined') return

  try {
    sessionStorage.setItem(sessionFeedTailStorageKey(sid), JSON.stringify([...tail]))
  } catch {
    // QuotaExceededError or private mode — ignore; in-memory cache still works until refresh.
  }
}
