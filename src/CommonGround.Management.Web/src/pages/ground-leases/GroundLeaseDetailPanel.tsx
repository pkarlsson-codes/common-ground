import { useState } from 'react'
import { useGroundLease, useRecordStewardshipCheckIn } from '../../hooks/groundLeases'
import { FORMULA_KIND_LABELS, statusLabel, type RecordCheckInForm } from '../../hooks/types'
import ConnectedRecords, { type ConnectedRecord } from '../../components/lifecycle/ConnectedRecords'
import { useHome } from '../../hooks/homes'
import { usePurchaseWorkflow } from '../../hooks/purchaseWorkflows'
import type { GroundLeaseResponse } from '../../api'
import SlideOver from '../../components/SlideOver'
import StatusBadge from '../../components/StatusBadge'
import { getErrorMessage } from '../../lib/getErrorMessage'
import { formatPhoneDisplay } from '../../lib/phone'
import { useToast } from '../../hooks/useToast'
import { trustToday } from '../../lib/calendarDate'
import { DateInput } from '../../components/DateInput'
import LeaseResaleSection from './LeaseResaleSection'
import LeaseFeesSection from './LeaseFeesSection'
import LeaseMortgagesSection from './LeaseMortgagesSection'
import LeaseImprovementsSection from './LeaseImprovementsSection'
import LeaseInsuranceSection from './LeaseInsuranceSection'
import LeaseResalePriceStatementsSection from './LeaseResalePriceStatementsSection'

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

const fmtPct = (n: number) => `${(n * 100).toFixed(1)}%`

const emptyCheckIn = (): RecordCheckInForm => ({
  checkInDate: trustToday(),
  notes: '',
})

interface Props {
  groundLeaseId: string
  onClose: () => void
}

