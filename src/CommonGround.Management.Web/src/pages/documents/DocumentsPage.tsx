import { useState } from 'react'
import type { DocumentLibraryItemResponse } from '../../api'
import { useDocuments, useExpiringDocuments, downloadDocument } from '../../hooks/documents'
import DocumentSensitivityBadge from '../../components/DocumentSensitivityBadge'
import DocumentDetailPanel from '../../components/documents/DocumentDetailPanel'
import Pagination from '../../components/Pagination'
import { fmtDate, fmtExpiry, fmtSize, isExpired, isExpiringSoon } from '../../components/documents/format'

const PAGE_SIZE = 20
const EXPIRING_DAYS = 90

type ActiveView = 'library' | 'expiring'

export default function DocumentsPage() {
  const [activeView, setActiveView] = useState<ActiveView>('library')
  const [page, setPage] = useState(1)
  const [expiringPage, setExpiringPage] = useState(1)
  const [search, setSearch] = useState('')
  const [tag, setTag] = useState('')
  const [contentType, setContentType] = useState('')
  const [tagInput, setTagInput] = useState('')
  const [selectedId, setSelectedId] = useState<string | null>(null)

  const { data, isLoading, isError } = useDocuments({
    page,
    pageSize: PAGE_SIZE,
    search: search.trim() || undefined,
    tag: tag || undefined,
    contentType: contentType || undefined,
  })

  const expiringQuery = useExpiringDocuments(EXPIRING_DAYS, expiringPage)
  const expiringCount = expiringQuery.data?.totalCount ?? 0

  const items = data?.items ?? []

  const applyTag = (value: string) => {
    const normalised = value.trim().toLowerCase()
    setTag(normalised)
    setTagInput(normalised)
    setPage(1)
  }

  const clearFilters = () => {
    setSearch('')
    setTag('')
    setTagInput('')
    setContentType('')
    setPage(1)
  }

  const hasFilters = !!search.trim() || !!tag || !!contentType

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Document Library</h1>
          <p className="page-subtitle">All documents across the CLT portfolio</p>
        </div>
      </div>

      {/* Expiring soon banner */}
      {expiringCount > 0 && (
        <div
          style={{
            background: 'var(--amber-dim)',
            border: '1px solid var(--amber)',
            borderRadius: 8,
            padding: '10px 16px',
            marginBottom: 20,
            display: 'flex',
            alignItems: 'center',
            gap: 12,
          }}
        >
          <svg width="16" height="16" viewBox="0 0 20 20" fill="var(--amber-bright)" aria-hidden="true">
            <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
          </svg>
          <span style={{ fontSize: 13, color: 'var(--amber-bright)', fontWeight: 500 }}>
            {expiringCount} document{expiringCount !== 1 ? 's' : ''} expired or expiring within {EXPIRING_DAYS} days
          </span>
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            style={{ marginLeft: 'auto', fontSize: 12 }}
            onClick={() => setActiveView(activeView === 'expiring' ? 'library' : 'expiring')}
          >
            {activeView === 'expiring' ? 'Show all' : 'Show expiring'}
          </button>
        </div>
      )}

      {activeView === 'expiring' && expiringCount > 0 ? (
        <>
          <div style={{ display: 'grid', gap: 6 }}>
            {(expiringQuery.data?.items ?? []).map(item => (
              <ExpiringRow key={item.id} item={item} onOpen={() => setSelectedId(item.id ?? null)} />
            ))}
          </div>
          {(expiringQuery.data?.totalPages ?? 1) > 1 && (
            <Pagination
              page={expiringPage}
              totalPages={expiringQuery.data?.totalPages ?? 1}
              totalCount={expiringCount}
              pageSize={PAGE_SIZE}
              onPage={setExpiringPage}
            />
          )}
        </>
      ) : (
        <>
          {/* Filters */}
          <div style={{ display: 'flex', gap: 8, alignItems: 'center', marginBottom: 16, flexWrap: 'wrap' }}>
            <input
              className="form-input"
              style={{ width: 240, padding: '6px 10px', fontSize: 13 }}
              type="search"
              placeholder="Search by title"
              aria-label="Search documents"
              value={search}
              onChange={e => { setSearch(e.target.value); setPage(1) }}
            />
            <div style={{ display: 'flex', gap: 4 }}>
              <input
                className="form-input"
                style={{ width: 160, padding: '6px 10px', fontSize: 13 }}
                type="text"
                placeholder="Filter by tag…"
                aria-label="Filter by tag"
                value={tagInput}
                onChange={e => setTagInput(e.target.value)}
                onKeyDown={e => { if (e.key === 'Enter') applyTag(tagInput) }}
              />
              <button type="button" className="btn btn-ghost btn-sm" onClick={() => applyTag(tagInput)}>
                Apply
              </button>
            </div>
            <select
              className="form-select"
              style={{ width: 200, padding: '6px 10px', fontSize: 13 }}
              aria-label="File type"
              value={contentType}
              onChange={e => { setContentType(e.target.value); setPage(1) }}
            >
              <option value="">All file types</option>
              <option value="application/pdf">PDF</option>
              <option value="application/msword">Word (.doc)</option>
              <option value="application/vnd.openxmlformats-officedocument.wordprocessingml.document">Word (.docx)</option>
              <option value="image/jpeg">JPEG</option>
              <option value="image/png">PNG</option>
              <option value="text/plain">Text</option>
            </select>
            {hasFilters && (
              <button type="button" className="btn btn-ghost btn-sm" onClick={clearFilters}>
                Clear filters
              </button>
            )}
            {tag && (
              <span style={{
                padding: '2px 8px',
                background: 'var(--amber-dim)',
                color: 'var(--amber-bright)',
                borderRadius: 4,
                fontSize: 12,
                fontFamily: 'var(--font-mono)',
              }}>
                #{tag}
              </span>
            )}
          </div>

          {/* List */}
          {isLoading ? (
            <div className="skeleton-rows">
              {Array.from({ length: 6 }).map((_, i) => <div key={i} className="skeleton-row" />)}
            </div>
          ) : isError ? (
            <div className="form-error">Failed to load documents.</div>
          ) : items.length === 0 ? (
            <div style={{ color: 'var(--text-secondary)', fontSize: 13, padding: '24px 0', textAlign: 'center' }}>
              {hasFilters ? 'No documents match the current filters.' : 'No documents in the library yet.'}
            </div>
          ) : (
            <div style={{ display: 'grid', gap: 6 }}>
              {items.map(item => (
                <LibraryRow
                  key={item.id}
                  item={item}
                  selected={selectedId === item.id}
                  onOpen={() => setSelectedId(item.id ?? null)}
                  onTag={applyTag}
                />
              ))}
            </div>
          )}

          {(data?.totalPages ?? 1) > 1 && (
            <Pagination
              page={page}
              totalPages={data?.totalPages ?? 1}
              totalCount={data?.totalCount ?? 0}
              pageSize={PAGE_SIZE}
              onPage={setPage}
            />
          )}
        </>
      )}

      {selectedId && (
        <DocumentDetailPanel documentId={selectedId} onClose={() => setSelectedId(null)} />
      )}
    </div>
  )
}

