import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useHouseholds } from '../hooks/households'
import HouseholdDetailPanel from './households/HouseholdDetailPanel'
import HouseholdsTable from './households/HouseholdsTable'

const PAGE_SIZE = 20

export default function HouseholdsPage() {
  const location = useLocation()
  const navigate = useNavigate()

  const [page, setPage] = useState(1)
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [selectedId, setSelectedId] = useState<string | null>(null)

  useEffect(() => {
    const timeout = setTimeout(() => {
      setSearch(searchInput.trim())
      setPage(1)
    }, 300)
    return () => clearTimeout(timeout)
  }, [searchInput])

  const { data, isLoading, isError, error, refetch } = useHouseholds({
    pageNumber: page,
    pageSize: PAGE_SIZE,
    search: search || undefined,
  })

  const households = data?.items ?? []

  const openDetail = (id: string) => setSelectedId(id)

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

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Homeownership</div>
          <h1 className="page-header__title">Households</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount?.toLocaleString()} household{data.totalCount !== 1 ? 's' : ''} registered
            </div>
          )}
        </div>
      </div>

      <div className="filter-bar">
        <input
          className="form-input"
          type="search"
          placeholder="Search by member name or email"
          aria-label="Search households"
          value={searchInput}
          onChange={e => setSearchInput(e.target.value)}
          style={{ maxWidth: 320 }}
        />
      </div>

      <HouseholdsTable
        households={households}
        isLoading={isLoading}
        isError={isError}
        selectedId={selectedId}
        page={page}
        totalPages={Number(data?.totalPages ?? 0)}
        totalCount={Number(data?.totalCount ?? 0)}
        pageSize={PAGE_SIZE}
        hasFilters={Boolean(search)}
        onRowClick={openDetail}
        onPage={setPage}
        onRetry={() => { void refetch() }}
        error={error}
      />

      {selectedId && (
        <HouseholdDetailPanel householdId={selectedId} onClose={() => setSelectedId(null)} />
      )}
    </div>
  )
}
