import axios from 'axios'
import { runtimeConfig } from '../config/runtimeConfig'

/**
 * Axios instance for backend calls. Tokens are kept in memory by default
 * (avoid long-lived PHI or secrets in localStorage). Production auth should
 * prefer short-lived access tokens from a secure token endpoint or httpOnly cookies
 * coordinated with your API gateway.
 */
export const httpClient = axios.create({
  baseURL: runtimeConfig.apiBaseUrl || undefined,
  timeout: 30_000,
  headers: { Accept: 'application/json' },
  withCredentials: false,
  validateStatus: (status) => status >= 200 && status < 300,
})

let accessTokenInMemory: string | null = null

export function setHttpAccessToken(token: string | null): void {
  accessTokenInMemory = token
}

export function getHttpAccessToken(): string | null {
  return accessTokenInMemory
}

httpClient.interceptors.request.use((config) => {
  if (accessTokenInMemory) {
    config.headers.setAuthorization(`Bearer ${accessTokenInMemory}`)
  }
  if (runtimeConfig.tenantId) {
    config.headers.set('X-Tenant-Id', runtimeConfig.tenantId)
  }
  return config
})
