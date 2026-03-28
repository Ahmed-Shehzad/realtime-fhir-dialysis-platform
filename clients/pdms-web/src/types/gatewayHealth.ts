/** Gateway anonymous GET /health (JSON camelCase from ASP.NET). */
export type GatewayHealthResponse = {
  status: string
  service: string
  /** ASP.NET Core IHostEnvironment.EnvironmentName (e.g. Development, Production). */
  environment?: string
}
