import { useState } from 'react'
import { useDevelopmentProjects } from '../../hooks/developmentProjects'
import StatusBadge from '../../components/StatusBadge'
import DevelopmentProjectInitiatePanel from '../development-projects/DevelopmentProjectInitiatePanel'

interface Props {
  parcelId: string
  initialHomeIds?: string[]
}

export default function DevelopmentProjectsSection({ parcelId, initialHomeIds }: Props) {
  const [expanded, setExpanded] = useState(true)
  const [showInitiate, setShowInitiate] = useState(false)
  const { data, isLoading } = useDevelopmentProjects({ pageNumber: 1, pageSize: 50, landParcelId: parcelId })
  const projects = data?.items ?? []

  return (
    <>
      <div
        className="detail-section"
        style={{ border: '1px solid var(--border)', borderRadius: 12, padding: 12, background: 'var(--surface-1)' }}
      >
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: expanded ? 10 : 0 }}>
          <button
            type="button"
            className="btn btn-ghost"
            style={{ fontWeight: 600, fontSize: 13, padding: 0 }}
            onClick={() => setExpanded(e => !e)}
          >
            <span style={{ marginRight: 6 }}>{expanded ? '▾' : '▸'}</span>
            Development Projects {!isLoading && `(${projects.length})`}
          </button>
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={() => setShowInitiate(true)}
          >
            + Initiate Project
          </button>
        </div>

        {expanded && (
          <div style={{ display: 'grid', gap: 6 }}>
            {isLoading && <div className="detail-field__value">Loading…</div>}
            {!isLoading && projects.length === 0 && (
              <div className="detail-field__value">No development projects on this parcel.</div>
            )}
            {!isLoading && projects.map(p => (
              <div
                key={p.id}
                style={{
                  display: 'grid',
                  gridTemplateColumns: '1fr auto',
                  gap: 10,
                  alignItems: 'center',
                  padding: '8px 10px',
                  borderRadius: 8,
                  border: '1px solid var(--border)',
                  background: 'var(--surface-2)',
                }}
              >
                <span style={{ fontSize: 13 }}>{p.name ?? 'Untitled Project'}</span>
                <StatusBadge status={p.status ?? ''} />
              </div>
            ))}
          </div>
        )}
      </div>

      {showInitiate && (
        <DevelopmentProjectInitiatePanel
          initialParcelId={parcelId}
          initialHomeIds={initialHomeIds}
          onClose={() => setShowInitiate(false)}
        />
      )}
    </>
  )
}
