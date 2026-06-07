import { useState } from 'react'
import SlideOver from '../../components/SlideOver'
import type { GroundLeaseListItemResponse } from '../../api'
import { FORMULA_KIND_LABELS } from '../../hooks/types'
import { useResaleFlowController } from './useResaleFlowController'

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

interface Props {
  groundLeaseId: string
  lease: GroundLeaseListItemResponse | undefined
  onClose: () => void
  onSuccess: (workflowId?: string) => void
}

const STEP_TITLES = ['Intent', 'Estimate', 'Initiate']

export default function ResaleSlideOver({ groundLeaseId, lease, onClose, onSuccess }: Props) {
  const ctrl = useResaleFlowController({ groundLeaseId, onSuccess })
  const [noticeDate, setNoticeDate] = useState(new Date().toISOString().slice(0, 10))

  const title = `Resale — ${lease?.homeownerFullName ?? groundLeaseId.slice(0, 8) + '…'}`
  const subtitle = `Step ${ctrl.step + 1} of 3: ${STEP_TITLES[ctrl.step]}`

  return (
    <SlideOver title={title} subtitle={subtitle} onClose={onClose}>
      {/* ── Step 0: Intent ─────────────────────────────────────────── */}
      {ctrl.step === 0 && (
        <div>
          <div className="detail-grid" style={{ marginBottom: 20 }}>
            <div className="detail-field">
              <div className="detail-field__label">Homeowner</div>
              <div className="detail-field__value">
                {lease?.homeownerFullName ?? <span style={{ color: 'var(--text-dim)' }}>—</span>}
              </div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Lease Start Date</div>
              <div className="detail-field__value mono">{lease?.leaseStartDate ?? '—'}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Formula</div>
              <div className="detail-field__value">
                {(lease as { formulaKind?: string } | undefined)?.formulaKind
                  ? FORMULA_KIND_LABELS[(lease as { formulaKind?: string }).formulaKind!] ?? (lease as { formulaKind?: string }).formulaKind
                  : <span style={{ color: 'var(--text-dim)' }}>—</span>}
              </div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Ground Lease ID</div>
              <div className="detail-field__value mono" style={{ fontSize: 11 }}>{groundLeaseId}</div>
            </div>
          </div>

          <div className="form-grid" style={{ marginBottom: 20 }}>
            <div className="form-field full">
              <label className="form-label">Homeowner Notice Date</label>
              <input
                className="form-input"
                type="date"
                value={noticeDate}
                onChange={e => setNoticeDate(e.target.value)}
              />
            </div>
          </div>

          <button className="btn btn-primary" onClick={ctrl.nextStep}>
            Calculate Resale Price →
          </button>
        </div>
      )}

      {/* ── Step 1: Estimate ────────────────────────────────────────── */}
      {ctrl.step === 1 && (
        <div>
          <div
            style={{
              padding: '10px 14px',
              background: 'var(--amber-dim)',
              borderRadius: 6,
              marginBottom: 16,
              fontSize: 13,
              color: 'var(--amber-bright)',
              borderLeft: '3px solid var(--amber)',
            }}
          >
            Original purchase price must be sourced from paper records or closing documents.
          </div>

          {ctrl.estimateResult !== null && (
            <div
              style={{
                padding: '12px 16px',
                background: 'var(--amber-dim)',
                borderRadius: 6,
                marginBottom: 14,
                fontFamily: 'var(--font-mono)',
                fontSize: 20,
                color: 'var(--amber-bright)',
              }}
            >
              Max resale: {fmtCurrency(ctrl.estimateResult)}
            </div>
          )}

          <form onSubmit={ctrl.handleEstimate}>
            <div className="form-grid" style={{ marginBottom: 12 }}>
              <div className="form-field">
                <label className="form-label">Original Purchase Price ($)</label>
                <input
                  className="form-input"
                  type="number"
                  min="0"
                  step="1"
                  value={ctrl.estimateForm.originalPurchasePrice || ''}
                  onChange={e => ctrl.setEstimateForm(f => ({ ...f, originalPurchasePrice: parseFloat(e.target.value) || 0 }))}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Current Appraisal Value ($)</label>
                <input
                  className="form-input"
                  type="number"
                  min="0"
                  step="1"
                  value={ctrl.estimateForm.currentAppraisalValue || ''}
                  onChange={e => ctrl.setEstimateForm(f => ({ ...f, currentAppraisalValue: parseFloat(e.target.value) || 0 }))}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Original Appraisal Value ($)</label>
                <input
                  className="form-input"
                  type="number"
                  min="0"
                  step="1"
                  value={ctrl.estimateForm.originalAppraisalValue || ''}
                  onChange={e => ctrl.setEstimateForm(f => ({ ...f, originalAppraisalValue: parseFloat(e.target.value) || 0 }))}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Ownership Years</label>
                <input
                  className="form-input"
                  type="number"
                  min="0"
                  step="0.1"
                  value={ctrl.estimateForm.ownershipYears || ''}
                  onChange={e => ctrl.setEstimateForm(f => ({ ...f, ownershipYears: parseFloat(e.target.value) || 0 }))}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Approved Improvements ($)</label>
                <input
                  className="form-input"
                  type="number"
                  min="0"
                  step="1"
                  value={ctrl.estimateForm.approvedImprovements || ''}
                  onChange={e => ctrl.setEstimateForm(f => ({ ...f, approvedImprovements: parseFloat(e.target.value) || 0 }))}
                />
              </div>
              <div className="form-field">
                <label className="form-label">Current Index Value</label>
                <input
                  className="form-input"
                  type="number"
                  min="0"
                  step="0.01"
                  value={ctrl.estimateForm.currentIndexValue || ''}
                  onChange={e => ctrl.setEstimateForm(f => ({ ...f, currentIndexValue: parseFloat(e.target.value) || 0 }))}
                />
              </div>
            </div>

            {ctrl.estimateError && (
              <div className="form-error" style={{ marginBottom: 12 }}>{ctrl.estimateError}</div>
            )}

            <div style={{ display: 'flex', gap: 10 }}>
              <button type="button" className="btn btn-secondary" onClick={ctrl.prevStep}>
                Back
              </button>
              <button type="submit" className="btn btn-primary" disabled={ctrl.estimatePending}>
                {ctrl.estimatePending ? 'Calculating…' : 'Initiate Purchase Workflow →'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* ── Step 2: Initiate ─────────────────────────────────────────── */}
      {ctrl.step === 2 && (
        <div>
          <form onSubmit={ctrl.handleInitiate}>
            <div className="form-grid" style={{ marginBottom: 12 }}>
              <div className="form-field full">
                <label className="form-label">Ground Lease</label>
                <div
                  className="form-input"
                  style={{
                    background: 'var(--surface)',
                    color: 'var(--text-secondary)',
                    fontFamily: 'var(--font-mono)',
                    fontSize: 12,
                    cursor: 'default',
                  }}
                >
                  {groundLeaseId}
                </div>
              </div>

              <div className="form-field full">
                <label className="form-label">Purchase Price ($)</label>
                <input
                  className="form-input"
                  type="number"
                  min="0"
                  step="1"
                  value={ctrl.initiateForm.purchasePrice || ''}
                  onChange={e => ctrl.setInitiateForm(f => ({ ...f, purchasePrice: parseFloat(e.target.value) || 0 }))}
                  required
                />
                {ctrl.estimateResult !== null && (
                  <div className="form-help">
                    Formula max: {fmtCurrency(ctrl.estimateResult)}
                  </div>
                )}
              </div>

              <div className="form-field full">
                <label className="form-label">Buyer Full Name</label>
                <input
                  className="form-input"
                  value={ctrl.initiateForm.buyerFullName}
                  onChange={e => ctrl.setInitiateForm(f => ({ ...f, buyerFullName: e.target.value }))}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Buyer Email</label>
                <input
                  className="form-input"
                  type="email"
                  value={ctrl.initiateForm.buyerEmail}
                  onChange={e => ctrl.setInitiateForm(f => ({ ...f, buyerEmail: e.target.value }))}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Buyer Phone (optional)</label>
                <input
                  className="form-input"
                  type="tel"
                  placeholder="+11231231234"
                  value={ctrl.initiateForm.buyerPhone}
                  onChange={e => ctrl.setInitiateForm(f => ({ ...f, buyerPhone: e.target.value }))}
                />
                <div className="form-help">Use E.164 format, for example +11231231234.</div>
              </div>
            </div>

            {ctrl.initiateError && (
              <div className="form-error" style={{ marginBottom: 12 }}>{ctrl.initiateError}</div>
            )}

            <div style={{ display: 'flex', gap: 10 }}>
              <button type="button" className="btn btn-secondary" onClick={ctrl.prevStep}>
                Back
              </button>
              <button type="submit" className="btn btn-primary" disabled={ctrl.initiatePending}>
                {ctrl.initiatePending ? 'Initiating…' : 'Initiate Workflow'}
              </button>
            </div>
          </form>
        </div>
      )}
    </SlideOver>
  )
}
