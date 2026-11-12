import type { IncomeLimitScheduleResponse } from '../../api'
import SlideOver from '../../components/SlideOver'
import { PUBLISHED_HOUSEHOLD_SIZES } from '../../hooks/incomeLimits'
import { formatCalendarDate, formatInstant } from '../../lib/calendarDate'
import { fmtCurrency } from './format'

type Props = {
  schedule: IncomeLimitScheduleResponse
  nameOf: (userId?: string | null) => string
  onCorrect: () => void
  onClose: () => void
}

const SIZES = Array.from({ length: PUBLISHED_HOUSEHOLD_SIZES }, (_, i) => i + 1)

export default function IncomeLimitScheduleDetailPanel({ schedule, nameOf, onCorrect, onClose }: Props) {
  return (
    <SlideOver
      title={`Income Limits from ${formatCalendarDate(schedule.effectiveFrom!)}`}
      subtitle={schedule.isInEffect ? 'In effect' : undefined}
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" type="button" onClick={onClose}>Close</button>
          <button className="btn btn-primary" type="button" onClick={onCorrect}>Correct</button>
        </>
      }
    >
      <div className="registry-table-wrap">
        <table className="registry-table">
          <thead>
            <tr>
              <th>Household Size</th>
              {(schedule.levels ?? []).map(l => <th key={l.percentOfAmi}>{l.percentOfAmi}% of AMI</th>)}
            </tr>
          </thead>
          <tbody>
            {SIZES.map(size => (
              <tr key={size}>
                <td className="cell-mono">{size}</td>
                {(schedule.levels ?? []).map(l => (
                  <td key={l.percentOfAmi} className="cell-mono">{fmtCurrency(Number(l.limits?.[size - 1] ?? 0))}</td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="detail-grid" style={{ marginTop: 16 }}>
        <div className="detail-field">
          <div className="detail-field__label">Entered By</div>
          <div className="detail-field__value">{nameOf(schedule.enteredBy)}</div>
        </div>
        <div className="detail-field">
          <div className="detail-field__label">Entered At</div>
          <div className="detail-field__value mono">{schedule.enteredAt ? formatInstant(schedule.enteredAt) : '—'}</div>
        </div>
        {schedule.correctedAt && (
          <>
            <div className="detail-field">
              <div className="detail-field__label">Corrected By</div>
              <div className="detail-field__value">{nameOf(schedule.correctedBy)}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Corrected At</div>
              <div className="detail-field__value mono">{formatInstant(schedule.correctedAt)}</div>
            </div>
          </>
        )}
      </div>
    </SlideOver>
  )
}
