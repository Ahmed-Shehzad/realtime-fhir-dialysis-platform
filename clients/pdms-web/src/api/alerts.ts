import { runtimeConfig } from '../config/runtimeConfig'
import { type AlertProjectionReadDto } from '../types/alertProjection'
import { httpClient } from './httpClient'

export async function fetchAlertProjections(severityFilter: string | null): Promise<AlertProjectionReadDto[]> {
  const v = runtimeConfig.apiVersion
  const params = new URLSearchParams()
  if (severityFilter?.trim()) {
    params.set('severity', severityFilter.trim())
  }
  const qs = params.toString()
  const path = `/api/v${v}/alerts${qs ? `?${qs}` : ''}`
  const { data } = await httpClient.get<AlertProjectionReadDto[]>(path)
  return data
}
