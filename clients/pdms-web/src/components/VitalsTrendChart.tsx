import * as d3 from 'd3'
import { useCallback, useEffect, useMemo, useRef, useState, type ReactElement } from 'react'

import type { VitalPoint } from './VitalsLineChart'

/** One physiological series (Bokeh-style multi-line plot). */
export type VitalSeriesDefinition = {
  id: string
  label: string
  unit: string
  color: string
  points: readonly VitalPoint[]
}

const defaultPalette = [
  '#1f77b4',
  '#ff7f0e',
  '#2ca02c',
  '#d62728',
  '#9467bd',
  '#8c564b',
] as const

export function vitalSeriesWithDefaultColors(
  definitions: readonly Omit<VitalSeriesDefinition, 'color'>[],
): VitalSeriesDefinition[] {
  return definitions.map((d, i) => ({
    ...d,
    color: defaultPalette[i % defaultPalette.length] ?? '#1f77b4',
  }))
}

type VitalsTrendChartProps = {
  series: readonly VitalSeriesDefinition[]
  title?: string
  subtitle?: string
  /** When true and the latest sample time advances, reset pan/zoom so streaming points stay visible. */
  resetZoomWhenTimeExtentGrows?: boolean
}

const plotHeightPx = 360

function visibleValueExtent(
  seriesList: readonly VitalSeriesDefinition[],
  timeStart: Date,
  timeEnd: Date,
): [number, number] {
  let lo = Number.POSITIVE_INFINITY
  let hi = Number.NEGATIVE_INFINITY
  for (const s of seriesList) {
    for (const p of s.points) {
      if (p.t >= timeStart && p.t <= timeEnd) {
        lo = Math.min(lo, p.value)
        hi = Math.max(hi, p.value)
      }
    }
  }
  if (!Number.isFinite(lo) || !Number.isFinite(hi)) return [0, 1]
  if (lo === hi) return [lo - 1, hi + 1]
  const pad = (hi - lo) * 0.08
  return [lo - pad, hi + pad]
}

function interpolateNearest(points: readonly VitalPoint[], t: Date): VitalPoint | undefined {
  if (points.length === 0) return undefined
  const tx = t.getTime()
  const sorted = [...points].sort((a, b) => a.t.getTime() - b.t.getTime())
  const times = sorted.map((p) => p.t.getTime())
  const i = d3.bisectLeft(times, tx)
  const p0 = sorted[Math.max(0, i - 1)]!
  const p1 = sorted[Math.min(sorted.length - 1, i)]!
  if (p0 === p1) return p0
  return tx - p0.t.getTime() < p1.t.getTime() - tx ? p0 : p1
}

function buildAriaSummary(series: readonly VitalSeriesDefinition[]): string {
  if (series.length === 0) return 'Vitals trend: no series'
  const timeFormat = new Intl.DateTimeFormat(undefined, { dateStyle: 'short', timeStyle: 'short' })
  const parts = series.map((s) => {
    if (s.points.length === 0) return `${s.label}: no points`
    const t0 = s.points[0]!.t
    const t1 = s.points[s.points.length - 1]!.t
    const vMin = d3.min(s.points, (d) => d.value) ?? 0
    const vMax = d3.max(s.points, (d) => d.value) ?? 0
    return `${s.label} (${s.unit}): ${s.points.length} samples, ${timeFormat.format(t0)}–${timeFormat.format(t1)}, ${vMin.toFixed(1)}–${vMax.toFixed(1)}`
  })
  return `Vitals trend. ${parts.join('. ')}.`
}

