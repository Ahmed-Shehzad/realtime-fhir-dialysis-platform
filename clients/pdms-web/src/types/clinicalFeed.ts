/** JSON shape from RealtimeDelivery SessionFeedPayload (camelCase). */
export type SessionFeedPayload = {
  eventType: string
  treatmentSessionId: string
  summary: string
  occurredAtUtc: string
  /** Optional numeric vitals for live trend (e.g. simulate-gateway stream). Keys: map, heart-rate, spo2. */
  vitalsByChannel?: Record<string, number>
  /** Merges into session overview cache for Patient context (preview). */
  patientDisplayLabel?: string | null
  sessionStateHint?: string | null
  linkedDeviceIdHint?: string | null
}

/** JSON shape from RealtimeDelivery AlertFeedPayload (camelCase). */
export type AlertFeedPayload = {
  eventType: string
  treatmentSessionId: string | null
  alertId: string
  severity: string
  lifecycleState: string
  occurredAtUtc: string
}
