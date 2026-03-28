import { Component, type ErrorInfo, type ReactNode } from 'react'

type DashboardErrorBoundaryProps = {
  children: ReactNode
}

type DashboardErrorBoundaryState = {
  error: Error | null
}

export class DashboardErrorBoundary extends Component<
  DashboardErrorBoundaryProps,
  DashboardErrorBoundaryState
> {
  constructor(props: DashboardErrorBoundaryProps) {
    super(props)
    this.state = { error: null }
  }

  static getDerivedStateFromError(error: Error): DashboardErrorBoundaryState {
    return { error }
  }

  override componentDidCatch(error: Error, info: ErrorInfo): void {
    const message = error.message
    if (import.meta.env.DEV && typeof console !== 'undefined' && console.error) {
      console.error('Dashboard render error', message, info.componentStack)
    }
  }

  override render(): ReactNode {
    if (this.state.error) {
      return (
        <div className="rounded-lg border border-red-200 bg-red-50 p-6 text-red-900" role="alert">
          <p className="font-medium">Something went wrong rendering this dashboard.</p>
          <p className="mt-2 text-sm">Refresh the page or contact support. Avoid copying patient details into logs.</p>
        </div>
      )
    }
    return this.props.children
  }
}
