import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useLandParcels } from '../hooks/landParcels'
import { PARCEL_STATUSES, statusLabel } from '../hooks/types'
import LandParcelsTable from './land-parcels/LandParcelsTable'
import LandParcelDetailPanel from './land-parcels/LandParcelDetailPanel'
import LandParcelRegisterPanel from './land-parcels/LandParcelRegisterPanel'

type Panel = 'detail' | 'register' | null

export default function LandParcelsPage() {
  const location = useLocation()
  const navigate = useNavigate()

  const [page,         setPage]         = useState(1)
  const [statusFilter, setStatusFilter] = useState('')
  const [selectedId,   setSelectedId]   = useState<string | null>(null)
  const [panel,        setPanel]        = useState<Panel>(null)

  const { data, isLoading, isError } = useLandParcels(page, statusFilter || undefined)

  const openDetail = (id: string) => {
    setSelectedId(id)
    setPanel('detail')
  }

  useEffect(() => {
    const openId = (location.state as { openId?: string } | null)?.openId
    if (!openId) {
      return
    }

    openDetail(openId)
    navigate(
      { pathname: location.pathname, search: location.search, hash: location.hash },
      { replace: true, state: null },
    )
  }, [location, navigate])

  const closePanel = () => {
    setPanel(null)
    setSelectedId(null)
  }

  const parcels = data?.items ?? []

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Property Registry</div>
          <h1 className="page-header__title">Land Parcels</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount.toLocaleString()} parcel{data.totalCount !== 1 ? 's' : ''} in registry
            </div>
          )}
        </div>
        <button className="btn btn-primary" onClick={() => setPanel('register')}>
          <svg width="13" height="13" viewBox="0 0 13 13" fill="none">
            <path d="M6.5 1v11M1 6.5h11" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
          </svg>
          Register Parcel
        </button>
      </div>

      <div className="filter-bar">
        <select
          className="select"
          value={statusFilter}
          onChange={e => { setStatusFilter(e.target.value); setPage(1) }}
        >
          <option value="">All Statuses</option>
          {PARCEL_STATUSES.map(s => (
            <option key={s} value={s}>{statusLabel(s)}</option>
          ))}
        </select>
      </div>

      <LandParcelsTable
        parcels={parcels}
        isLoading={isLoading}
        isError={isError}
        selectedId={selectedId}
        panel={panel}
        page={page}
        totalPages={Number(data?.totalPages ?? 0)}
        totalCount={Number(data?.totalCount ?? 0)}
        onRowClick={openDetail}
        onPage={setPage}
      />

      {panel === 'detail' && selectedId && (
        <LandParcelDetailPanel parcelId={selectedId} onClose={closePanel} />
      )}

      {panel === 'register' && (
        <LandParcelRegisterPanel onClose={closePanel} />
      )}
    </div>
  )
}
