import { createContext, type Dispatch, type SetStateAction } from 'react'

export type SessionValue = {
  roles: readonly string[]
  setRoles: Dispatch<SetStateAction<string[]>>
}

export const SessionContext = createContext<SessionValue | null>(null)
