import { useEffect } from 'react'

interface ConfirmActionModalProps {
  open: boolean
  title: string
  message: string
  confirmLabel: string
  confirmTone?: 'danger' | 'default'
  onConfirm: () => void
  onCancel: () => void
}

export default function ConfirmActionModal({
  open,
  title,
  message,
  confirmLabel,
  confirmTone = 'default',
  onConfirm,
  onCancel,
}: ConfirmActionModalProps) {
  useEffect(() => {
    if (!open) return
    const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape') onCancel() }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [open, onCancel])

  if (!open) return null

  return (
    <div
      role="dialog"
      aria-modal="true"
      style={{
        position: 'fixed',
        inset: 0,
        zIndex: 60,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
      }}
    >
      <div
        onClick={onCancel}
        style={{
          position: 'absolute',
          inset: 0,
          background: 'var(--overlay-heavy)',
          backdropFilter: 'blur(3px)',
        }}
      />
      <div
        style={{
          position: 'relative',
          zIndex: 1,
          width: 420,
          maxWidth: '90vw',
          background: 'var(--surface)',
          border: '1px solid var(--border)',
          borderRadius: 12,
          padding: 24,
          boxShadow: 'var(--shadow-overlay-xl)',
        }}
      >
        <div
          style={{
            fontFamily: 'var(--font-display)',
            fontSize: 20,
            fontWeight: 600,
            color: 'var(--text-bright)',
            marginBottom: 10,
          }}
        >
          {title}
        </div>
        <div
          style={{
            fontSize: 13.5,
            color: 'var(--text-secondary)',
            lineHeight: 1.5,
            marginBottom: 20,
          }}
        >
          {message}
        </div>
        <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end' }}>
          <button type="button" className="btn btn-ghost btn-sm" onClick={onCancel}>
            Cancel
          </button>
          <button
            type="button"
            className="btn btn-secondary btn-sm"
            style={
              confirmTone === 'danger'
                ? { color: 'var(--red)', borderColor: 'var(--danger-surface)' }
                : undefined
            }
            onClick={onConfirm}
          >
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  )
}
