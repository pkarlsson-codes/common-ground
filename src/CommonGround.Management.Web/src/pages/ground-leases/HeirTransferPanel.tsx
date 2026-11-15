import type { ReactNode } from 'react'
import SlideOver from '../../components/SlideOver'
import { HEIR_TRANSFER_STATUS_LABELS, useHeirTransfer } from '../../hooks/groundLeases'
import { useHome } from '../../hooks/homes'
import { formatCalendarDate, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'

function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="detail-field">
      <div className="detail-field__label">{label}</div>
      <div className="detail-field__value">{children}</div>
    </div>
  )
}

const date = (d: CalendarDate | null | undefined) => (d ? <span className="cell-mono">{formatCalendarDate(d)}</span> : '—')

/** What the trust was told of the homeowner's death, the heir it recorded, and how the transfer ended. */
export default function HeirTransferPanel({ heirTransferId, onClose }: { heirTransferId: string; onClose: () => void }) {
  const { data: transfer, isLoading, isError } = useHeirTransfer(heirTransferId)
  const { data: home } = useHome(transfer?.homeId ?? null)
  const status = transfer?.status ?? ''

  return (
    <SlideOver
      title={home?.label ?? 'Heir Transfer'}
      subtitle={transfer ? `${transfer.deceasedHomeownerFullName ?? 'Homeowner'} · ${HEIR_TRANSFER_STATUS_LABELS[status] ?? status}` : undefined}
      onClose={onClose}
    >
      {isLoading && <div className="skeleton-rows"><div className="skeleton-row" /></div>}
      {isError && <div className="form-error" role="alert">Could not load the heir transfer.</div>}
      {transfer && (
        <>
          <div className="detail-section">
            <div className="detail-section__title">Notice</div>
            <div className="detail-grid">
              <Field label="Date of Death">{date(transfer.diedOn)}</Field>
              <Field label="Notice Received On">
                {date(transfer.noticeReceivedOn)}
                {transfer.noticeWasLate && <div style={{ fontSize: 12, color: 'var(--danger)' }}>Notice was late</div>}
              </Field>
              <Field label="Notified By">{transfer.notifiedBy}</Field>
              {transfer.notifiedByContact && <Field label="Contact">{transfer.notifiedByContact}</Field>}
              {transfer.notes && <Field label="Notes">{transfer.notes}</Field>}
            </div>
          </div>

          {transfer.heirFullName && (
            <div className="detail-section">
              <div className="detail-section__title">Heir</div>
              <div className="detail-grid">
                <Field label="Name">{transfer.heirFullName}</Field>
                <Field label="Email">{transfer.heirEmail}</Field>
                {transfer.heirPhone && <Field label="Phone">{transfer.heirPhone}</Field>}
                <Field label="Relationship">
                  {transfer.relationship === 'HouseholdMember'
                    ? <>Household member since {date(transfer.heirMovedInOn)}</>
                    : transfer.relationship}
                </Field>
                {transfer.heirVerifiedAnnualIncome != null && (
                  <>
                    <Field label="Verified Income"><span className="cell-mono">{formatPrice(transfer.heirVerifiedAnnualIncome)}</span></Field>
                    <Field label="Household Size">{transfer.heirVerifiedHouseholdSize}</Field>
                    <Field label="Home's Limit">
                      {transfer.incomeLimit == null ? '—' : <span className="cell-mono">{formatPrice(transfer.incomeLimit)}</span>}
                    </Field>
                  </>
                )}
              </div>
            </div>
          )}

          {transfer.transferredOn && (
            <div className="detail-section">
              <div className="detail-section__title">Transfer</div>
              <div className="detail-grid">
                <Field label="Transfer Date">{date(transfer.transferredOn)}</Field>
                {transfer.completionNotes && <Field label="Notes">{transfer.completionNotes}</Field>}
              </div>
            </div>
          )}

          {status === 'Closed' && (
            <div className="detail-section">
              <div className="detail-section__title">Closed</div>
              <div className="detail-grid">
                <Field label="Closed On">{transfer.closedAt ? <span className="cell-mono">{transfer.closedAt.toISOString().slice(0, 10)}</span> : '—'}</Field>
                <Field label="Reason">{transfer.closedReason}</Field>
                {transfer.closedNotes && <Field label="Notes">{transfer.closedNotes}</Field>}
              </div>
            </div>
          )}
        </>
      )}
    </SlideOver>
  )
}
