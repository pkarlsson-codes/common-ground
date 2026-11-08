import { useState, type ReactNode } from 'react'
import ConfirmActionModal from './ConfirmActionModal'
import { SlideOverCloseContext } from './slideOverClose'
import { useOverlayEscape } from '../lib/overlayStack'

interface Props {
  title: string
  subtitle?: string
  onClose: () => void
  footer?: ReactNode
  /** When true, closing asks "Discard changes?" first. */
  isDirty?: boolean
  children: ReactNode
}

export default function SlideOver({ title, subtitle, onClose, footer, isDirty = false, children }: Props) {
  const [confirmingDiscard, setConfirmingDiscard] = useState(false)
  const requestClose = () => (isDirty ? setConfirmingDiscard(true) : onClose())
  useOverlayEscape(true, requestClose)

  return (
    <div className="slideover-overlay" role="dialog" aria-modal="true">
      <div className="slideover-backdrop" onClick={requestClose} />
      <div className="slideover-panel">
        <div className="slideover-header">
          <div style={{ minWidth: 0 }}>
            <div className="slideover-header__title">{title}</div>
            {subtitle && <div className="slideover-header__subtitle">{subtitle}</div>}
          </div>
          <button className="slideover-close" onClick={requestClose} aria-label="Close">
            <svg width="13" height="13" viewBox="0 0 13 13" fill="none">
              <path d="M1 1l11 11M12 1L1 12" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round"/>
            </svg>
          </button>
        </div>
        <div className="slideover-body">{children}</div>
        {footer && (
          <SlideOverCloseContext.Provider value={requestClose}>
            <div className="slideover-footer">{footer}</div>
          </SlideOverCloseContext.Provider>
        )}
      </div>
      <ConfirmActionModal
        open={confirmingDiscard}
        title="Discard changes?"
        message="What you typed will be lost."
        confirmLabel="Discard"
        confirmTone="danger"
        onConfirm={onClose}
        onCancel={() => setConfirmingDiscard(false)}
      />
    </div>
  )
}
