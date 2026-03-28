import { useContext } from 'react'
import { SessionContext, type SessionValue } from './sessionContext'

export function useSession(): SessionValue {
  const ctx = useContext(SessionContext)
  if (!ctx) {
    throw new Error('useSession must be used within SessionProvider')
  }
  return ctx
}
