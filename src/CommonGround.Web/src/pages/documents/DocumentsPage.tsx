import { useState } from 'react'
import type { DocumentLibraryItemResponse } from '../../api'
import { useDocuments, useExpiringDocuments, downloadDocument } from '../../hooks/documents'
import DocumentSensitivityBadge from '../../components/DocumentSensitivityBadge'

const fmtSize = (bytes: number): string => {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

const fmtDate = (d: Date | string | undefined) => {
  if (!d) return '—'
  const date = d instanceof Date ? d : new Date(d as string)
  return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
}

type ActiveView = 'library' | 'expiring'

export default function DocumentsPage() {
  const [activeView, setActiveView] = useState<ActiveView>('library')
  const [page, setPage] = useState(1)
  const [tag, setTag] = useState('')
  const [contentType, setContentType] = useState('')
  const [tagInput, setTagInput] = useState('')

  const { data, isLoading, isError } = useDocuments({
    page,
    tag: tag || undefined,
    contentType: contentType || undefined,
  })

  const expiringQuery = useExpiringDocuments(90)

  const items = data?.items ?? []
  const totalPages = data?.totalPages ?? 1
  const expiringItems = expiringQuery.data?.items ?? []

  const applyTag = () => {
    setTag(tagInput.trim().toLowerCase())
    setPage(1)
  }

  const clearFilters = () => {
    setTag('')
    setTagInput('')
    setContentType('')
    setPage(1)
  }

  const hasFilters = !!tag || !!contentType

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Document Library</h1>
          <p className="page-subtitle">All documents across the CLT portfolio</p>
        </div>
      </div>

      {/* Expiring soon banner */}
      {expiringItems.length > 0 && (
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
            {expiringItems.length} document{expiringItems.length !== 1 ? 's' : ''} expiring within 90 days
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

      {activeView === 'expiring' ? (
        <ExpiringDocumentsPanel items={expiringItems} isLoading={expiringQuery.isLoading} />
      ) : (
        <>
          {/* Filters */}
          <div style={{ display: 'flex', gap: 8, alignItems: 'center', marginBottom: 16, flexWrap: 'wrap' }}>
            <div style={{ display: 'flex', gap: 4 }}>
              <input
                className="form-input"
                style={{ width: 160, padding: '6px 10px', fontSize: 13 }}
                type="text"
                placeholder="Filter by tag…"
                value={tagInput}
                onChange={e => setTagInput(e.target.value)}
                onKeyDown={e => { if (e.key === 'Enter') applyTag() }}
              />
              <button type="button" className="btn btn-ghost btn-sm" onClick={applyTag}>
                Apply
              </button>
            </div>
            <select
              className="form-select"
              style={{ width: 200, padding: '6px 10px', fontSize: 13 }}
              value={contentType}
              onChange={e => { setContentType(e.target.value); setPage(1) }}
            >
              <option value="">All file types</option>
              <option value="application/pdf">PDF</option>
              <option value="application/msword">Word (.doc)</option>
              <option value="application/vnd.openxmlformats-officedocument.wordprocessingml.document">Word (.docx)</option>
              <option value="image/jpeg">JPEG</option>
              <option value="image/png">PNG</option>
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

          {/* Table */}
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
                <div
                  key={item.id}
                  style={{
                    display: 'grid',
                    gridTemplateColumns: '1fr auto',
                    gap: 12,
                    padding: '12px 16px',
                    borderRadius: 8,
                    border: '1px solid var(--border)',
                    background: 'var(--surface-2)',
                    opacity: item.isDeleted ? 0.5 : 1,
                  }}
                >
                  <div style={{ display: 'grid', gap: 6 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 10, flexWrap: 'wrap' }}>
                      <span style={{ fontWeight: 600, color: 'var(--text-bright)', fontSize: 14 }}>
                        {item.title}
                      </span>
                      {item.sensitivity && (
                        <DocumentSensitivityBadge sensitivity={item.sensitivity} />
                      )}
                      {item.isDeleted && (
                        <span style={{
                          fontSize: 10, fontFamily: 'var(--font-mono)',
                          color: 'var(--red)', textTransform: 'uppercase', letterSpacing: '0.05em',
                        }}>
                          deleted
                        </span>
                      )}
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
                          expires {fmtDate(item.expiresAt)}
                        </span>
                      )}
                    </div>
                    {item.tags && item.tags.length > 0 && (
                      <div style={{ display: 'flex', gap: 4, flexWrap: 'wrap' }}>
                        {item.tags.map(t => (
                          <span
                            key={t}
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
                            onClick={() => { setTag(t); setTagInput(t); setPage(1) }}
                          >
                            #{t}
                          </span>
                        ))}
                      </div>
                    )}
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                    {!item.isDeleted && (
                      <button
                        type="button"
                        className="btn btn-ghost btn-sm"
                        onClick={() => downloadDocument(item.id ?? '')}
                        title="Download current version"
                      >
                        Download
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}

          {/* Pagination */}
          {totalPages > 1 && (
            <div style={{ display: 'flex', gap: 8, justifyContent: 'center', marginTop: 16 }}>
              <button
                type="button"
                className="btn btn-ghost btn-sm"
                disabled={page <= 1}
                onClick={() => setPage(p => p - 1)}
              >
                Prev
              </button>
              <span style={{ fontSize: 12, color: 'var(--text-secondary)', lineHeight: '28px' }}>
                {page} / {totalPages}
              </span>
              <button
                type="button"
                className="btn btn-ghost btn-sm"
                disabled={page >= totalPages}
                onClick={() => setPage(p => p + 1)}
              >
                Next
              </button>
            </div>
          )}
        </>
      )}
    </div>
  )
}

function isExpiringSoon(expiresAt: Date | string): boolean {
  const d = expiresAt instanceof Date ? expiresAt : new Date(expiresAt as string)
  const cutoff = new Date()
  cutoff.setDate(cutoff.getDate() + 30)
  return d <= cutoff
}

interface ExpiringProps {
  items: DocumentLibraryItemResponse[]
  isLoading: boolean
}

function ExpiringDocumentsPanel({ items = [], isLoading }: ExpiringProps) {
  if (isLoading) {
    return (
      <div className="skeleton-rows">
        {Array.from({ length: 4 }).map((_, i) => <div key={i} className="skeleton-row" />)}
      </div>
    )
  }

  if (items.length === 0) {
    return (
      <div style={{ color: 'var(--text-secondary)', fontSize: 13, padding: '24px 0', textAlign: 'center' }}>
        No documents expiring in the next 90 days.
      </div>
    )
  }

  return (
    <div style={{ display: 'grid', gap: 6 }}>
      {items.map(item => (
        <div
          key={item.id}
          style={{
            display: 'grid',
            gridTemplateColumns: '1fr auto',
            gap: 12,
            padding: '12px 16px',
            borderRadius: 8,
            border: `1px solid ${isExpiringSoon(item.expiresAt!) ? 'var(--amber)' : 'var(--border)'}`,
            background: isExpiringSoon(item.expiresAt!) ? 'var(--amber-dim)' : 'var(--surface-2)',
          }}
        >
          <div style={{ display: 'grid', gap: 4 }}>
            <span style={{ fontWeight: 600, color: 'var(--text-bright)', fontSize: 14 }}>
              {item.title}
            </span>
            <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}>
              <span style={{ color: isExpiringSoon(item.expiresAt!) ? 'var(--amber-bright)' : 'inherit', fontWeight: 500 }}>
                Expires {fmtDate(item.expiresAt ?? undefined)}
              </span>
              {' · '}
              <span style={{ fontFamily: 'var(--font-mono)' }}>{item.currentFileName}</span>
            </div>
          </div>
          <div style={{ display: 'flex', alignItems: 'center' }}>
            <button
              type="button"
              className="btn btn-ghost btn-sm"
              onClick={() => downloadDocument(item.id ?? '')}
            >
              Download
            </button>
          </div>
        </div>
      ))}
    </div>
  )
}
