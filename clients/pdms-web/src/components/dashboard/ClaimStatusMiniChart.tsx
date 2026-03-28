import * as d3 from 'd3'
import { useEffect, useRef, type ReactElement } from 'react'

const stages = ['Submitted', 'Adjudicated', 'EOB'] as const

function statusIndex(status: string): number {
  const s = status.trim()
  if (s.toLowerCase() === 'submitted') return 0
  if (s.toLowerCase() === 'adjudicated') return 1
  return 0
}

function eobAttached(hasEob: boolean): boolean {
  return hasEob
}

type ClaimStatusMiniChartProps = {
  claimStatus: string
  hasEob: boolean
  label: string
}

const heightPx = 56

/**
 * Discrete horizontal strip: submitted → adjudicated → EOB (accessibility via aria on section wrapper).
 */
export function ClaimStatusMiniChart({ claimStatus, hasEob, label }: ClaimStatusMiniChartProps): ReactElement {
  const ref = useRef<SVGSVGElement>(null)

  useEffect(() => {
    const svgEl = ref.current
    if (!svgEl) return

    const idx = statusIndex(claimStatus)
    const eob = eobAttached(hasEob)
    const activeIdx = eob ? 2 : Math.min(idx, 1)

    const width = svgEl.clientWidth || 320
    const margin = { left: 8, right: 8 }
    const innerW = width - margin.left - margin.right
    const xScale = d3.scaleBand().domain([...stages]).range([0, innerW]).padding(0.35)

    const svg = d3.select(svgEl)
    svg.selectAll('*').remove()

    const g = svg
      .attr('width', width)
      .attr('height', heightPx)
      .attr('role', 'img')
      .attr('aria-label', `${label}: ${stages.map((s, i) => `${s}${i <= activeIdx ? ' complete' : ''}`).join(', ')}`)
      .append('g')
      .attr('transform', `translate(${margin.left},${heightPx / 2 - 6})`)

    for (let i = 0; i < stages.length; i++) {
      const filled = i <= activeIdx
      g.append('circle')
        .attr('cx', (xScale(stages[i]) ?? 0) + (xScale.bandwidth() / 2))
        .attr('cy', 0)
        .attr('r', 8)
        .attr('fill', filled ? 'rgb(37, 99, 235)' : 'rgb(226, 232, 240)')
        .attr('stroke', 'rgb(148, 163, 184)')
        .attr('stroke-width', 1)
    }

    g.selectAll('text.label')
      .data([...stages])
      .enter()
      .append('text')
      .attr('class', 'label')
      .attr('fill', 'rgb(71, 85, 105)')
      .attr('font-size', '10px')
      .attr('x', (d) => (xScale(d) ?? 0) + xScale.bandwidth() / 2)
      .attr('y', 22)
      .attr('text-anchor', 'middle')
      .text((d) => d)
  }, [claimStatus, hasEob, label])

  return <svg ref={ref} className="w-full block" />
}
