import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useDevelopmentProjects } from '../hooks/developmentProjects'
import DevelopmentProjectsTable from './development-projects/DevelopmentProjectsTable'
import DevelopmentProjectDetailPanel from './development-projects/DevelopmentProjectDetailPanel'
import DevelopmentProjectInitiatePanel from './development-projects/DevelopmentProjectInitiatePanel'

type Panel = 'detail' | 'initiate' | null

export default function DevelopmentProjectsPage() {
  const location = useLocation()
  const navigate = useNavigate()

  const [page,      setPage]      = useState(1)
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [panel,     setPanel]     = useState<Panel>(null)

  const { data, isLoading, isError } = useDevelopmentProjects({
    pageNumber: page,
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

  const projects = data?.items ?? []

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
        <button className="btn btn-primary" onClick={() => setPanel('initiate')}>
          <svg width="13" height="13" viewBox="0 0 13 13" fill="none">
            <path d="M6.5 1v11M1 6.5h11" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
          </svg>
          Initiate Project
        </button>
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
        <DevelopmentProjectInitiatePanel onClose={closePanel} />
      )}
    </div>
  )
}
