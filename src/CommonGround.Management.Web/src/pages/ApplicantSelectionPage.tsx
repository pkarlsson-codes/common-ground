import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useHomeownerApplications } from '../hooks/applicantSelection'
import ApplicationDetailPanel from './applicant-selection/ApplicationDetailPanel'
import ApplicantSelectionTable from './applicant-selection/ApplicantSelectionTable'
import SubmitApplicationPanel from './applicant-selection/SubmitApplicationPanel'

type Panel = 'detail' | 'submit' | null

const PAGE_SIZE = 20

const PHASES = [
  'Submitted',
  'Approved',
  'Selected',
  'Denied',
  'Withdrawn',
]

export default function ApplicantSelectionPage() {
  const location = useLocation()
  const navigate = useNavigate()

  const [page, setPage] = useState(1)
  const [phaseFilter, setPhaseFilter] = useState('')
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [panel, setPanel] = useState<Panel>(null)

  const { data, isLoading, isError, error, refetch } = useHomeownerApplications({
    pageNumber: page,
    pageSize: PAGE_SIZE,
    phase: phaseFilter || undefined,
  })

  const applications = data?.items ?? []

  const openDetail = (id: string) => {
    setSelectedId(id)
    setPanel('detail')
  }

  const closePanel = () => {
    setPanel(null)
    setSelectedId(null)
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

  const hasFilters = Boolean(phaseFilter)

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Homeownership</div>
          <h1 className="page-header__title">Applicant Selection</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount?.toLocaleString()} application{data.totalCount !== 1 ? 's' : ''} in pipeline
            </div>
          )}
        </div>
        <button className="btn btn-primary" onClick={() => setPanel('submit')}>
          <svg width="13" height="13" viewBox="0 0 13 13" fill="none">
            <path d="M6.5 1v11M1 6.5h11" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
          </svg>
          New Application
        </button>
      </div>

      <div className="filter-bar" style={{ gap: 12 }}>
        <select
          className="select"
          value={phaseFilter}
          onChange={e => { setPhaseFilter(e.target.value); setPage(1) }}
        >
          <option value="">All Phases</option>
          {PHASES.map(phase => (
            <option key={phase} value={phase}>{phase}</option>
          ))}
        </select>
      </div>

      <ApplicantSelectionTable
        applications={applications}
        isLoading={isLoading}
        isError={isError}
        error={error}
        selectedId={selectedId}
        panel={panel}
        page={page}
        totalPages={Number(data?.totalPages ?? 0)}
        totalCount={Number(data?.totalCount ?? 0)}
        pageSize={PAGE_SIZE}
        hasFilters={hasFilters}
        onRowClick={openDetail}
        onPage={setPage}
        onRetry={() => { void refetch() }}
      />

      {panel === 'detail' && selectedId && (
        <ApplicationDetailPanel applicationId={selectedId} onClose={closePanel} />
      )}

      {panel === 'submit' && (
        <SubmitApplicationPanel onClose={closePanel} />
      )}
    </div>
  )
}
