import { DateInput } from '../../components/DateInput'
import type { LoanAppraisalForm } from '../../hooks/groundLeases'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'

interface Props {
  appraisal: LoanAppraisalForm
  indexBased: boolean
  /** Whether the value is required (Add Appraisal) or optional (Record Request, where the appraisal often comes last). */
  required: boolean
  onChange: (appraisal: LoanAppraisalForm) => void
}

/** The loan's appraisal: value, date and, on an index-based lease, the index value on that date. */
export default function LoanAppraisalFields({ appraisal, indexBased, required, onChange }: Props) {
  const hasValue = appraisal.appraisedValue > 0
  return (
    <>
      <div className="form-field">
        <label className="form-label" htmlFor="loan-appraised-value">Appraised Value ($)</label>
        <input
          id="loan-appraised-value"
          className="form-input"
          type="number"
          min="0.01"
          step="0.01"
          value={appraisal.appraisedValue || ''}
          onChange={e => onChange({ ...appraisal, appraisedValue: parseFloat(e.target.value) || 0 })}
          required={required}
        />
      </div>
      <div className="form-field">
        <label className="form-label" htmlFor="loan-appraisal-date">Appraisal Date</label>
        <DateInput
          id="loan-appraisal-date"
          className="form-input"
          value={appraisal.appraisalDate}
          max={trustToday()}
          onChange={(v: CalendarDate | null) => onChange({ ...appraisal, appraisalDate: v })}
          required={false}
          aria-required={required || hasValue}
        />
      </div>
      {indexBased && (
        <div className="form-field">
          <label className="form-label" htmlFor="loan-current-index">Current Index Value</label>
          <input
            id="loan-current-index"
            className="form-input"
            type="number"
            min="0.0001"
            step="any"
            value={appraisal.currentIndexValue || ''}
            onChange={e => onChange({ ...appraisal, currentIndexValue: parseFloat(e.target.value) || 0 })}
            required={required || hasValue}
            aria-describedby="loan-current-index-hint"
          />
          <p id="loan-current-index-hint" className="form-hint">The lease's index on the appraisal date.</p>
        </div>
      )}
    </>
  )
}
