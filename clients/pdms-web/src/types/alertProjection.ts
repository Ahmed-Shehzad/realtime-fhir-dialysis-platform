/** Mirrors QueryReadModel AlertProjectionReadDto (JSON camelCase). */
export type AlertProjectionReadDto = {
  id: string
  alertRowKey: string
  alertType: string
  severity: string
  alertState: string
  treatmentSessionId: string | null
  raisedAtUtc: string
  projectionUpdatedAtUtc: string
}
