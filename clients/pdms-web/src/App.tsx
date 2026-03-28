import { type ReactElement } from 'react'
import { Route, Routes } from 'react-router'
import DashboardPage from './pages/DashboardPage'
import { ClinicalFeedBridge } from './realtime/ClinicalFeedBridge'
import { RealtimeHubProvider } from './realtime/RealtimeHubContext'

export default function App(): ReactElement {
  return (
    <RealtimeHubProvider>
      <ClinicalFeedBridge />
      <Routes>
        <Route path="/" element={<DashboardPage />} />
      </Routes>
    </RealtimeHubProvider>
  )
}