function rowStyle(highlight: boolean, selected: boolean) {
  return {
    display: 'grid',
    gridTemplateColumns: '1fr auto',
    gap: 12,
    padding: '12px 16px',
    borderRadius: 8,
    border: `1px solid ${highlight || selected ? 'var(--amber)' : 'var(--border)'}`,
    background: highlight ? 'var(--amber-dim)' : 'var(--surface-2)',
    cursor: 'pointer',
  } as const
}

function DownloadButton({ documentId }: { documentId: string }) {
  return (
    <button
      type="button"
      className="btn btn-ghost btn-sm"
      title="Download current version"
      onClick={e => { e.stopPropagation(); downloadDocument(documentId) }}
    >
      Download
    </button>
  )
}

function LibraryRow({ item, selected, onOpen, onTag }: {
  item: DocumentLibraryItemResponse
  selected: boolean
  onOpen: () => void
  onTag: (tag: string) => void
}) {
  return (
    <div style={rowStyle(false, selected)} onClick={onOpen} data-testid="document-row">
      <div style={{ display: 'grid', gap: 6 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10, flexWrap: 'wrap' }}>
          <span style={{ fontWeight: 600, color: 'var(--text-bright)', fontSize: 14 }}>
            {item.title}
          </span>
          {item.sensitivity && <DocumentSensitivityBadge sensitivity={item.sensitivity} />}
        </div>
        <div style={{ display: 'flex', gap: 12, fontSize: 12, color: 'var(--text-secondary)', flexWrap: 'wrap' }}>
          <span style={{ fontFamily: 'var(--font-mono)' }}>{item.currentFileName}</span>
          {item.currentSizeBytes != null && <span>{fmtSize(item.currentSizeBytes)}</span>}
          {item.currentVersionNumber != null && item.currentVersionNumber > 1 && (
            <span>v{item.currentVersionNumber}</span>
          )}
          <span>{fmtDate(item.lastModifiedAt)}</span>
          {item.expiresAt && (
            <span style={{ color: isExpiringSoon(item.expiresAt) ? 'var(--amber-bright)' : 'inherit' }}>
              {isExpired(item.expiresAt) ? 'expired' : 'expires'} {fmtExpiry(item.expiresAt)}
            </span>
          )}
        </div>
        {item.tags && item.tags.length > 0 && (
          <div style={{ display: 'flex', gap: 4, flexWrap: 'wrap' }}>
            {item.tags.map(t => (
              <button
                key={t}
                type="button"
                aria-label={`Filter by tag ${t}`}
                style={{
                  padding: '1px 6px',
                  background: 'var(--surface-3)',
                  border: '1px solid var(--border)',
                  borderRadius: 3,
                  fontSize: 11,
                  fontFamily: 'var(--font-mono)',
                  color: 'var(--text-secondary)',
                  cursor: 'pointer',
                }}
                onClick={e => { e.stopPropagation(); onTag(t) }}
              >
                #{t}
              </button>
            ))}
          </div>
        )}
      </div>
      <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
        <DownloadButton documentId={item.id ?? ''} />
      </div>
    </div>
  )
}

function ExpiringRow({ item, onOpen }: { item: DocumentLibraryItemResponse; onOpen: () => void }) {
  const expiresAt = item.expiresAt!
  const soon = isExpiringSoon(expiresAt)

  return (
    <div style={rowStyle(soon, false)} onClick={onOpen} data-testid="document-row">
      <div style={{ display: 'grid', gap: 4 }}>
        <span style={{ fontWeight: 600, color: 'var(--text-bright)', fontSize: 14 }}>
          {item.title}
        </span>
        <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}>
          <span style={{ color: soon ? 'var(--amber-bright)' : 'inherit', fontWeight: 500 }}>
            {isExpired(expiresAt) ? 'Expired' : 'Expires'} {fmtExpiry(expiresAt)}
          </span>
          {' · '}
          <span style={{ fontFamily: 'var(--font-mono)' }}>{item.currentFileName}</span>
        </div>
      </div>
      <div style={{ display: 'flex', alignItems: 'center' }}>
        <DownloadButton documentId={item.id ?? ''} />
      </div>
    </div>
  )
}
