import * as d3 from 'd3'
import { useEffect, useMemo, useRef, type ReactElement } from 'react'

function buildVitalsChartAriaLabel(
  series: readonly VitalPoint[],
  label: string,
  valueLabel: string,
): string {
  if (series.length === 0) {
    return `${label}: no data`
  }
  const start = d3.min(series, (d) => d.t) ?? series[0]!.t
  const end = d3.max(series, (d) => d.t) ?? series[series.length - 1]!.t
  const valueMin = d3.min(series, (d) => d.value) ?? 0
  const valueMax = d3.max(series, (d) => d.value) ?? 0
  const timeFormat = new Intl.DateTimeFormat(undefined, { dateStyle: 'short', timeStyle: 'short' })
  return (
    `${label}. Demo series until Observation FHIR feed is wired. ` +
    `${series.length} samples from ${timeFormat.format(start)} to ${timeFormat.format(end)}. ` +
    `${valueLabel} from ${valueMin.toFixed(1)} to ${valueMax.toFixed(1)}.`
  )
}

export type VitalPoint = {
  t: Date
  value: number
}

type VitalsLineChartProps = {
  series: readonly VitalPoint[]
  label: string
  valueLabel?: string
}

const chartHeightPx = 220

export function VitalsLineChart({ series, label, valueLabel = 'value' }: VitalsLineChartProps): ReactElement {
  const containerRef = useRef<HTMLDivElement>(null)
  const svgRef = useRef<SVGSVGElement>(null)
  const ariaLabel = useMemo(() => buildVitalsChartAriaLabel(series, label, valueLabel), [series, label, valueLabel])

  useEffect(() => {
    const container = containerRef.current
    const svgElement = svgRef.current
    if (!container || !svgElement) {
      return
    }
    if (series.length === 0) {
      return
    }

    const width = container.clientWidth
    const margin = { top: 16, right: 12, bottom: 36, left: 52 }
    const innerWidth = Math.max(0, width - margin.left - margin.right)
    const innerHeight = chartHeightPx - margin.top - margin.bottom

    const svg = d3.select(svgElement)
    svg.selectAll('*').remove()

    const timeExtent = d3.extent(series, (d) => d.t)
    const start = timeExtent[0] ?? series[0]?.t ?? new Date()
    const end = timeExtent[1] ?? series[series.length - 1]?.t ?? start

    const valueMin = d3.min(series, (d) => d.value) ?? 0
    const valueMax = d3.max(series, (d) => d.value) ?? 1

    const xScale = d3.scaleTime().domain([start, end]).range([0, innerWidth])
    const yScale = d3
      .scaleLinear()
      .domain([valueMin, valueMax])
      .nice()
      .range([innerHeight, 0])

    const line = d3
      .line<VitalPoint>()
      .x((d) => xScale(d.t))
      .y((d) => yScale(d.value))
      .curve(d3.curveMonotoneX)

    const root = svg
      .attr('width', width)
      .attr('height', chartHeightPx)
      .attr('role', 'img')
      .attr('aria-label', ariaLabel)
      .append('g')
      .attr('transform', `translate(${margin.left},${margin.top})`)

    const xAxis = d3.axisBottom(xScale).ticks(Math.min(8, series.length))
    const yAxis = d3.axisLeft(yScale).ticks(5)

    root
      .append('g')
      .attr('transform', `translate(0,${innerHeight})`)
      .call(xAxis)
      .attr('class', 'text-slate-500 text-xs')
      .selectAll('text')
      .attr('transform', 'rotate(-25)')
      .style('text-anchor', 'end')

    root.append('g').call(yAxis).attr('class', 'text-slate-500 text-xs')

    root
      .append('path')
      .datum([...series])
      .attr('fill', 'none')
      .attr('stroke', 'rgb(59, 130, 246)')
      .attr('stroke-width', 2)
      .attr('d', line)

    root
      .append('text')
      .attr('x', innerWidth / 2)
      .attr('y', -4)
      .attr('text-anchor', 'middle')
      .attr('class', 'fill-slate-700 text-sm font-medium')
      .text(`${label} (${valueLabel})`)
  }, [series, label, valueLabel, ariaLabel])

  if (series.length === 0) {
    return (
      <div
        className="flex h-[220px] items-center justify-center rounded-lg border border-slate-200 bg-slate-50 text-sm text-slate-500"
        role="status"
      >
        No series data
      </div>
    )
  }

  return (
    <div ref={containerRef} className="w-full">
      <svg ref={svgRef} className="block w-full" role="img" aria-label={ariaLabel} />
    </div>
  )
}
