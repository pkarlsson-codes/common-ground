import { useEffect, type ReactNode } from 'react'

interface Props {
  title: string
  subtitle?: string
  onClose: () => void
  footer?: ReactNode
  children: ReactNode
}

export default function SlideOver({ title, subtitle, onClose, footer, children }: Props) {
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose() }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [onClose])

  return (
    <div className="slideover-overlay" role="dialog" aria-modal="true">
      <div className="slideover-backdrop" onClick={onClose} />
      <div className="slideover-panel">
        <div className="slideover-header">
          <div style={{ minWidth: 0 }}>
            <div className="slideover-header__title">{title}</div>
            {subtitle && <div className="slideover-header__subtitle">{subtitle}</div>}
          </div>
          <button className="slideover-close" onClick={onClose} aria-label="Close">
            <svg width="13" height="13" viewBox="0 0 13 13" fill="none">
              <path d="M1 1l11 11M12 1L1 12" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round"/>
            </svg>
          </button>
        </div>
        <div className="slideover-body">{children}</div>
        {footer && <div className="slideover-footer">{footer}</div>}
      </div>
    </div>
  )
}
