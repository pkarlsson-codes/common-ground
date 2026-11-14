import { useState } from 'react'
import type { GroundLeaseResponse, InsuranceRequirementsChangeDto } from '../../api'
import { formatCalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import SetInsuranceRequirementsPanel from './SetInsuranceRequirementsPanel'

/** The insurance the lease requires (Art 9.4), with the way to change it and its history. */
export default function LeaseInsuranceSection({ lease }: { lease: GroundLeaseResponse }) {
  const [setting, setSetting] = useState(false)
  const insurance = lease.insurance
  if (!insurance) return null
  const active = lease.status === 'Active'
  const history = insurance.requirementsHistory ?? []

  return (
    <div className="detail-section">
      <div className="detail-section__title">Insurance</div>
      <div className="detail-grid" style={{ marginBottom: 12 }}>
        <div className="detail-field">
          <div className="detail-field__label">Replacement Value</div>
          <div className="detail-field__value mono">{amountOrNotSet(insurance.requiredReplacementValue)}</div>
        </div>
        <div className="detail-field">
          <div className="detail-field__label">Liability per Occurrence</div>
          <div className="detail-field__value mono">{amountOrNotSet(insurance.requiredLiabilityPerOccurrence)}</div>
        </div>
      </div>

      {active && (
        <div style={{ marginBottom: 12 }}>
          <button type="button" className="btn btn-secondary" onClick={() => setSetting(true)}>
            Set Requirements
          </button>
        </div>
      )}

      {history.length > 1 && (
        <ul aria-label="Insurance requirements history" style={{ fontSize: 13, color: 'var(--text-secondary)', margin: 0, paddingLeft: 18 }}>
          {history.map(change => (
            <li key={`${change.effectiveFrom}-${change.setAt?.toISOString()}`}>{changeText(change)}</li>
          ))}
        </ul>
      )}

      {setting && <SetInsuranceRequirementsPanel lease={lease} onClose={() => setSetting(false)} />}
    </div>
  )
}

const amountOrNotSet = (amount: number | null | undefined, notSet = 'Not set') =>
  amount == null ? notSet : formatPrice(amount)

/** "From {date}: replacement value $280,000.00, liability $300,000.00". */
function changeText(change: InsuranceRequirementsChangeDto) {
  const from = change.effectiveFrom ? formatCalendarDate(change.effectiveFrom) : '—'
  return `From ${from}: replacement value ${amountOrNotSet(change.replacementValue, 'not set')}, liability ${amountOrNotSet(change.liabilityPerOccurrence, 'not set')}`
}
