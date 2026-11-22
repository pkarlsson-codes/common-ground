import { formatInstant } from '../../lib/calendarDate'
import { useState } from 'react'
import { useLandParcel, useDecommissionParcel, useRecommissionParcel } from '../../hooks/landParcels'
import SlideOver from '../../components/SlideOver'
import { useToast } from '../../hooks/useToast'
import HomesSection from './HomesSection'
import ConnectedRecords from '../../components/lifecycle/ConnectedRecords'
import { formatStepDate } from '../../lib/lifecycle'
import { useConnectedGroundLeases } from '../../hooks/groundLeases'
import DevelopmentProjectsSection from './DevelopmentProjectsSection'
import UserName from '../../components/UserName'

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

function ParcelStatusBadge({ isDecommissioned }: { isDecommissioned?: boolean }) {
  if (isDecommissioned) {
    return <span className="badge badge-failed">Decommissioned</span>
  }
  return <span className="badge badge-active">Active</span>
}

interface Props {
  parcelId: string
  onClose: () => void
}

export default function LandParcelDetailPanel({ parcelId, onClose }: Props) {
  const { data: detail, isLoading: detailLoading } = useLandParcel(parcelId)
  const decommission = useDecommissionParcel()
  const recommission = useRecommissionParcel()
  const [menuOpen, setMenuOpen] = useState(false)
  const [confirmAction, setConfirmAction] = useState<'decommission' | 'recommission' | null>(null)
  const { success } = useToast()

  const handleConfirm = async () => {
    if (!confirmAction) return
    try {
      if (confirmAction === 'decommission') {
        await decommission.mutateAsync(parcelId)
        success('Parcel decommissioned')
      } else {
        await recommission.mutateAsync(parcelId)
        success('Parcel recommissioned')
      }
    } catch { /* error visible via mutation state */ }
    setConfirmAction(null)
  }

  const isPending = decommission.isPending || recommission.isPending
  const isError = decommission.isError || recommission.isError

  return (
    <SlideOver
      title={detail?.legalParcelNumber ?? '…'}
      subtitle={detail?.address}
      onClose={onClose}
    >
      {detailLoading || !detail ? <SkeletonRows /> : (
        <>
          <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: 8, position: 'relative' }}>
            <button
              type="button"
              className="btn btn-ghost btn-sm"
              onClick={() => setMenuOpen(o => !o)}
              aria-label="Actions"
            >
              ···
            </button>
            {menuOpen && (
              <div
                style={{
                  position: 'absolute',
                  top: '100%',
                  right: 0,
                  zIndex: 10,
                  background: 'var(--surface-1)',
                  border: '1px solid var(--border)',
                  borderRadius: 8,
                  padding: '4px 0',
                  minWidth: 160,
                  boxShadow: 'var(--shadow-overlay-sm)',
                }}
              >
                {detail.isDecommissioned ? (
                  <button
                    type="button"
                    className="btn btn-ghost"
                    style={{ width: '100%', textAlign: 'left', borderRadius: 0, padding: '8px 16px' }}
                    onClick={() => { setMenuOpen(false); setConfirmAction('recommission') }}
                  >
                    Recommission
                  </button>
                ) : (
                  <button
                    type="button"
                    className="btn btn-ghost"
                    style={{ width: '100%', textAlign: 'left', borderRadius: 0, padding: '8px 16px' }}
                    onClick={() => { setMenuOpen(false); setConfirmAction('decommission') }}
                  >
                    Decommission
                  </button>
                )}
              </div>
            )}
          </div>

          {confirmAction && (
            <div
              style={{
                border: '1px solid var(--border)',
                borderRadius: 8,
                padding: 12,
                marginBottom: 16,
                background: 'var(--surface-1)',
              }}
            >
              <div style={{ fontSize: 13, marginBottom: 10 }}>
                {confirmAction === 'decommission'
                  ? 'Decommission this parcel? It will be marked inactive.'
                  : 'Recommission this parcel? It will be marked active again.'}
              </div>
              <div style={{ display: 'flex', gap: 8 }}>
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={handleConfirm}
                  disabled={isPending}
                >
                  {isPending ? 'Saving…' : 'Confirm'}
                </button>
                <button
                  type="button"
                  className="btn btn-ghost"
                  onClick={() => setConfirmAction(null)}
                  disabled={isPending}
                >
                  Cancel
                </button>
              </div>
              {isError && (
                <div className="form-error" style={{ marginTop: 8 }}>Action failed. Please try again.</div>
              )}
            </div>
          )}

          <div className="detail-grid">
            <div className="detail-field">
              <div className="detail-field__label">Status</div>
              <div className="detail-field__value">
                <ParcelStatusBadge isDecommissioned={detail.isDecommissioned} />
              </div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Area</div>
              <div className="detail-field__value mono">{Number(detail.areaAcres).toFixed(4)} ac</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Acquisition Method</div>
              <div className="detail-field__value">{detail.acquisitionMethod}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Acquisition Date</div>
              <div className="detail-field__value mono">{detail.acquisitionDate}</div>
            </div>
            <div className="detail-field full-span">
              <div className="detail-field__label">Registered By</div>
              <div className="detail-field__value mono"><UserName id={detail.registeredBy} /></div>
            </div>
            {detail.covenantTerms && (
              <div className="detail-field full-span">
                <div className="detail-field__label">Covenant Terms</div>
                <div className="detail-field__value">{detail.covenantTerms}</div>
              </div>
            )}
            <div className="detail-field full-span">
              <div className="detail-field__label">Registered At</div>
              <div className="detail-field__value mono">
                {formatInstant(new Date(detail.registeredAt))}
              </div>
            </div>
          </div>

          <HomesSection parcelId={parcelId} />
          <DevelopmentProjectsSection parcelId={parcelId} />
          <ParcelConnectedRecords parcelId={parcelId} />
        </>
      )}
    </SlideOver>
  )
}

/** Links to the ground leases on the parcel's homes. */
function ParcelConnectedRecords({ parcelId }: { parcelId: string }) {
  const { data: leases, isLoading } = useConnectedGroundLeases({ landParcelId: parcelId })

  return (
    <ConnectedRecords
      isLoading={isLoading}
      records={(leases?.items ?? []).map(l => ({
        key: `lease-${l.id}`,
        type: 'Ground lease',
        name: l.homeownerFullName ?? 'Ground lease',
        meta: `From ${formatStepDate(l.leaseStartDate)}`,
        status: l.status ?? undefined,
        to: '/ground-leases',
        openId: l.id!,
      }))}
    />
  )
}
