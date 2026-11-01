import { useState, type ReactNode } from 'react'

export interface NextStepAction {
  label: string
  onClick: () => void
  disabled?: boolean
}

interface Props {
  title: string
  body?: ReactNode
  primary?: NextStepAction
  moreActions?: NextStepAction[]
  children?: ReactNode
}

/** What the record is waiting on, with one primary action and the rest under "More actions". */
export default function NextStepCard({ title, body, primary, moreActions = [], children }: Props) {
  const [moreOpen, setMoreOpen] = useState(false)

  return (
    <section className="next-step-card" aria-label="Next step">
      <div className="next-step-card__kicker">Next step</div>
      <p className="next-step-card__title">{title}</p>
      {body && <div className="next-step-card__body">{body}</div>}
      {(primary || moreActions.length > 0) && (
        <div className="next-step-card__actions">
          {primary && (
            <button type="button" className="btn btn-primary btn-sm" disabled={primary.disabled} onClick={primary.onClick}>
              {primary.label}
            </button>
          )}
          {moreActions.length > 0 && (
            <div className="next-step-card__more">
              <button
                type="button"
                className="btn btn-ghost btn-sm"
                aria-expanded={moreOpen}
                aria-haspopup="menu"
                onClick={() => setMoreOpen(o => !o)}
              >
                More actions ▾
              </button>
              {moreOpen && (
                <div className="next-step-card__menu" role="menu">
                  {moreActions.map(action => (
                    <button
                      key={action.label}
                      type="button"
                      role="menuitem"
                      className="next-step-card__menu-item"
                      disabled={action.disabled}
                      onClick={() => { setMoreOpen(false); action.onClick() }}
                    >
                      {action.label}
                    </button>
                  ))}
                </div>
              )}
            </div>
          )}
        </div>
      )}
      {children}
    </section>
  )
}
