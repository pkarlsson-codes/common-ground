import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useHomes } from '../../hooks/homes'
import type { HomeListItemResponse } from '../../api'
import { HOME_TYPE_LABELS } from '../../hooks/types'
import StatusBadge from '../../components/StatusBadge'
import HomeRegisterPanel from '../homes/HomeRegisterPanel'

interface Props {
  parcelId: string
}

export default function HomesSection({ parcelId }: Props) {
  const [expanded, setExpanded] = useState(true)
  const [showRegister, setShowRegister] = useState(false)
  const navigate = useNavigate()
  const { data, isLoading } = useHomes({ pageNumber: 1, pageSize: 50, landParcelId: parcelId })
  const homes = data?.items ?? []

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
            Homes {!isLoading && `(${homes.length})`}
          </button>
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={() => setShowRegister(true)}
          >
            + Register Home
          </button>
        </div>

        {expanded && (
          <div style={{ display: 'grid', gap: 6 }}>
            {isLoading && <div className="detail-field__value">Loading…</div>}
            {!isLoading && homes.length === 0 && (
              <div className="detail-field__value">No homes registered on this parcel.</div>
            )}
            {!isLoading && homes.map((h: HomeListItemResponse) => (
              <button
                type="button"
                key={h.id}
                aria-label={`Open Home: ${h.label}`}
                onClick={() => navigate('/homes', { state: { openId: h.id } })}
                style={{
                  width: '100%',
                  textAlign: 'left',
                  font: 'inherit',
                  color: 'inherit',
                  cursor: 'pointer',
                  display: 'grid',
                  gridTemplateColumns: '1fr auto auto',
                  gap: 10,
                  alignItems: 'center',
                  padding: '8px 10px',
                  borderRadius: 8,
                  border: '1px solid var(--border)',
                  background: 'var(--surface-2)',
                }}
              >
                <span style={{ fontSize: 13 }}>
                  {h.label}
                  {' · '}
                  {HOME_TYPE_LABELS[h.homeType ?? ''] ?? h.homeType}
                  {' · '}
                  <span className="cell-mono">{h.bedrooms} bd / {Number(h.bathrooms).toFixed(1)} ba</span>
                </span>
                <StatusBadge status={h.status ?? ''} />
              </button>
            ))}
          </div>
        )}
      </div>

      {showRegister && (
        <HomeRegisterPanel
          initialParcelId={parcelId}
          onClose={() => setShowRegister(false)}
        />
      )}
    </>
  )
}
