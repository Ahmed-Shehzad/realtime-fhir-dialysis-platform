import { type ReactElement } from 'react'
import { useRealtimeHubConnectionState } from '../../realtime/RealtimeHubContext'

type RealtimeHubBannerProps = {
  show: boolean
}

/** Surfaces SignalR negotiate/connection state for ClinicalFeedHub (dev diagnostics + operators). */
export function RealtimeHubBanner({ show }: RealtimeHubBannerProps): ReactElement | null {
  const { connectionState } = useRealtimeHubConnectionState()

  if (!show) {
    return null
  }

  const base = 'rounded-lg border px-3 py-2 text-sm lg:col-span-12'
  switch (connectionState.kind) {
    case 'off':
      return (
        <div className={`${base} border-slate-200 bg-slate-100 text-slate-600`} role="status">
          Realtime hub: disconnected. Reopen the dashboard or restart the gateway when you are ready to receive live
          events again.
        </div>
      )
    case 'connecting':
      return (
        <div className={`${base} border-amber-200 bg-amber-50 text-amber-900`} role="status">
          Realtime hub: connecting to <code className="rounded bg-amber-100 px-1">/hubs/clinical-feed</code>…
        </div>
      )
    case 'reconnecting':
      return (
        <div className={`${base} border-amber-200 bg-amber-50 text-amber-900`} role="status">
          Realtime hub: reconnecting…
        </div>
      )
    case 'connected':
      return (
        <div className={`${base} border-emerald-200 bg-emerald-50 text-emerald-900`} role="status">
          Realtime hub: connected — tenant alerts and session feed events will appear in the panels below when the
          server broadcasts.
        </div>
      )
    case 'failed':
      return (
        <div className={`${base} border-red-200 bg-red-50 text-red-900`} role="alert">
          Realtime hub: could not connect ({connectionState.message}). Run the API gateway on{' '}
          <code className="rounded bg-red-100 px-1">localhost:5100</code> and RealtimeDelivery (YARP cluster). In
          dev, hubs are proxied from the Vite dev server — if REST works but this fails, restart{' '}
          <code className="rounded bg-red-100 px-1">npm run dev</code> after pulling updates. See{' '}
          <code className="rounded bg-red-100 px-1">clients/pdms-web/README.md</code> (Realtime).
        </div>
      )
    default:
      return null
  }
}
