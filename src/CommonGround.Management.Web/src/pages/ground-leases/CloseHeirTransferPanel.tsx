import { useState } from 'react'
import type { HeirTransferResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useCloseHeirTransfer } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

const REASONS = [
  { value: 'HeirNotQualified', label: 'Heir not qualified' },
  { value: 'HeirDeclined', label: 'Heir declined' },
  { value: 'Other', label: 'Other' },
]

/** Ends a transfer without the home passing to the heir. The lease is unchanged; the home is then resold. */
export default function CloseHeirTransferPanel({ transfer, onClose }: { transfer: HeirTransferResponse; onClose: () => void }) {
  const [reason, setReason] = useState('HeirNotQualified')
  const [notes, setNotes] = useState('')
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useCloseHeirTransfer()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)
    try {
      await mutation.mutateAsync({
        heirTransferId: transfer.id!,
        groundLeaseId: transfer.groundLeaseId!,
        request: { reason, notes: notes.trim() || null },
      })
      success('Transfer closed')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to close the transfer.'))
    }
  }

  return (
    <SlideOver
      title="Close Heir Transfer"
      subtitle={transfer.deceasedHomeownerFullName ?? undefined}
      onClose={onClose}
      isDirty={notes !== '' || reason !== 'HeirNotQualified'}
      footer={
        <>
          <SlideOverCancelButton />
          <button className="btn btn-primary" type="submit" form="close-heir-transfer-form" disabled={mutation.isPending}>
            {mutation.isPending ? 'Closing…' : 'Close Transfer'}
          </button>
        </>
      }
    >
      <form id="close-heir-transfer-form" onSubmit={handleSubmit}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>
          The lease is unchanged. The home is then sold under the resale terms.
        </p>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label" htmlFor="heir-close-reason">Reason</label>
            <select id="heir-close-reason" className="form-select" value={reason} onChange={e => setReason(e.target.value)}>
              {REASONS.map(r => <option key={r.value} value={r.value}>{r.label}</option>)}
            </select>
          </div>
          <div className="form-field full">
            <label className="form-label" htmlFor="heir-close-notes">Notes (optional)</label>
            <textarea id="heir-close-notes" className="form-textarea" value={notes} onChange={e => setNotes(e.target.value)} />
          </div>
        </div>
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
