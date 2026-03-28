import { isViteDevelopment, viteMode } from '../config/runtimeConfig'
import { type GatewayHealthResponse } from '../types/gatewayHealth'

export type DevStackAlignment = {
  ok: boolean
  summary: string
  detail?: string
}

/**
 * Compares Vite client mode to gateway /health `environment` (IHostEnvironment.EnvironmentName).
 * Use to confirm local `npm run dev` is paired with ASP.NET Development.
 */
export function getDevStackAlignment(health: GatewayHealthResponse | undefined): DevStackAlignment {
  const frontendLabel = isViteDevelopment ? `Vite development (${viteMode})` : `Vite ${viteMode}`

  if (!health) {
    return { ok: false, summary: 'Gateway health has not loaded yet.' }
  }

  const backendEnv = health.environment?.trim()
  if (!backendEnv) {
    return {
      ok: true,
      summary: `${frontendLabel}; gateway did not report environment (rebuild RealtimePlatform.ApiGateway).`,
    }
  }

  if (isViteDevelopment && backendEnv === 'Development') {
    return {
      ok: true,
      summary: `${frontendLabel} and API gateway are both Development — OK for local work.`,
    }
  }

  if (isViteDevelopment) {
    return {
      ok: false,
      summary: `${frontendLabel}, but the gateway reports environment "${backendEnv}".`,
      detail:
        'For typical local debugging, run the gateway with ASPNETCORE_ENVIRONMENT=Development (e.g. Rider run configuration).',
    }
  }

  if (backendEnv === 'Development') {
    return {
      ok: false,
      summary: `Production-built UI (${viteMode}) is using a Development gateway.`,
      detail: 'Use a staging or production gateway origin for real deployments.',
    }
  }

  return {
    ok: true,
    summary: `${frontendLabel}; gateway environment is "${backendEnv}".`,
  }
}
