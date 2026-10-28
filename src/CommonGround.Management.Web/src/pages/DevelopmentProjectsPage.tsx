import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useDevelopmentProjects } from '../hooks/developmentProjects'
import DevelopmentProjectsTable from './development-projects/DevelopmentProjectsTable'
import DevelopmentProjectDetailPanel from './development-projects/DevelopmentProjectDetailPanel'
import DevelopmentProjectInitiatePanel from './development-projects/DevelopmentProjectInitiatePanel'

type Panel = 'detail' | 'initiate' | null

interface DevelopmentProjectsLocationState {
  openId?: string
  initiateParcelId?: string
  initiateHomeIds?: string[]
}

export default function DevelopmentProjectsPage() {
  const location = useLocation()
  const navigate = useNavigate()

  const [page,      setPage]      = useState(1)
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [panel,     setPanel]     = useState<Panel>(null)
  const [initiateParcelId, setInitiateParcelId] = useState<string | undefined>(undefined)
  const [initiateHomeIds, setInitiateHomeIds] = useState<string[] | undefined>(undefined)

  useEffect(() => {
    const timeout = setTimeout(() => {
      setSearch(searchInput.trim())
      setPage(1)
    }, 300)
    return () => clearTimeout(timeout)
  }, [searchInput])

  const { data, isLoading, isError } = useDevelopmentProjects({
    pageNumber: page,
    search: search || undefined,
  })

  const openDetail = (id: string) => {
    setSelectedId(id)
    setInitiateParcelId(undefined)
    setInitiateHomeIds(undefined)
    setPanel('detail')
  }

  const openInitiate = (parcelId?: string, homeIds?: string[]) => {
    setSelectedId(null)
    setInitiateParcelId(parcelId)
    setInitiateHomeIds(homeIds)
    setPanel('initiate')
  }

  useEffect(() => {
    const state = (location.state as DevelopmentProjectsLocationState | null) ?? null
    const openId = state?.openId
    const parcelId = state?.initiateParcelId
    const homeIds = state?.initiateHomeIds

    if (parcelId) {
      openInitiate(parcelId, homeIds)
      navigate(
        { pathname: location.pathname, search: location.search, hash: location.hash },
        { replace: true, state: null },
      )
      return
    }

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
    setInitiateParcelId(undefined)
    setInitiateHomeIds(undefined)
  }

  const projects = (data?.items ?? [])
    .filter((p): p is typeof p & { id: string; landParcelId: string } =>
      typeof p.id === 'string' && typeof p.landParcelId === 'string',
    )
    .map(p => ({
      id: p.id,
      landParcelId: p.landParcelId,
      name: p.name ?? 'Untitled Project',
      status: p.status ?? '',
      projectType: p.projectType ?? '',
    }))

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Property Registry</div>
          <h1 className="page-header__title">Development Projects</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount.toLocaleString()} project{data.totalCount !== 1 ? 's' : ''} in registry
            </div>
          )}
        </div>
        <button className="btn btn-primary" onClick={() => openInitiate()}>
          <svg width="13" height="13" viewBox="0 0 13 13" fill="none">
            <path d="M6.5 1v11M1 6.5h11" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
          </svg>
          Initiate Project
        </button>
      </div>

      <div className="filter-bar">
        <input
          className="form-input"
          type="search"
          placeholder="Search by name or description"
          aria-label="Search development projects"
          value={searchInput}
          onChange={e => setSearchInput(e.target.value)}
          style={{ maxWidth: 320 }}
        />
      </div>

      <DevelopmentProjectsTable
        projects={projects}
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
        <DevelopmentProjectDetailPanel projectId={selectedId} onClose={closePanel} />
      )}

      {panel === 'initiate' && (
        initiateParcelId ? (
          <DevelopmentProjectInitiatePanel
            onClose={closePanel}
            initialParcelId={initiateParcelId}
            initialHomeIds={initiateHomeIds}
          />
        ) : (
          <DevelopmentProjectInitiatePanel onClose={closePanel} />
        )
      )}
    </div>
  )
}
