import { useState } from 'react'
import type { GroundLeaseResponse, InsuranceCertificateDto, InsuranceRequirementsChangeDto } from '../../api'
import { downloadDocument } from '../../hooks/documents'
import { daysBetween, formatCalendarDate, trustToday } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { SHORTFALL_LABELS } from './insuranceCheck'
import RecordInsuranceCertificatePanel from './RecordInsuranceCertificatePanel'
import RemoveInsuranceCertificatePanel from './RemoveInsuranceCertificatePanel'
import SetInsuranceRequirementsPanel from './SetInsuranceRequirementsPanel'

/**
 * The insurance the lease requires (Art 9.4), with the way to change it and its history, and the homeowner's
 * certificates: covered-through, how the current one compares with the lease, and each one recorded.
 */
export default function LeaseInsuranceSection({ lease }: { lease: GroundLeaseResponse }) {
  const [setting, setSetting] = useState(false)
  const [recording, setRecording] = useState(false)
  const [removing, setRemoving] = useState<InsuranceCertificateDto | null>(null)
  const insurance = lease.insurance
  if (!insurance) return null
  const active = lease.status === 'Active'
  const history = insurance.requirementsHistory ?? []
  const certificates = insurance.certificates ?? []
  const check = insurance.currentCheck
  const coveredThrough = insurance.coveredThrough
  const today = trustToday()
  // Red when cover has ended or none is on file, amber within the 30 days Today warns from.
  const coverColor = !coveredThrough || coveredThrough < today
    ? 'var(--red)'
    : daysBetween(today, coveredThrough) <= 30 ? 'var(--warning)' : undefined

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
        {active && (
          <div className="detail-field">
            <div className="detail-field__label">Covered Through</div>
            <div className="detail-field__value" style={coverColor ? { color: coverColor } : undefined}>
              {coveredThrough ? formatCalendarDate(coveredThrough) : 'No certificate on file'}
            </div>
          </div>
        )}
        {check && (
          <div className="detail-field">
            <div className="detail-field__label">Current Certificate</div>
            <div className="detail-field__value" style={{ color: check.meetsLease ? 'var(--success)' : 'var(--warning)' }}>
              {check.meetsLease
                ? 'Meets the lease'
                : (check.shortfalls ?? []).map(s => SHORTFALL_LABELS[s] ?? s).join('; ')}
            </div>
          </div>
        )}
      </div>

      {active && (
        <div style={{ display: 'flex', gap: 8, marginBottom: 12 }}>
          <button type="button" className="btn btn-primary" onClick={() => setRecording(true)}>
            Record Certificate
          </button>
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

      {certificates.length === 0 ? (
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', margin: '12px 0 0' }}>No certificates recorded</p>
      ) : (
        <ul aria-label="Insurance certificates" style={{ listStyle: 'none', margin: '12px 0 0', padding: 0, display: 'grid', gap: 8 }}>
          {certificates.map(c => (
            <CertificateRow key={c.certificateId} certificate={c} canRemove={active} onRemove={() => setRemoving(c)} />
          ))}
        </ul>
      )}

      {setting && <SetInsuranceRequirementsPanel lease={lease} onClose={() => setSetting(false)} />}
      {recording && <RecordInsuranceCertificatePanel lease={lease} onClose={() => setRecording(false)} />}
      {removing && (
        <RemoveInsuranceCertificatePanel groundLeaseId={lease.id!} certificate={removing} onClose={() => setRemoving(null)} />
      )}
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

function CertificateRow({ certificate: c, canRemove, onRemove }: {
  certificate: InsuranceCertificateDto
  canRemove: boolean
  onRemove: () => void
}) {
  const removed = !!c.removedReason
  const date = (d: typeof c.policyStart) => (d ? formatCalendarDate(d) : '—')
  return (
    <li
      aria-label={`Certificate ${c.policyNumber}`}
      style={{
        padding: '8px 12px',
        border: '1px solid var(--border)',
        borderRadius: 6,
        fontSize: 13,
        opacity: removed ? 0.55 : 1,
      }}
    >
      <div style={{ display: 'flex', justifyContent: 'space-between', gap: 8, alignItems: 'baseline' }}>
        <span style={removed ? { textDecoration: 'line-through' } : undefined}>
          {c.insurer} · {c.policyNumber} · {date(c.policyStart)} – {date(c.policyEnd)}
        </span>
        <span style={{ display: 'flex', gap: 4 }}>
          {c.documentId && (
            <button type="button" className="btn btn-ghost btn-sm" onClick={() => downloadDocument(c.documentId!)}>
              Download
            </button>
          )}
          {canRemove && !removed && (
            <button type="button" className="btn btn-danger-ghost btn-sm" onClick={onRemove}>
              Remove
            </button>
          )}
        </span>
      </div>
      <div style={{ color: 'var(--text-secondary)', marginTop: 2 }}>
        Dwelling {formatPrice(c.dwellingCoverage ?? 0)} · liability {formatPrice(c.liabilityPerOccurrence ?? 0)} ·{' '}
        {c.trustNamedAdditionalInsured ? 'trust named' : 'trust not named'} · received {date(c.receivedOn)}
      </div>
      {c.notes && <div style={{ color: 'var(--text-secondary)', marginTop: 2 }}>{c.notes}</div>}
      {removed && <div style={{ color: 'var(--text-secondary)', marginTop: 2 }}>Removed: {c.removedReason}</div>}
    </li>
  )
}
