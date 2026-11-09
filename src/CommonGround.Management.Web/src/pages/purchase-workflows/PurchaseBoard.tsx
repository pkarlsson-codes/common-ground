import type { PurchaseBoardCardResponse } from '../../api'
import { usePurchaseBoard, PURCHASE_STEP_LABELS } from '../../hooks/purchaseWorkflows'
import { daysBetween, formatCalendarDate, type CalendarDate } from '../../lib/calendarDate'

const plural = (n: number, word: string) => `${n.toLocaleString()} ${word}${n === 1 ? '' : 's'}`

function Card({ card, today, selected, onOpen }: {
  card: PurchaseBoardCardResponse
  today: CalendarDate
  selected: boolean
  onOpen: (id: string) => void
}) {
  const days = card.stepEnteredOn ? daysBetween(card.stepEnteredOn, today) : 0
  return (
    <li>
      <button
        type="button"
        className={`board-card${selected ? ' board-card--selected' : ''}`}
        onClick={() => onOpen(card.id!)}
      >
        <span className="board-card__title">{card.homeLabel ?? 'Home'}</span>
        <span className="board-card__buyer">{card.buyerFullName}</span>
        <span className="board-card__meta">
          <span>{days > 0 ? `${plural(days, 'day')} on this step` : 'Since today'}</span>
          {card.scheduledClosingDate && <span>Closing {formatCalendarDate(card.scheduledClosingDate)}</span>}
          {card.phase === 'Stalled' && <span className="board-card__flag">Stalled</span>}
        </span>
      </button>
    </li>
  )
}

/** Open purchases in one column per step they wait on, longest waiting first. */
export default function PurchaseBoard({ selectedId, onOpen }: { selectedId: string | null; onOpen: (id: string) => void }) {
  const { data, isLoading, isError } = usePurchaseBoard()

  if (isLoading) {
    return (
      <div className="skeleton-rows">
        {Array.from({ length: 4 }).map((_, i) => <div key={i} className="skeleton-row" />)}
      </div>
    )
  }

  if (isError || !data?.today) {
    return (
      <div className="error-state">
        <div className="error-state__title">Could not load the purchase board</div>
        <div className="error-state__sub">Try again in a moment. If it keeps failing, contact support.</div>
      </div>
    )
  }

  return (
    <div className="board" aria-label="Purchases by step">
      {(data.columns ?? []).map(column => {
        const label = PURCHASE_STEP_LABELS[column.step ?? ''] ?? column.step
        const cards = column.cards ?? []
        return (
          <section key={column.step} className="board-column" aria-labelledby={`board-${column.step}`}>
            <h2 id={`board-${column.step}`} className="board-column__title">
              {label} <span className="board-column__count">{cards.length}</span>
            </h2>
            {cards.length === 0
              ? <p className="board-column__empty">Nothing waiting</p>
              : (
                <ul className="board-column__cards">
                  {cards.map(card => (
                    <Card key={card.id} card={card} today={data.today!} selected={card.id === selectedId} onOpen={onOpen} />
                  ))}
                </ul>
              )}
          </section>
        )
      })}
    </div>
  )
}
