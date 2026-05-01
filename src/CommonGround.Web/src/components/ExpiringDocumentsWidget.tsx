import { useExpiringDocuments, downloadDocument } from '../hooks/documents'

const fmtDate = (d: Date | string | undefined) => {
  if (!d) return '—'
  const date = d instanceof Date ? d : new Date(d as string)
  return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
}

function daysUntil(d: Date | string): number {
  const target = d instanceof Date ? d : new Date(d as string)
  const now = new Date()
  return Math.ceil((target.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))
}

interface Props {
  /** Number of days to look ahead. Defaults to 30. */
  daysAhead?: number
}

/** Dashboard widget showing documents that expire within the next N days. */
export default function ExpiringDocumentsWidget({ daysAhead = 30 }: Props) {
  const { data, isLoading, isError } = useExpiringDocuments(daysAhead)
  const items = data?.items ?? []

  if (isLoading) {
    return (
      <div className="detail-card">
        <div className="detail-section__title">Expiring Documents</div>
        <div className="skeleton-rows">
          {Array.from({ length: 3 }).map((_, i) => <div key={i} className="skeleton-row" />)}
        </div>
      </div>
    )
  }

  if (isError || items.length === 0) {
    return null
  }

  return (
    <div
      className="detail-card"
      style={{
        border: '1px solid var(--amber)',
        background: 'var(--amber-dim)',
      }}
    >
      <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 12 }}>
        <svg width="14" height="14" viewBox="0 0 20 20" fill="var(--amber-bright)" aria-hidden="true">
          <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
        </svg>
        <div className="detail-section__title" style={{ marginBottom: 0, color: 'var(--amber-bright)' }}>
          Expiring within {daysAhead} days
        </div>
      </div>
      <div style={{ display: 'grid', gap: 6 }}>
        {items.slice(0, 5).map(item => {
          const days = item.expiresAt ? daysUntil(item.expiresAt) : null
          const urgent = days !== null && days <= 7

          return (
            <div
              key={item.id}
              style={{
                display: 'grid',
                gridTemplateColumns: '1fr auto',
                gap: 8,
                padding: '8px 10px',
                borderRadius: 6,
                background: urgent ? 'var(--urgent-surface)' : 'var(--surface-2)',
                border: `1px solid ${urgent ? 'var(--red)' : 'var(--border)'}`,
              }}
            >
              <div>
                <div style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-bright)' }}>
                  {item.title}
                </div>
                <div style={{ fontSize: 11, color: urgent ? 'var(--red)' : 'var(--amber-bright)', marginTop: 2 }}>
                  {days !== null
                    ? days <= 0
                      ? 'Expired'
                      : days === 1
                        ? 'Expires tomorrow'
                        : `Expires in ${days} days · ${fmtDate(item.expiresAt ?? undefined)}`
                    : fmtDate(item.expiresAt ?? undefined)}
                </div>
              </div>
              <button
                type="button"
                className="btn btn-ghost btn-sm"
                style={{ fontSize: 11 }}
                onClick={() => downloadDocument(item.id ?? '')}
                title="Download"
              >
                Download
              </button>
            </div>
          )
        })}
        {items.length > 5 && (
          <div style={{ fontSize: 12, color: 'var(--text-secondary)', textAlign: 'center', paddingTop: 4 }}>
            +{items.length - 5} more — see Documents library
          </div>
        )}
      </div>
    </div>
  )
}
