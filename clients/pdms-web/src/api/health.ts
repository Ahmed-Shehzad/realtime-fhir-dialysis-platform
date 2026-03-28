import { httpClient } from './httpClient'
import { type GatewayHealthResponse } from '../types/gatewayHealth'

export async function fetchGatewayHealth(): Promise<GatewayHealthResponse> {
  const response = await httpClient.get<GatewayHealthResponse>('/health')
  return response.data
}