export default function GroundLeaseDetailPanel({ groundLeaseId, onClose }: Props) {
  const { data: detail, isLoading: detailLoading } = useGroundLease(groundLeaseId)
  const checkInMutation = useRecordStewardshipCheckIn()
  const [checkInForm, setCheckInForm] = useState<RecordCheckInForm>(emptyCheckIn)
  const [checkInError, setCheckInError] = useState<string | null>(null)
  const { success } = useToast()

  // Most recent visit first; same-day check-ins in the order they were recorded, newest first.
  const checkIns = [...(detail?.stewardshipCheckIns ?? [])].sort((a, b) =>
    (b.checkInDate ?? '').localeCompare(a.checkInDate ?? '') || (b.occurredAt?.getTime() ?? 0) - (a.occurredAt?.getTime() ?? 0))

  const handleCheckIn = async (e: React.FormEvent) => {
    e.preventDefault()
    setCheckInError(null)
    try {
      await checkInMutation.mutateAsync({ groundLeaseId, form: checkInForm })
      setCheckInForm(emptyCheckIn())
      success('Check-in recorded')
    } catch (error) {
      setCheckInError(await getErrorMessage(error, 'Failed to record check-in.'))
    }
  }

  return (
    <SlideOver
      title={detail?.homeownerFullName ?? 'Ground Lease'}
      subtitle={detail ? `${detail.leaseStartDate} · ${detail.termYears} yr · ${statusLabel(detail.status ?? '')}` : undefined}
      onClose={onClose}
    >
      {detailLoading || !detail ? <SkeletonRows /> : (
        <>
          <div className="detail-grid">
            <div className="detail-field">
              <div className="detail-field__label">Status</div>
              <div className="detail-field__value"><StatusBadge status={detail.status ?? ''} /></div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Formula</div>
              <div className="detail-field__value">{FORMULA_KIND_LABELS[detail.formulaKind ?? ''] ?? detail.formulaKind}</div>
            </div>
            {detail.appreciationShare != null && (
              <div className="detail-field">
                <div className="detail-field__label">Appreciation Share</div>
                <div className="detail-field__value mono">{fmtPct(detail.appreciationShare)}</div>
              </div>
            )}
            {detail.indexType && (
              <div className="detail-field">
                <div className="detail-field__label">Index Type</div>
                <div className="detail-field__value mono">{detail.indexType}</div>
              </div>
            )}
            {detail.baselineIndexValue != null && (
              <div className="detail-field">
                <div className="detail-field__label">Baseline Index</div>
                <div className="detail-field__value mono">{detail.baselineIndexValue}</div>
              </div>
            )}
            {detail.annualPercentage != null && (
              <div className="detail-field">
                <div className="detail-field__label">Annual %</div>
                <div className="detail-field__value mono">{fmtPct(detail.annualPercentage)}</div>
              </div>
            )}
            <div className="detail-field">
              <div className="detail-field__label">Base Price</div>
              <div className="detail-field__value mono">{fmtCurrency(detail.basePrice ?? 0)}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Original Appraised Value</div>
              <div className="detail-field__value mono">{detail.originalAppraisedValue != null ? fmtCurrency(detail.originalAppraisedValue) : '—'}</div>
            </div>
            {detail.improvementsCap != null && (
              <div className="detail-field">
                <div className="detail-field__label">Improvements Cap</div>
                <div className="detail-field__value mono">{fmtCurrency(detail.improvementsCap)}</div>
              </div>
            )}
            <div className="detail-field">
              <div className="detail-field__label">Start Date</div>
              <div className="detail-field__value mono">{detail.leaseStartDate}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Term</div>
              <div className="detail-field__value mono">{detail.termYears} years</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Option Window</div>
              <div className="detail-field__value mono">{detail.optionWindowDays} days</div>
            </div>
            {detail.homeownerEmail && (
              <div className="detail-field">
                <div className="detail-field__label">Email</div>
                <div className="detail-field__value">{detail.homeownerEmail}</div>
              </div>
            )}
            {detail.homeownerPhone && (
              <div className="detail-field">
                <div className="detail-field__label">Phone</div>
                <div className="detail-field__value mono">{formatPhoneDisplay(detail.homeownerPhone)}</div>
              </div>
            )}
            {detail.covenantTerms && (
              <div className="detail-field full-span">
                <div className="detail-field__label">Covenant Terms</div>
                <div className="detail-field__value">{detail.covenantTerms}</div>
              </div>
            )}
            <div className="detail-field">
              <div className="detail-field__label">Recorded On</div>
              <div className="detail-field__value mono">{detail.recordedOn}</div>
            </div>
          </div>

          <LeaseConnectedRecords lease={detail} />

          <LeaseResaleSection groundLeaseId={groundLeaseId} leaseActive={detail.status === 'Active'} />

          <LeaseResalePriceStatementsSection lease={detail} />

          <LeaseFeesSection lease={detail} />

          <LeaseMortgagesSection lease={detail} />

          <LeaseImprovementsSection lease={detail} />

          <LeaseInsuranceSection lease={detail} />

          <div className="detail-section">
            <div className="detail-section__title">Stewardship Check-Ins</div>
            {checkIns.length > 0 && (
              <div style={{ marginBottom: 14 }}>
                {checkIns.map((c, i) => (
                  <div
                    key={i}
                    style={{
                      padding: '8px 12px',
                      borderLeft: '2px solid var(--amber)',
                      marginBottom: 8,
                      background: 'var(--surface)',
                      borderRadius: '0 4px 4px 0',
                    }}
                  >
                    <div style={{ fontFamily: 'var(--font-mono)', fontSize: 12, color: 'var(--amber)' }}>
                      {c.checkInDate}
                    </div>
                    {c.notes && <div style={{ fontSize: 13, marginTop: 2 }}>{c.notes}</div>}
                    <div style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 2 }}>
                      by {c.recordedBy}
                    </div>
                  </div>
                ))}
              </div>
            )}
            <form onSubmit={handleCheckIn}>
              <div className="form-grid" style={{ marginBottom: 12 }}>
                <div className="form-field">
                  <label className="form-label">Check-In Date</label>
                  <DateInput
                    className="form-input"
                    value={checkInForm.checkInDate}
                    onChange={v => setCheckInForm(f => ({ ...f, checkInDate: v }))}
                    required
                  />
                </div>
                <div className="form-field full">
                  <label className="form-label">Notes</label>
                  <textarea
                    className="form-textarea"
                    value={checkInForm.notes}
                    onChange={e => setCheckInForm(f => ({ ...f, notes: e.target.value }))}
                    placeholder="Observations from the visit…"
                    required
                  />
                </div>
              </div>
              <button
                type="submit"
                className="btn btn-secondary"
                disabled={checkInMutation.isPending}
              >
                {checkInMutation.isPending ? 'Recording…' : 'Record Check-In'}
              </button>
              {checkInError && (
                <div className="form-error">{checkInError}</div>
              )}
            </form>
          </div>
        </>
      )}
    </SlideOver>
  )
}

/** Links to the lease's home, land parcel, and the purchase and household it came from. */
function LeaseConnectedRecords({ lease }: { lease: GroundLeaseResponse }) {
  const { data: home } = useHome(lease.homeId ?? null)
  const { data: purchase } = usePurchaseWorkflow(lease.purchaseWorkflowId ?? null)

  const records: ConnectedRecord[] = [
    ...(lease.homeId ? [{ key: 'home', type: 'Home', name: home?.label || 'Home', to: '/homes', openId: lease.homeId }] : []),
    ...(lease.landParcelId ? [{ key: 'parcel', type: 'Land parcel', name: 'Land parcel', to: '/land-parcels', openId: lease.landParcelId }] : []),
    ...(lease.purchaseWorkflowId
      ? [{ key: 'purchase', type: 'Purchase', name: purchase?.buyerFullName ?? 'Purchase', status: purchase?.phase ?? undefined, to: '/purchase-workflows', openId: lease.purchaseWorkflowId }]
      : []),
    ...(purchase?.householdId
      ? [{ key: 'household', type: 'Household', name: purchase.buyerFullName ?? 'Household', to: '/households', openId: purchase.householdId }]
      : []),
  ]

  return <ConnectedRecords records={records} />
}
