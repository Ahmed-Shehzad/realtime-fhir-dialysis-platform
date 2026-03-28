import { runtimeConfig } from '../config/runtimeConfig'
import { type SessionOverviewReadDto } from '../types/sessionOverview'
import { httpClient } from './httpClient'

export async function fetchSessionOverview(treatmentSessionId: string): Promise<SessionOverviewReadDto> {
  const v = runtimeConfig.apiVersion
  const { data } = await httpClient.get<SessionOverviewReadDto>(
    `/api/v${v}/sessions/${encodeURIComponent(treatmentSessionId)}/overview`,
  )
  return data
}
