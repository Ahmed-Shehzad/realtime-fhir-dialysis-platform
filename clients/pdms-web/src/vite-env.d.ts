/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** HTTPS API origin for production builds; optional in development. */
  readonly VITE_API_BASE_URL?: string
  /** Comma-separated role hints for local UI gates only; never rely on this for authorization. */
  readonly VITE_APP_ROLES?: string
  /** Sent as X-Tenant-Id on API requests when set (C5 multi-tenancy). */
  readonly VITE_APP_TENANT_ID?: string
  /** API version segment for paths, default 1. */
  readonly VITE_API_VERSION?: string
  /**
   * When true, production builds treat the API as same-origin (empty base URL). Use with reverse proxy (e.g. Docker nginx → gateway).
   */
  readonly VITE_SAME_ORIGIN_API?: string
  /**
   * Development only: comma-separated backend scopes (e.g. Dialysis.ReadModel.Read)
   * mapped to UI roles via auth/dialysisScopeMap.
   */
  readonly VITE_DIALYSIS_SCOPES?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
