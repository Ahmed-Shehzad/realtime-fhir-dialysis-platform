import { runtimeConfig } from '../config/runtimeConfig'
import { type FinancialSessionTimelineDto } from '../types/financialTimeline'
import { httpClient } from './httpClient'

export async function fetchFinancialTimeline(
  treatmentSessionId: string,
  patientId: string | null,
): Promise<FinancialSessionTimelineDto> {
  const v = runtimeConfig.apiVersion
  const params = new URLSearchParams()
  if (patientId) {
    params.set('patientId', patientId)
  }
  const qs = params.toString()
  const path = `/api/v${v}/financial/sessions/${encodeURIComponent(treatmentSessionId)}/timeline${qs ? `?${qs}` : ''}`
  const { data } = await httpClient.get<FinancialSessionTimelineDto>(path)
  return data
}
