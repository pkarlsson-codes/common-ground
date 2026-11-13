import { useState } from 'react'
import type { IncomeLimitScheduleResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import {
  PUBLISHED_HOUSEHOLD_SIZES,
  useCorrectIncomeLimitSchedule,
  useEnterIncomeLimitSchedule,
} from '../../hooks/incomeLimits'
import { useToast } from '../../hooks/useToast'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { parseAmount, parsePastedLimits } from '../../lib/pastedLimits'

type LevelRow = { key: number; percentOfAmi: string; limits: string[] }

type Props = {
  /** The schedule to correct, or null to enter a new one. */
  schedule: IncomeLimitScheduleResponse | null
  /** The newest schedule, whose levels a new schedule starts with. */
  newest: IncomeLimitScheduleResponse | null
  onClose: () => void
}

let nextKey = 0
const SIZES = Array.from({ length: PUBLISHED_HOUSEHOLD_SIZES }, (_, i) => i + 1)
const emptyLimits = () => Array.from({ length: PUBLISHED_HOUSEHOLD_SIZES }, () => '')
const row = (percentOfAmi: string, limits = emptyLimits()): LevelRow => ({ key: nextKey++, percentOfAmi, limits })

const initialRows = (schedule: IncomeLimitScheduleResponse | null, newest: IncomeLimitScheduleResponse | null) => {
  if (schedule) {
    return (schedule.levels ?? []).map(l => row(String(l.percentOfAmi), (l.limits ?? []).map(String)))
  }
  const levels = newest?.levels ?? []
  return levels.length > 0 ? levels.map(l => row(String(l.percentOfAmi))) : [row('')]
}

const sizeLabel = (size: number) => `${size} ${size === 1 ? 'person' : 'people'}`

/** The first reason the rows can't be saved, or null. The API checks the same rules. */
const rowsProblem = (rows: LevelRow[]) => {
  if (rows.length === 0) return 'Add at least one level.'
  const percents = rows.map(r => parseAmount(r.percentOfAmi))
  if (percents.some(p => !Number.isInteger(p) || p < 1 || p > 200)) return 'Every level needs a percent of AMI from 1 to 200.'
  if (new Set(percents).size !== percents.length) return 'Two levels have the same percent of AMI.'
  if (rows.some(r => r.limits.some(l => !(parseAmount(l) > 0)))) return 'Every level needs all eight limits, above zero.'
  return null
}

export default function IncomeLimitSchedulePanel({ schedule, newest, onClose }: Props) {
  const [effectiveFrom, setEffectiveFrom] = useState<CalendarDate>(schedule?.effectiveFrom ?? trustToday())
  const [rows, setRows] = useState<LevelRow[]>(() => initialRows(schedule, newest))
  const [isDirty, setIsDirty] = useState(false)
  const [rowsError, setRowsError] = useState<string | null>(null)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const enter = useEnterIncomeLimitSchedule()
  const correct = useCorrectIncomeLimitSchedule()
  const { success } = useToast()
  const isPending = enter.isPending || correct.isPending

  const updateRows = (update: (rows: LevelRow[]) => LevelRow[]) => {
    setRows(update)
    setIsDirty(true)
  }

  const setPercent = (key: number, value: string) =>
    updateRows(rs => rs.map(r => (r.key === key ? { ...r, percentOfAmi: value } : r)))

  const setLimit = (key: number, index: number, value: string) =>
    updateRows(rs => rs.map(r => (r.key === key ? { ...r, limits: r.limits.map((l, i) => (i === index ? value : l)) } : r)))

  /** Fills the cells right of and below the one pasted into (column 0 is the percent of AMI). */
  const handlePaste = (e: React.ClipboardEvent<HTMLInputElement>, rowIndex: number, column: number) => {
    const lines = parsePastedLimits(e.clipboardData.getData('text'))
    if (lines.length === 0) return
    e.preventDefault()
    updateRows(rs => {
      const next = rs.map(r => ({ ...r, limits: [...r.limits] }))
      while (next.length < rowIndex + lines.length) next.push(row(''))
      lines.forEach((line, i) => {
        const target = next[rowIndex + i]
        if (line.percentOfAmi !== null) target.percentOfAmi = String(line.percentOfAmi)
        const first = column === 0 && line.percentOfAmi !== null ? 1 : column
        // More numbers than cells: the limits are the last numbers on a HUD row, after e.g. the median income.
        const room = PUBLISHED_HOUSEHOLD_SIZES + 1 - first
        line.amounts.slice(-room).forEach((amount, j) => {
          const c = first + j
          if (c === 0) target.percentOfAmi = String(amount)
          else if (c <= PUBLISHED_HOUSEHOLD_SIZES) target.limits[c - 1] = String(amount)
        })
      })
      return next
    })
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)
    const error = rowsProblem(rows)
    setRowsError(error)
    if (error) return

    const request = {
      effectiveFrom,
      levels: rows.map(r => ({ percentOfAmi: parseAmount(r.percentOfAmi), limits: r.limits.map(parseAmount) })),
    }
    try {
      if (schedule) {
        await correct.mutateAsync({ scheduleId: schedule.id!, request })
      } else {
        await enter.mutateAsync(request)
      }
      success('Income limits saved')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to save the income limits.'))
    }
  }

  return (
    <SlideOver
      title={schedule ? 'Correct Income Limits' : 'Enter Income Limits'}
      subtitle="Copy the limits as published by HUD or the state agency"
      onClose={onClose}
      wide
      isDirty={isDirty}
      footer={
        <>
          <SlideOverCancelButton />
          <button className="btn btn-primary" type="submit" form="income-limit-schedule-form" disabled={isPending}>
            {isPending ? 'Saving…' : 'Save Income Limits'}
          </button>
        </>
      }
    >
      <form id="income-limit-schedule-form" onSubmit={handleSubmit} noValidate>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="income-limits-effective-from">Effective From</label>
            <DateInput
              id="income-limits-effective-from"
              className="form-input"
              required
              value={effectiveFrom}
              onChange={v => { setEffectiveFrom(v); setIsDirty(true) }}
            />
          </div>
        </div>

        <table className="limit-grid" style={{ marginTop: 16 }}>
          <thead>
            <tr>
              <th />
              <th colSpan={PUBLISHED_HOUSEHOLD_SIZES} className="limit-grid__group">Household size (people)</th>
              <th />
            </tr>
            <tr>
              <th>% of AMI</th>
              {SIZES.map(size => <th key={size} className="limit-grid__size">{size}</th>)}
              <th />
            </tr>
          </thead>
          <tbody>
            {rows.map((r, index) => (
              <tr key={r.key}>
                <td className="limit-grid__percent">
                  <input
                    aria-label="Percent of AMI"
                    className="form-input"
                    inputMode="numeric"
                    value={r.percentOfAmi}
                    onChange={e => setPercent(r.key, e.target.value)}
                    onPaste={e => handlePaste(e, index, 0)}
                  />
                </td>
                {r.limits.map((limit, i) => (
                  <td key={i}>
                    <input
                      aria-label={`${r.percentOfAmi || 'New'}% of AMI, ${sizeLabel(i + 1)}`}
                      className="form-input"
                      inputMode="numeric"
                      value={limit}
                      onChange={e => setLimit(r.key, i, e.target.value)}
                      onPaste={e => handlePaste(e, index, i + 1)}
                    />
                  </td>
                ))}
                <td>
                  <button
                    type="button"
                    className="btn btn-ghost btn-sm"
                    aria-label={`Remove level ${index + 1}`}
                    onClick={() => updateRows(rs => rs.filter(x => x.key !== r.key))}
                  >
                    Remove
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        <button type="button" className="btn btn-secondary btn-sm" style={{ marginTop: 12 }} onClick={() => updateRows(rs => [...rs, row('')])}>
          Add Level
        </button>

        <p className="form-hint" style={{ marginTop: 12 }}>
          To paste HUD&apos;s table, copy its rows and paste them into the 1-person cell of the first row to fill. The paste fills
          right and down from that cell and adds levels as needed. A label like &quot;Very Low (50%)&quot; sets the
          level&apos;s percent of AMI.
        </p>
        <p className="form-hint" style={{ marginTop: 8 }}>
          A household of more than 8 people gets the 8-person limit plus 8% of the 4-person limit for each person over 8,
          rounded to the nearest $50.
          {schedule && ' Selections already started keep the limits they were judged by.'}
        </p>

        {rowsError && <ApiProblemAlert problem={{ message: rowsError }} />}
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
