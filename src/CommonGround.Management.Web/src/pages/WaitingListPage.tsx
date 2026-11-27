import PageEyebrow from '../components/PageEyebrow'
import { useEffect, useState } from 'react'
import { useWaitingList } from '../hooks/applications'
import ApplicationDetailPanel from './applications/ApplicationDetailPanel'
import WaitingListTable from './waiting-list/WaitingListTable'

const PAGE_SIZE = 20

export default function WaitingListPage() {
  const [page, setPage] = useState(1)
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [selectedApplicationId, setSelectedApplicationId] = useState<string | null>(null)

  useEffect(() => {
    const timeout = setTimeout(() => {
      setSearch(searchInput.trim())
      setPage(1)
    }, 300)
    return () => clearTimeout(timeout)
  }, [searchInput])

  const { data, isLoading, isError, error, refetch } = useWaitingList({
    pageNumber: page,
    pageSize: PAGE_SIZE,
    search: search || undefined,
  })

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <PageEyebrow />
          <h1 className="page-header__title">Waiting List</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount?.toLocaleString()} household{data.totalCount !== 1 ? 's' : ''} waiting · longest waiting first
            </div>
          )}
        </div>
      </div>

      <div className="filter-bar">
        <input
          className="form-input"
          type="search"
          placeholder="Search by member name or email"
          aria-label="Search waiting list"
          value={searchInput}
          onChange={e => setSearchInput(e.target.value)}
          style={{ maxWidth: 320 }}
        />
      </div>

      <WaitingListTable
        entries={data?.items ?? []}
        isLoading={isLoading}
        isError={isError}
        error={error}
        selectedApplicationId={selectedApplicationId}
        page={page}
        totalPages={Number(data?.totalPages ?? 0)}
        totalCount={Number(data?.totalCount ?? 0)}
        pageSize={PAGE_SIZE}
        hasFilters={Boolean(search)}
        onRowClick={setSelectedApplicationId}
        onPage={setPage}
        onRetry={() => { void refetch() }}
      />

      {selectedApplicationId && (
        <ApplicationDetailPanel applicationId={selectedApplicationId} onClose={() => setSelectedApplicationId(null)} />
      )}
    </div>
  )
}
