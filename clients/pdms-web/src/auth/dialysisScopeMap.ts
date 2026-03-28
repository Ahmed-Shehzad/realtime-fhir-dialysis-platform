/**
 * Maps Entra/API scopes (BuildingBlocks AuthorizationScopesOptions) to UI role strings
 * used by RoleGate. Server-side policies remain authoritative.
 */
export const dialysisScopeToUiRole: Readonly<Record<string, string>> = {
  'Dialysis.ReadModel.Read': 'readmodel.read',
  'Dialysis.ReadModel.Write': 'readmodel.write',
  'Dialysis.Sessions.Read': 'sessions.read',
  'Dialysis.Sessions.Write': 'sessions.write',
  'Dialysis.Financial.Read': 'financial.read',
  'Dialysis.Financial.Write': 'financial.write',
  'Dialysis.Interoperability.Read': 'clinical.read',
  'Dialysis.Interoperability.Write': 'clinical.write',
  'Dialysis.Surveillance.Read': 'surveillance.read',
  'Dialysis.Surveillance.Write': 'surveillance.write',
  'Dialysis.Delivery.Read': 'delivery.read',
  'Dialysis.Delivery.Write': 'delivery.write',
}

export function mapDialysisScopesToUiRoles(commaSeparated: string | undefined): string[] {
  if (!commaSeparated) {
    return []
  }
  const out = new Set<string>()
  for (const part of commaSeparated.split(',')) {
    const scope = part.trim()
    if (!scope) continue
    const role = dialysisScopeToUiRole[scope]
    if (role) {
      out.add(role)
    }
  }
  return [...out]
}
