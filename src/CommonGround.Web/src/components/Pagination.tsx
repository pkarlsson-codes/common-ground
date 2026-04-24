interface Props {
  page: number
  totalPages: number
  totalCount: number
  pageSize: number
  onPage: (p: number) => void
}

export default function Pagination({ page, totalPages, totalCount, pageSize, onPage }: Props) {
  const start = (page - 1) * pageSize + 1
  const end   = Math.min(page * pageSize, totalCount)
  return (
    <div className="pagination">
      <span className="pagination__info">{start}–{end} of {totalCount}</span>
      <div className="pagination__controls">
        <button
          className="btn btn-ghost btn-sm"
          onClick={() => onPage(page - 1)}
          disabled={page <= 1}
        >← Prev</button>
        <button
          className="btn btn-ghost btn-sm"
          onClick={() => onPage(page + 1)}
          disabled={page >= totalPages}
        >Next →</button>
      </div>
    </div>
  )
}
