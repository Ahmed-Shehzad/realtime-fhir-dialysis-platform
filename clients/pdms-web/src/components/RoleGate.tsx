import { type ReactNode } from 'react'
import { useSession } from '../auth/useSession'

type RoleGateProps = {
  anyOf: readonly string[]
  children: ReactNode
  fallback?: ReactNode
}

/**
 * Hides UI that may surface PHI when the signed-in principal is missing clinical roles.
 * All sensitive operations must still be denied by the API — this is a view-layer guard only.
 */
export function RoleGate({ anyOf, children, fallback = null }: RoleGateProps): ReactNode {
  const { roles } = useSession()
  const allowed = anyOf.some((role) => roles.includes(role))
  if (!allowed) {
    return fallback
  }
  return children
}
