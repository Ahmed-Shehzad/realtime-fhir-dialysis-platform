function normalizeApiBaseUrl(raw: string): string {
  return raw.trim().replace(/\/$/, '')
}

function assertProductionHttps(url: string): void {
  if (!import.meta.env.PROD) {
    return
  }
  let parsed: URL
  try {
    parsed = new URL(url)
  } catch {
    throw new Error('VITE_API_BASE_URL must be a valid absolute URL in production builds.')
  }
  if (parsed.protocol !== 'https:') {
    throw new Error('VITE_API_BASE_URL must use https in production builds.')
  }
}

const apiBaseFromEnv = import.meta.env.VITE_API_BASE_URL
  ? normalizeApiBaseUrl(import.meta.env.VITE_API_BASE_URL)
  : ''

/** Absolute gateway URL from env, or empty (development: same-origin via Vite proxy; production: unset). */
const configuredBase = apiBaseFromEnv.length > 0 ? apiBaseFromEnv : ''

if (apiBaseFromEnv.length > 0) {
  assertProductionHttps(apiBaseFromEnv)
}

const tenantRaw = import.meta.env.VITE_APP_TENANT_ID?.trim() ?? ''
const versionRaw = import.meta.env.VITE_API_VERSION?.trim() ?? '1'

const sameOriginApiFlag = import.meta.env.VITE_SAME_ORIGIN_API
const sameOriginApi =
  sameOriginApiFlag === 'true' || sameOriginApiFlag === '1'

/** False only for production builds with no gateway URL and no same-origin mode. */
export const isGatewayClientConfigured: boolean =
  configuredBase.length > 0 || import.meta.env.DEV || (import.meta.env.PROD && sameOriginApi)

/** React Query cache segment: direct gateway URL vs same-origin (dev Vite proxy or production nginx). */
export const gatewayRequestOriginKey: string =
  configuredBase.length > 0
    ? configuredBase
    : import.meta.env.DEV || sameOriginApi
      ? 'same-origin'
      : ''

/** Vite mode: `development` when running `vite` / `npm run dev`; `production` for `vite build` output. */
export const viteMode: string = import.meta.env.MODE

/** True when serving the app from the Vite dev server (`npm run dev`). */
export const isViteDevelopment: boolean = import.meta.env.DEV

export const runtimeConfig = {
  apiBaseUrl: configuredBase,
  tenantId: tenantRaw.length > 0 ? tenantRaw : null,
  apiVersion: versionRaw,
} as const
