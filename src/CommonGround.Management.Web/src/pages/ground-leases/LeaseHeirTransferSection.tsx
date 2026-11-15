import { useState } from 'react'
import type { GroundLeaseResponse } from '../../api'
import { HEIR_TRANSFER_STATUS_LABELS, useHeirTransfers } from '../../hooks/groundLeases'
import { formatCalendarDate } from '../../lib/calendarDate'
import HeirTransferPanel from './HeirTransferPanel'
import OpenHeirTransferPanel from './OpenHeirTransferPanel'

/** The lease's transfers to an heir after the homeowner's death (Art 10.3), newest first. */
export default function LeaseHeirTransferSection({ lease }: { lease: GroundLeaseResponse }) {
  const [opening, setOpening] = useState(false)
  const [openTransferId, setOpenTransferId] = useState<string | null>(null)
  const { data: transfers } = useHeirTransfers(lease.id ?? null)
  const inProgress = (transfers ?? []).some(t => t.status === 'Open' || t.status === 'Heir recorded')

  return (
    <div className="detail-section">
      <div className="detail-section__title">Heir Transfer</div>

      {lease.status === 'Active' && !inProgress && (
        <div style={{ marginBottom: 12 }}>
          <button type="button" className="btn btn-secondary" onClick={() => setOpening(true)}>Open Heir Transfer</button>
        </div>
      )}

      {transfers && transfers.length > 0 ? (
        <div className="registry-table-wrap">
          <table className="registry-table" aria-label="Heir transfers">
            <thead>
              <tr>
                <th>Date of Death</th>
                <th>Heir</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {[...transfers]
                .sort((a, b) => (b.diedOn ?? '').localeCompare(a.diedOn ?? ''))
                .map(t => (
                  <tr
                    key={t.id}
                    tabIndex={0}
                    onClick={() => setOpenTransferId(t.id!)}
                    onKeyDown={e => { if (e.key === 'Enter') setOpenTransferId(t.id!) }}
                  >
                    <td className="cell-mono">{t.diedOn ? formatCalendarDate(t.diedOn) : '—'}</td>
                    <td>{t.heirFullName ?? '—'}</td>
                    <td>
                      {HEIR_TRANSFER_STATUS_LABELS[t.status ?? ''] ?? t.status}
                      {t.noticeWasLate && <div style={{ fontSize: 11, color: 'var(--danger)' }}>Notice was late</div>}
                    </td>
                  </tr>
                ))}
            </tbody>
          </table>
        </div>
      ) : (
        <p style={{ fontSize: 13, color: 'var(--text-dim)' }}>No heir transfers.</p>
      )}

      {(lease.previousHomeowners ?? []).length > 0 && (
        <div style={{ marginTop: 12 }}>
          <div className="detail-field__label">Previous Homeowners</div>
          <ul aria-label="Previous homeowners" style={{ margin: '4px 0 0', paddingLeft: 18, fontSize: 13 }}>
            {[...(lease.previousHomeowners ?? [])]
              .sort((a, b) => (b.transferredOn ?? '').localeCompare(a.transferredOn ?? ''))
              .map(p => (
                <li key={p.heirTransferId}>
                  {p.fullName}, until {p.transferredOn ? formatCalendarDate(p.transferredOn) : '—'}
                </li>
              ))}
          </ul>
        </div>
      )}

      {opening && (
        <OpenHeirTransferPanel
          lease={lease}
          onClose={() => setOpening(false)}
          onOpened={id => {
            setOpening(false)
            setOpenTransferId(id)
          }}
        />
      )}
      {openTransferId && <HeirTransferPanel heirTransferId={openTransferId} onClose={() => setOpenTransferId(null)} />}
    </div>
  )
}
