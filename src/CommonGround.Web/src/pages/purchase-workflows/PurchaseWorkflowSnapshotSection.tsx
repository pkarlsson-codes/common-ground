import type { PurchaseWorkflowResponse } from '../../api'
import NavIdLink from '../../components/NavIdLink'
import StatusBadge from '../../components/StatusBadge'

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

interface PurchaseWorkflowSnapshotSectionProps {
  detail: PurchaseWorkflowResponse
}

export default function PurchaseWorkflowSnapshotSection({ detail }: PurchaseWorkflowSnapshotSectionProps) {
  return (
    <>
      {detail.stalledReason && (
        <div
          role="status"
          style={{
            marginBottom: 20,
            padding: '12px 14px',
            borderRadius: 8,
            border: '1px solid var(--orange-text)',
            background: 'var(--orange-bg)',
            color: 'var(--orange-text)',
            fontSize: 13,
            lineHeight: 1.4,
          }}
        >
          <div style={{ fontFamily: 'var(--font-mono)', fontSize: 10, letterSpacing: '0.16em', textTransform: 'uppercase', marginBottom: 4 }}>
            Stalled
          </div>
          <div>{detail.stalledReason}</div>
          {detail.resolutionTargetDate && (
            <div style={{ marginTop: 4, fontFamily: 'var(--font-mono)', fontSize: 11.5 }}>
              Resolution target: {detail.resolutionTargetDate}
            </div>
          )}
        </div>
      )}
      <div className="detail-grid">
        <div className="detail-field">
          <div className="detail-field__label">Phase</div>
          <div className="detail-field__value"><StatusBadge status={detail.phase ?? ''} /></div>
        </div>
        <div className="detail-field">
          <div className="detail-field__label">Purchase Price</div>
          <div className="detail-field__value mono">{fmtCurrency(Number(detail.purchasePrice))}</div>
        </div>
        <div className="detail-field">
          <div className="detail-field__label">Email</div>
          <div className="detail-field__value">{detail.buyerEmail}</div>
        </div>
        {detail.buyerPhone && (
          <div className="detail-field">
            <div className="detail-field__label">Phone</div>
            <div className="detail-field__value mono">{detail.buyerPhone}</div>
          </div>
        )}
        <div className="detail-field full-span">
          <div className="detail-field__value">
            <NavIdLink id={detail.groundLeaseId} to="/ground-leases" label="Open Ground Lease" />
          </div>
        </div>
        {detail.failureReason && (
          <div className="detail-field full-span">
            <div className="detail-field__label">Failure Reason</div>
            <div className="detail-field__value" style={{ color: 'var(--red)' }}>{detail.failureReason}</div>
          </div>
        )}
        {detail.withdrawalReason && (
          <div className="detail-field full-span">
            <div className="detail-field__label">Withdrawal Reason</div>
            <div className="detail-field__value">{detail.withdrawalReason}</div>
          </div>
        )}
        {detail.closingNotes && (
          <div className="detail-field full-span">
            <div className="detail-field__label">Closing Notes</div>
            <div className="detail-field__value">{detail.closingNotes}</div>
          </div>
        )}
        <div className="detail-field">
          <div className="detail-field__label">Created</div>
          <div className="detail-field__value mono">
            {new Date(detail.createdAt!).toLocaleString()}
          </div>
        </div>
      </div>
    </>
  )
}
