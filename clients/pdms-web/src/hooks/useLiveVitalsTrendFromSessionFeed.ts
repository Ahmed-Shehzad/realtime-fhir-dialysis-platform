import { useMemo } from 'react'
import type { VitalSeriesDefinition } from '../components/VitalsTrendChart'
import { normalizeTreatmentSessionId } from '../lib/normalizeTreatmentSessionId'
import { vitalSeriesWithDefaultColors } from '../components/VitalsTrendChart'
import type { VitalPoint } from '../components/VitalsLineChart'
import type { SessionFeedPayload } from '../types/clinicalFeed'

const vitalsTrendEventType = 'Simulation.VitalsTrend'
const maxVitalsPoints = 120

function trimPoints(points: VitalPoint[]): VitalPoint[] {
  return points.length <= maxVitalsPoints ? points : points.slice(-maxVitalsPoints)
}

/** Builds vitals series from session feed items emitted by simulate-gateway (vitals stream + delivery broadcast). */
export function useLiveVitalsTrendFromSessionFeed(
  treatmentSessionId: string,
  feedTail: readonly SessionFeedPayload[] | undefined,
): VitalSeriesDefinition[] | null {
  const sid = normalizeTreatmentSessionId(treatmentSessionId)
  return useMemo(() => {
    if (!sid || !feedTail?.length) return null

    const mapPoints: VitalPoint[] = []
    const hrPoints: VitalPoint[] = []
    const spo2Points: VitalPoint[] = []

    for (const e of feedTail) {
      if (e.eventType !== vitalsTrendEventType || normalizeTreatmentSessionId(e.treatmentSessionId) !== sid) {
        continue
      }
      const v = e.vitalsByChannel
      if (!v) continue
      const t = new Date(e.occurredAtUtc)
      if (typeof v.map === 'number') mapPoints.push({ t, value: v.map })
      const hr = v['heart-rate']
      if (typeof hr === 'number') hrPoints.push({ t, value: hr })
      if (typeof v.spo2 === 'number') spo2Points.push({ t, value: v.spo2 })
    }

    if (mapPoints.length === 0 && hrPoints.length === 0 && spo2Points.length === 0) return null

    return vitalSeriesWithDefaultColors([
      { id: 'map', label: 'Mean arterial pressure', unit: 'mmHg', points: trimPoints(mapPoints) },
      { id: 'hr', label: 'Heart rate', unit: '/min', points: trimPoints(hrPoints) },
      { id: 'spo2', label: 'SpO₂ (estimated)', unit: '%', points: trimPoints(spo2Points) },
    ])
  }, [sid, feedTail])
}
