import type { ReactNode } from 'react'

interface PurchaseTrackSectionProps {
  label: string
  summary: string
  complete: boolean
  expanded: boolean
  onToggle?: () => void
  expandable?: boolean
  children?: ReactNode
}

export default function PurchaseTrackSection({
  label,
  summary,
  complete,
  expanded,
  onToggle,
  expandable = true,
  children,
}: PurchaseTrackSectionProps) {
  return (
    <div
      style={{
        borderLeft: `3px solid ${complete ? 'var(--success)' : 'var(--border-bright)'}`,
        paddingLeft: 12,
        marginBottom: 16,
      }}
    >
      <div
        style={{
          display: 'flex',
          alignItems: 'baseline',
          justifyContent: 'space-between',
          gap: 12,
          marginBottom: expanded ? 10 : 0,
        }}
      >
        <div style={{ minWidth: 0, flex: 1 }}>
          <div
            style={{
              fontFamily: 'var(--font-mono)',
              fontSize: 11,
              color: complete ? 'var(--success)' : 'var(--text-secondary)',
              textTransform: 'uppercase',
              letterSpacing: '0.06em',
              marginBottom: 4,
            }}
          >
            {complete ? '✓ ' : ''}{label}
          </div>
          <div style={{ fontSize: 13, color: 'var(--text-primary)' }}>{summary}</div>
        </div>
        {expandable && onToggle && (
          <button type="button" className="btn btn-ghost btn-sm" onClick={onToggle}>
            {expanded ? 'Collapse' : 'Expand'}
          </button>
        )}
      </div>
      {expanded && children && <div style={{ marginTop: 12 }}>{children}</div>}
    </div>
  )
}
