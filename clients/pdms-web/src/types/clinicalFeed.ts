/** JSON shape from RealtimeDelivery SessionFeedPayload (camelCase). */
export type SessionFeedPayload = {
  eventType: string
  treatmentSessionId: string
  summary: string
  occurredAtUtc: string
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
