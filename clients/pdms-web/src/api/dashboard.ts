import { runtimeConfig } from '../config/runtimeConfig'
import { type DashboardSummaryReadDto } from '../types/dashboardSummary'
import { httpClient } from './httpClient'

export async function fetchDashboardSummary(): Promise<DashboardSummaryReadDto> {
  const v = runtimeConfig.apiVersion
  const { data } = await httpClient.get<DashboardSummaryReadDto>(`/api/v${v}/dashboard/summary`)
  return data
}