export function VitalsTrendChart({
  series,
  title,
  subtitle,
  resetZoomWhenTimeExtentGrows = false,
}: VitalsTrendChartProps): ReactElement {
  const containerRef = useRef<HTMLDivElement>(null)
  const svgRef = useRef<SVGSVGElement>(null)
  const transformRef = useRef(d3.zoomIdentity)
  const lastTimeExtentMaxRef = useRef<number | null>(null)
  const rafRef = useRef<number | undefined>(undefined)
  const [hiddenIds, setHiddenIds] = useState<Set<string>>(() => new Set())

  useEffect(() => {
    if (!resetZoomWhenTimeExtentGrows) {
      lastTimeExtentMaxRef.current = null
    }
  }, [resetZoomWhenTimeExtentGrows])

  const visibleSeries = useMemo(
    () => series.filter((s) => s.points.length > 0 && !hiddenIds.has(s.id)),
    [series, hiddenIds],
  )

  const ariaLabel = useMemo(() => buildAriaSummary(series), [series])

  const toggleSeries = useCallback((id: string) => {
    setHiddenIds((prev) => {
      const next = new Set(prev)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }, [])

  useEffect(() => {
    const container = containerRef.current
    const svgElement = svgRef.current
    if (!container || !svgElement) return

    let applyingProgrammaticZoom = false

    const scheduleDraw = () => {
      if (rafRef.current !== undefined) cancelAnimationFrame(rafRef.current)
      rafRef.current = requestAnimationFrame(() => drawChart())
    }

    const drawChart = () => {
      d3.select(container).selectAll('.vitals-hover-tooltip').remove()

      if (visibleSeries.length > 0 && resetZoomWhenTimeExtentGrows) {
        const times = visibleSeries.flatMap((s) => s.points.map((p) => p.t.getTime()))
        if (times.length > 0) {
          const maxT = Math.max(...times)
          const prevMax = lastTimeExtentMaxRef.current
          if (prevMax !== null && maxT > prevMax) {
            transformRef.current = d3.zoomIdentity
          }
          lastTimeExtentMaxRef.current = maxT
        }
      }

      const width = container.clientWidth || 640
      const distinctUnits = new Set(visibleSeries.map((s) => s.unit))
      const hasSecondAxis = distinctUnits.size === 2
      const margin = { top: title ? 36 : 28, right: hasSecondAxis ? 58 : 16, bottom: 48, left: 60 }
      const innerWidth = Math.max(40, width - margin.left - margin.right)
      const innerHeight = plotHeightPx - margin.top - margin.bottom

      const svg = d3.select(svgElement)
      svg.selectAll('*').remove()

      if (visibleSeries.length === 0) {
        svg
          .attr('width', width)
          .attr('height', plotHeightPx)
          .attr('role', 'img')
          .attr('aria-label', ariaLabel)
        svg
          .append('text')
          .attr('x', width / 2)
          .attr('y', plotHeightPx / 2)
          .attr('text-anchor', 'middle')
          .attr('class', 'fill-slate-400 text-sm')
          .text('No visible series — use the legend to show at least one trace.')
        return
      }

      const allTimes = visibleSeries.flatMap((s) => s.points.map((p) => p.t.getTime()))
      const tMin = Math.min(...allTimes)
      const tMax = Math.max(...allTimes)
      const start = new Date(tMin)
      const end = new Date(tMax)

      const xScaleBase = d3.scaleTime().domain([start, end]).range([0, innerWidth])
      const xScale = transformRef.current.rescaleX(xScaleBase)
      const domainRaw = xScale.domain()
      const vxStart = domainRaw[0] instanceof Date ? domainRaw[0] : new Date(domainRaw[0])
      const vxEnd = domainRaw[1] instanceof Date ? domainRaw[1] : new Date(domainRaw[1])

      const unitGroups = d3.group(visibleSeries, (s) => s.unit)
      const unitKeys = [...unitGroups.keys()]

      /** Dual axis only for exactly two unit groups; otherwise one scale (avoids mis-scaling a third trace). */
      const useDualAxis = unitKeys.length === 2
      const leftUnit = useDualAxis ? (unitKeys[0] ?? '') : unitKeys.join(' · ')
      const rightUnit = useDualAxis ? unitKeys[1]! : null
      const leftList = useDualAxis ? (unitGroups.get(unitKeys[0]!) ?? []) : visibleSeries
      const rightList = useDualAxis && rightUnit ? (unitGroups.get(rightUnit) ?? []) : []

      const [y0Min, y0Max] = visibleValueExtent(leftList, vxStart, vxEnd)
      const yScaleLeft = d3.scaleLinear().domain([y0Min, y0Max]).nice().range([innerHeight, 0])

      let yScaleRight: d3.ScaleLinear<number, number> | null = null
      if (useDualAxis && rightList.length > 0 && rightUnit) {
        const [y1Min, y1Max] = visibleValueExtent(rightList, vxStart, vxEnd)
        yScaleRight = d3.scaleLinear().domain([y1Min, y1Max]).nice().range([innerHeight, 0])
      }

      svg.attr('width', width).attr('height', plotHeightPx).attr('role', 'img').attr('aria-label', ariaLabel)

      if (title) {
        svg
          .append('text')
          .attr('x', margin.left)
          .attr('y', 18)
          .attr('class', 'fill-slate-800 text-sm font-semibold')
          .text(title)
      }

      const root = svg.append('g').attr('transform', `translate(${margin.left},${margin.top})`)

      const clipId = `vitals-clip-${Math.random().toString(36).slice(2)}`
      root
        .append('defs')
        .append('clipPath')
        .attr('id', clipId)
        .append('rect')
        .attr('width', innerWidth)
        .attr('height', innerHeight)

      const gridG = root.append('g').attr('class', 'grid').attr('opacity', 0.55)

      const xTicks = xScale.ticks(12)
      gridG
        .selectAll('line.x')
        .data(xTicks)
        .join('line')
        .attr('class', 'x')
        .attr('stroke', '#e2e8f0')
        .attr('stroke-dasharray', '2,4')
        .attr('x1', (d) => xScale(d))
        .attr('x2', (d) => xScale(d))
        .attr('y1', 0)
        .attr('y2', innerHeight)

      const yTicksLeft = yScaleLeft.ticks(8)
      gridG
        .selectAll('line.y')
        .data(yTicksLeft)
        .join('line')
        .attr('class', 'y')
        .attr('stroke', '#e2e8f0')
        .attr('stroke-dasharray', '2,4')
        .attr('x1', 0)
        .attr('x2', innerWidth)
        .attr('y1', (d) => yScaleLeft(d))
        .attr('y2', (d) => yScaleLeft(d))

      const plotG = root.append('g').attr('clip-path', `url(#${clipId})`)

      const yForSeries = (s: VitalSeriesDefinition) => {
        if (!useDualAxis) return yScaleLeft
        if (s.unit === unitKeys[0]) return yScaleLeft
        if (yScaleRight && s.unit === rightUnit) return yScaleRight
        return yScaleLeft
      }

      for (const s of visibleSeries) {
        const ySc = yForSeries(s)
        const line = d3
          .line<VitalPoint>()
          .defined((d) => d.t >= vxStart && d.t <= vxEnd)
          .x((d) => xScale(d.t))
          .y((d) => ySc(d.value))
          .curve(d3.curveMonotoneX)

        plotG
          .append('path')
          .datum([...s.points])
          .attr('fill', 'none')
          .attr('stroke', s.color)
          .attr('stroke-width', 2)
          .attr('stroke-linejoin', 'round')
          .attr('stroke-linecap', 'round')
          .attr('d', line)
      }

      const xAxis = d3.axisBottom(xScale).ticks(Math.min(12, Math.max(4, Math.ceil(allTimes.length / 5))))
      const xAxisG = root.append('g').attr('transform', `translate(0,${innerHeight})`).call(xAxis)
      xAxisG.selectAll('text').attr('class', 'text-slate-600').style('font-size', '10px')
      xAxisG.selectAll('path,line').attr('stroke', '#94a3b8')

      root
        .append('g')
        .call(d3.axisLeft(yScaleLeft))
        .call((g) => g.selectAll('text').attr('class', 'text-slate-600').style('font-size', '10px'))
        .call((g) => g.selectAll('path,line').attr('stroke', '#94a3b8'))
        .append('text')
        .attr('transform', 'rotate(-90)')
        .attr('x', -innerHeight / 2)
        .attr('y', -46)
        .attr('text-anchor', 'middle')
        .attr('class', 'fill-slate-600 text-xs font-medium')
        .text(leftUnit)

      if (useDualAxis && yScaleRight && rightUnit) {
        root
          .append('g')
          .attr('transform', `translate(${innerWidth},0)`)
          .call(d3.axisRight(yScaleRight))
          .call((g) => g.selectAll('text').attr('class', 'text-slate-600').style('font-size', '10px'))
          .call((g) => g.selectAll('path,line').attr('stroke', '#94a3b8'))
          .append('text')
          .attr('transform', 'rotate(90)')
          .attr('x', innerHeight / 2)
          .attr('y', 52)
          .attr('text-anchor', 'middle')
          .attr('class', 'fill-slate-600 text-xs font-medium')
          .text(rightUnit)
      }

      const tooltip = d3
        .select(container)
        .append('div')
        .attr(
          'class',
          'vitals-hover-tooltip fixed z-50 hidden max-w-sm rounded-md border border-slate-200 bg-white/95 px-3 py-2 text-xs shadow-md backdrop-blur-sm pointer-events-none',
        )

      const timeFmt = new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'medium' })
      const focusG = root.append('g').style('display', 'none')
      focusG.append('line').attr('stroke', '#64748b').attr('stroke-dasharray', '4,3').attr('stroke-opacity', 0.85)

      focusG
        .selectAll<SVGCircleElement, VitalSeriesDefinition>('circle.focus')
        .data(visibleSeries, (d) => d.id)
        .join('circle')
        .attr('class', 'focus')
        .attr('r', 4.5)
        .attr('fill', (d) => d.color)
        .attr('stroke', '#fff')
        .attr('stroke-width', 1.5)

      function setTooltip(clientX: number, clientY: number, html: string) {
        tooltip.classed('hidden', false).html(html)
        const pad = 12
        tooltip.style('left', `${clientX + pad}px`).style('top', `${clientY + pad}px`)
      }

      function hideTooltip() {
        tooltip.classed('hidden', true)
        focusG.style('display', 'none')
      }

      const overlay = root
        .append('rect')
        .attr('width', innerWidth)
        .attr('height', innerHeight)
        .attr('fill', 'transparent')
        .style('cursor', 'crosshair')

      overlay
        .on('mousemove', (event: MouseEvent) => {
          const [mx] = d3.pointer(event, overlay.node())
          const tHover = xScale.invert(mx)
          if (tHover < vxStart || tHover > vxEnd) {
            hideTooltip()
            return
          }

          focusG.style('display', null)
          const lineEl = focusG.select('line')
          lineEl.attr('x1', mx).attr('x2', mx).attr('y1', 0).attr('y2', innerHeight)

          const rows: string[] = [
            `<div class="font-semibold text-slate-800 border-b border-slate-100 pb-1 mb-1">${timeFmt.format(tHover)}</div>`,
          ]

          focusG.selectAll<SVGCircleElement, VitalSeriesDefinition>('circle.focus').each(function (d) {
            const p = interpolateNearest(d.points, tHover)
            const ySc = yForSeries(d)
            const circle = d3.select(this)
            if (!p) {
              circle.attr('opacity', 0)
              return
            }
            circle.attr('opacity', 1).attr('cx', xScale(p.t)).attr('cy', ySc(p.value))
            rows.push(
              `<div class="flex justify-between gap-4"><span style="color:${d.color}" class="font-medium">${d.label}</span><span class="tabular-nums text-slate-700">${p.value.toFixed(1)} <span class="text-slate-500">${d.unit}</span></span></div>`,
            )
          })

          setTooltip(event.clientX, event.clientY, rows.join(''))
        })
        .on('mouseleave', hideTooltip)

      const zoom = d3
        .zoom<SVGRectElement, unknown>()
        .scaleExtent([0.2, 64])
        .translateExtent([
          [-innerWidth, -innerHeight],
          [innerWidth * 2, innerHeight * 2],
        ])
        .extent([
          [0, 0],
          [innerWidth, innerHeight],
        ])
        .on('zoom', (event) => {
          if (applyingProgrammaticZoom) return
          transformRef.current = event.transform
          scheduleDraw()
        })

      overlay.call(zoom)

      applyingProgrammaticZoom = true
      overlay.call(zoom.transform, transformRef.current)
      applyingProgrammaticZoom = false

      overlay.on('dblclick.zoom', null)
      overlay.on('dblclick', (event: MouseEvent) => {
        event.preventDefault()
        transformRef.current = d3.zoomIdentity
        scheduleDraw()
      })
    }

    scheduleDraw()

    const ro = new ResizeObserver(() => scheduleDraw())
    ro.observe(container)

    return () => {
      ro.disconnect()
      d3.select(container).selectAll('.vitals-hover-tooltip').remove()
      if (rafRef.current !== undefined) cancelAnimationFrame(rafRef.current)
    }
  }, [visibleSeries, ariaLabel, title, resetZoomWhenTimeExtentGrows])

  return (
    <div ref={containerRef} className="w-full space-y-3">
      {subtitle ? <p className="text-xs text-slate-600">{subtitle}</p> : null}
      <div className="flex flex-wrap gap-3 border-b border-slate-100 pb-3" aria-label="Series legend">
        {series.map((s) => {
          const off = hiddenIds.has(s.id)
          return (
            <button
              key={s.id}
              type="button"
              onClick={() => toggleSeries(s.id)}
              className={`inline-flex items-center gap-2 rounded-md border px-2.5 py-1 text-xs font-medium transition ${
                off
                  ? 'border-slate-200 bg-slate-50 text-slate-400 line-through'
                  : 'border-slate-200 bg-white text-slate-800 shadow-sm hover:bg-slate-50'
              }`}
            >
              <span className="h-2 w-4 rounded-sm" style={{ backgroundColor: off ? '#cbd5e1' : s.color }} />
              {s.label}
              <span className="text-slate-500">({s.unit})</span>
            </button>
          )
        })}
      </div>
      <svg ref={svgRef} className="block w-full select-none touch-none" />
      <p className="text-[11px] leading-relaxed text-slate-500">
        Scroll or pinch to zoom (time), drag to pan. Double-click plot to reset. Hover for crosshair and tooltip. Legend
        toggles traces (Bokeh-style palette).
      </p>
    </div>
  )
}
