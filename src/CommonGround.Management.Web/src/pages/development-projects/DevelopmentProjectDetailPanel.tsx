import { useDevelopmentProject } from '../../hooks/developmentProjects'
import { useHomes } from '../../hooks/homes'
import { HOME_TYPE_LABELS, PROJECT_TYPE_LABELS } from '../../hooks/types'
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
  const projectHomeIds = detail?.homeIds ?? []
  const { data: parcelHomes } = useHomes({
    pageNumber: 1,
    pageSize: 100,
    landParcelId: detail?.landParcelId,
    enabled: projectHomeIds.length > 0,
  })
  const homes = projectHomeIds.length > 0
    ? (parcelHomes?.items ?? []).filter(h => h.id && projectHomeIds.includes(h.id))
    : []
  const typeLabel = detail ? (PROJECT_TYPE_LABELS[detail.projectType ?? ''] ?? detail.projectType) : ''

  return (
    <SlideOver
      title={detail?.name ?? '…'}
      subtitle={detail ? `${typeLabel} · ${detail.status}` : undefined}
      onClose={onClose}
    >
      {isLoading || !detail ? <SkeletonRows /> : (
        <div className="detail-grid">
          <div className="detail-field">
            <div className="detail-field__label">Status</div>
            <div className="detail-field__value"><StatusBadge status={detail.status ?? ''} /></div>
          </div>
          <div className="detail-field">
            <div className="detail-field__label">Type</div>
            <div className="detail-field__value">{typeLabel}</div>
          </div>
          {detail.description && (
            <div className="detail-field full-span">
              <div className="detail-field__label">Description</div>
              <div className="detail-field__value">{detail.description}</div>
            </div>
          )}
          {homes.length > 0 && (
            <div className="detail-field full-span">
              <div className="detail-field__label">Homes</div>
              <div style={{ display: 'grid', gap: 6 }}>
                {homes.map(h => (
                  <div
                    key={h.id}
                    style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 10 }}
                  >
                    <span className="detail-field__value">
                      {HOME_TYPE_LABELS[h.homeType ?? ''] ?? h.homeType}
                      {' · '}
                      <span className="cell-mono">{h.bedrooms} bd / {Number(h.bathrooms).toFixed(1)} ba</span>
                    </span>
                    <NavIdLink id={h.id} to="/homes" label="Open Home" />
                  </div>
                ))}
              </div>
            </div>
          )}
          <div className="detail-field full-span">
            <div className="detail-field__value">
              <NavIdLink id={detail.landParcelId} to="/land-parcels" label="Open Land Parcel" />
            </div>
          </div>
        </div>
      )}
    </SlideOver>
  )
}
