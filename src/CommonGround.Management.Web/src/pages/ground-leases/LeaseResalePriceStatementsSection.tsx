import { useState } from 'react'
import type { GroundLeaseResponse } from '../../api'
import { NOTICE_DELIVERY_LABELS } from '../../hooks/groundLeases'
import { formatCalendarDate, trustToday } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import PrepareResalePriceStatementPanel from './PrepareResalePriceStatementPanel'
import { statementFiguresText } from './resalePriceStatementLetter'

/**
 * The lease's yearly resale price statements (Ch 23, Disclosure): when the next is due, the way to prepare one, and
 * the statements sent, newest first.
 */
export default function LeaseResalePriceStatementsSection({ lease }: { lease: GroundLeaseResponse }) {
  const [preparing, setPreparing] = useState(false)
  const statements = lease.resalePriceStatements
  if (!statements) return null
  const active = lease.status === 'Active'
  const sent = statements.statements ?? []
  const nextDue = statements.nextDueOn ?? null
  const overdue = !!nextDue && nextDue <= trustToday()

  return (
    <div className="detail-section">
      <div className="detail-section__title">Resale Price Statement</div>
      {active && (
        <div className="detail-grid" style={{ marginBottom: 12 }}>
          <div className="detail-field">
            <div className="detail-field__label">Next Due</div>
            <div className="detail-field__value" style={overdue ? { color: 'var(--red)' } : undefined}>
              {statements.resaleInProgress
                ? 'Not due while a resale is in progress'
                : nextDue ? <span className="mono">{formatCalendarDate(nextDue)}</span> : '—'}
            </div>
          </div>
        </div>
      )}

      {active && (
        <div style={{ marginBottom: 12 }}>
          <button type="button" className="btn btn-secondary" onClick={() => setPreparing(true)}>
            Prepare Statement
          </button>
        </div>
      )}

      {sent.length === 0 ? (
        <p style={{ fontSize: 13, color: 'var(--text-dim)' }}>No statements sent</p>
      ) : (
        <div className="registry-table-wrap">
          <table className="registry-table" aria-label="Resale price statements">
            <thead>
              <tr>
                <th>Date</th>
                <th>Price</th>
                <th>Delivery</th>
                <th>Figures</th>
              </tr>
            </thead>
            <tbody>
              {sent.map(s => (
                <tr key={`${s.statementDate}-${s.recordedAt?.toISOString()}`}>
                  <td className="cell-mono">{s.statementDate ? formatCalendarDate(s.statementDate) : '—'}</td>
                  <td className="cell-mono">{formatPrice(s.price ?? 0)}</td>
                  <td>{NOTICE_DELIVERY_LABELS[s.delivery ?? ''] ?? s.delivery}</td>
                  <td style={{ fontSize: 12 }}>{statementFiguresText(lease, s)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {preparing && <PrepareResalePriceStatementPanel lease={lease} onClose={() => setPreparing(false)} />}
    </div>
  )
}
