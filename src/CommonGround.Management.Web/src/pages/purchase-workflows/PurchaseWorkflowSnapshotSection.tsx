import type { PurchaseWorkflowResponse } from '../../api'
import { formatStepDate } from '../../lib/lifecycle'
import ConnectedRecords, { type ConnectedRecord } from '../../components/lifecycle/ConnectedRecords'
import StatusBadge from '../../components/StatusBadge'
import { useConnectedGroundLeases } from '../../hooks/groundLeases'
import { formatPhoneDisplay } from '../../lib/phone'

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
            border: '1px solid var(--warning)',
            background: 'var(--warning-surface)',
            color: 'var(--warning)',
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
            <div className="detail-field__value mono">{formatPhoneDisplay(detail.buyerPhone)}</div>
          </div>
        )}
        {detail.scheduledClosingDate && (
          <div className="detail-field">
            <div className="detail-field__label">Scheduled Closing</div>
            <div className="detail-field__value mono">{formatStepDate(detail.scheduledClosingDate)}</div>
          </div>
        )}
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
      <PurchaseConnectedRecords detail={detail} />
    </>
  )
}

/** Links to the purchase's home, land parcel, lease template, application, household, and the lease its closing created. */
function PurchaseConnectedRecords({ detail }: { detail: PurchaseWorkflowResponse }) {
  const { data: leases, isLoading } = useConnectedGroundLeases({ homeId: detail.phase === 'Closed' ? detail.homeId ?? undefined : undefined })
  const lease = leases?.items?.find(l => l.purchaseWorkflowId === detail.id)

  const records: ConnectedRecord[] = [
    { key: 'home', type: 'Home', name: detail.homeLabel ?? 'Home', to: '/homes', openId: detail.homeId! },
    { key: 'parcel', type: 'Land parcel', name: 'Land parcel', to: '/land-parcels', openId: detail.landParcelId! },
    { key: 'template', type: 'Lease template', name: 'Lease template', to: '/ground-lease-templates', openId: detail.groundLeaseTemplateId! },
    ...(detail.applicationId
      ? [{ key: 'application', type: 'Application', name: 'Application', to: '/applications', openId: detail.applicationId }]
      : []),
    ...(detail.householdId
      ? [{ key: 'household', type: 'Household', name: detail.buyerFullName ?? 'Household', to: '/households', openId: detail.householdId }]
      : []),
    ...(lease
      ? [{ key: 'lease', type: 'Ground lease', name: lease.homeownerFullName ?? 'Ground lease', status: lease.status ?? undefined, to: '/ground-leases', openId: lease.id! }]
      : []),
  ]

  return <ConnectedRecords records={records} isLoading={isLoading && detail.phase === 'Closed'} />
}
