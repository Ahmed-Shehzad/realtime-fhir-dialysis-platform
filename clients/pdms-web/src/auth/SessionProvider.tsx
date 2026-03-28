import { useMemo, useState, type ReactElement, type ReactNode } from 'react'
import { mapDialysisScopesToUiRoles } from './dialysisScopeMap'
import { SessionContext } from './sessionContext'

function parseDevRolesFromEnv(): string[] {
  const raw = import.meta.env.VITE_APP_ROLES
  if (!raw) {
    return []
  }
  return raw
    .split(',')
    .map((role) => role.trim())
    .filter((role) => role.length > 0)
}

function initialDevRoles(): string[] {
  const fromAliases = parseDevRolesFromEnv()
  const fromScopes =
    import.meta.env.DEV && import.meta.env.VITE_DIALYSIS_SCOPES
      ? mapDialysisScopesToUiRoles(import.meta.env.VITE_DIALYSIS_SCOPES)
      : []
  return [...new Set([...fromAliases, ...fromScopes])]
}

export function SessionProvider({ children }: { children: ReactNode }): ReactElement {
  const [roles, setRoles] = useState<string[]>(() =>
    import.meta.env.DEV ? initialDevRoles() : [],
  )

  const value = useMemo(
    () => ({
      roles,
      setRoles,
    }),
    [roles],
  )

  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>
}
