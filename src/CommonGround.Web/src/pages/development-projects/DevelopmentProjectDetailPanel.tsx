import { useDevelopmentProject } from '../../hooks/developmentProjects'
import SlideOver from '../../components/SlideOver'
import StatusBadge from '../../components/StatusBadge'
import NavIdLink from '../../components/NavIdLink'

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

interface Props {
  projectId: string
  onClose: () => void
}

export default function DevelopmentProjectDetailPanel({ projectId, onClose }: Props) {
  const { data: detail, isLoading } = useDevelopmentProject(projectId)

  return (
    <SlideOver
      title={detail?.name ?? '…'}
      subtitle={detail ? `${detail.status}` : undefined}
      onClose={onClose}
    >
      {isLoading || !detail ? <SkeletonRows /> : (
        <>
          <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: 8, position: 'relative' }} />

          {/* Action buttons removed — not supported by gateway */}

          {/* Activate action removed — not supported by gateway */}

          {/* Completion action removed — not supported by gateway */}

          <div className="detail-grid">
            <div className="detail-field">
              <div className="detail-field__label">Status</div>
              <div className="detail-field__value"><StatusBadge status={detail.status} /></div>
            </div>
            {detail.projectedStartDate && (
              <div className="detail-field">
                <div className="detail-field__label">Projected Start</div>
                <div className="detail-field__value mono">
                  {new Date(detail.projectedStartDate).toLocaleDateString()}
                </div>
              </div>
            )}
            {detail.projectedEndDate && (
              <div className="detail-field">
                <div className="detail-field__label">Projected End</div>
                <div className="detail-field__value mono">
                  {new Date(detail.projectedEndDate).toLocaleDateString()}
                </div>
              </div>
            )}
            {detail.actualCompletionDate && (
              <div className="detail-field">
                <div className="detail-field__label">Completed</div>
                <div className="detail-field__value mono">
                  {new Date(detail.actualCompletionDate).toLocaleDateString()}
                </div>
              </div>
            )}
            {detail.budget != null && (
              <div className="detail-field">
                <div className="detail-field__label">Budget</div>
                <div className="detail-field__value mono">
                  {new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(detail.budget)}
                </div>
              </div>
            )}
            {detail.description && (
              <div className="detail-field full-span">
                <div className="detail-field__label">Description</div>
                <div className="detail-field__value">{detail.description}</div>
              </div>
            )}
            <div className="detail-field full-span">
              <div className="detail-field__value">
                <NavIdLink id={detail.landParcelId} to="/land-parcels" label="Open Land Parcel" />
              </div>
            </div>
          </div>
        </>
      )}
    </SlideOver>
  )
}
