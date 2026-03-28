import {
  createContext,
  useContext,
  useMemo,
  useState,
  type Dispatch,
  type ReactElement,
  type ReactNode,
  type SetStateAction,
} from 'react'

export type RealtimeHubConnectionState =
  | { kind: 'off' }
  | { kind: 'connecting' }
  | { kind: 'connected' }
  | { kind: 'reconnecting' }
  | { kind: 'failed'; message: string }

type RealtimeHubContextValue = {
  connectionState: RealtimeHubConnectionState
  setConnectionState: Dispatch<SetStateAction<RealtimeHubConnectionState>>
}

const RealtimeHubContext = createContext<RealtimeHubContextValue | null>(null)

export function RealtimeHubProvider({ children }: { children: ReactNode }): ReactElement {
  const [connectionState, setConnectionState] = useState<RealtimeHubConnectionState>({ kind: 'connecting' })
  const value = useMemo(
    () => ({ connectionState, setConnectionState }),
    [connectionState],
  )
  return <RealtimeHubContext.Provider value={value}>{children}</RealtimeHubContext.Provider>
}

export function useRealtimeHubConnectionState(): RealtimeHubContextValue {
  const ctx = useContext(RealtimeHubContext)
  if (!ctx) {
    throw new Error('useRealtimeHubConnectionState must be used within RealtimeHubProvider')
  }
  return ctx
}
