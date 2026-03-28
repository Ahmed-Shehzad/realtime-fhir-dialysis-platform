/** Mirrors QueryReadModel SessionOverviewReadDto (JSON camelCase). */
export type SessionOverviewReadDto = {
  id: string
  treatmentSessionId: string
  sessionState: string
  patientDisplayLabel: string | null
  linkedDeviceId: string | null
  sessionStartedAtUtc: string
  projectionUpdatedAtUtc: string
}
