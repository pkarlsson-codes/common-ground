import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useHomes } from '../hooks/homes'
import { HOME_STATUSES, statusLabel } from '../hooks/types'
import EntityCombobox from '../components/EntityCombobox'
import { LandParcelsApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import HomesTable from './homes/HomesTable'
import HomeDetailPanel from './homes/HomeDetailPanel'
import HomeRegisterPanel from './homes/HomeRegisterPanel'

const landParcelsApi = new LandParcelsApi(apiConfig)

type Panel = 'detail' | 'register' | null

export default function HomesPage() {
  const location = useLocation()
  const navigate = useNavigate()

  const [page,         setPage]         = useState(1)
  const [statusFilter, setStatusFilter] = useState('')
  const [parcelFilter, setParcelFilter] = useState('')
  const [selectedId,   setSelectedId]   = useState<string | null>(null)
  const [panel,        setPanel]        = useState<Panel>(null)

  const { data, isLoading, isError } = useHomes({
    pageNumber: page,
    landParcelId: parcelFilter || undefined,
    status: statusFilter || undefined,
  })

  const openDetail = (id: string) => {
    setSelectedId(id)
    setPanel('detail')
  }

  useEffect(() => {
    const openId = (location.state as { openId?: string } | null)?.openId
    if (!openId) return
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

  const clearParcelFilter = () => {
    setParcelFilter('')
    setPage(1)
  }

  const homes = data?.items ?? []

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Property Registry</div>
          <h1 className="page-header__title">Homes</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount?.toLocaleString()} home{data.totalCount !== 1 ? 's' : ''} in registry
            </div>
          )}
        </div>
        <button className="btn btn-primary" onClick={() => setPanel('register')}>
          <svg width="13" height="13" viewBox="0 0 13 13" fill="none">
            <path d="M6.5 1v11M1 6.5h11" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
          </svg>
          Register Home
        </button>
      </div>

      <div className="filter-bar">
        <select
          className="select"
          value={statusFilter}
          onChange={e => { setStatusFilter(e.target.value); setPage(1) }}
        >
          <option value="">All Statuses</option>
          {HOME_STATUSES.map(s => (
            <option key={s} value={s}>{statusLabel(s)}</option>
          ))}
        </select>

        <div style={{ display: 'flex', gap: 6, alignItems: 'center' }}>
          <EntityCombobox
            value={parcelFilter || null}
            onChange={id => { setParcelFilter(id ?? ''); setPage(1) }}
            fetchOptions={async q => {
              const result = await landParcelsApi.getLandParcels({ search: q, pageSize: 10 })
              return (result.items ?? []).map(p => ({
                id: p.id!,
                label: [p.legalParcelNumber, p.address].filter(Boolean).join(' — '),
              }))
            }}
            placeholder="Filter by parcel…"
          />
          {parcelFilter && (
            <button type="button" className="btn btn-ghost btn-sm" onClick={clearParcelFilter}>
              Clear
            </button>
          )}
        </div>
      </div>

      <HomesTable
        homes={homes}
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
        <HomeDetailPanel homeId={selectedId} onClose={closePanel} />
      )}

      {panel === 'register' && (
        <HomeRegisterPanel onClose={closePanel} />
      )}
    </div>
  )
}
