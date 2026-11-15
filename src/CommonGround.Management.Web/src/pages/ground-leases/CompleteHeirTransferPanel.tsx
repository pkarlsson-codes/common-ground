import { useState } from 'react'
import type { HeirTransferResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useAttachDocument, useUploadDocument } from '../../hooks/documents'
import { useCompleteHeirTransfer } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { formatCalendarDate, trustToday, type CalendarDate } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

type Letter = 'agreement' | 'acknowledgement'

const LETTERS: Record<Letter, { label: string; role: string; title: string; id: string }> = {
  agreement: { label: 'Signed Letter of Agreement', role: 'heir-letter-of-agreement', title: 'Heir letter of agreement', id: 'heir-letter-of-agreement' },
  acknowledgement: { label: "Signed Attorney's Acknowledgment", role: 'heir-attorney-acknowledgement', title: 'Heir attorney acknowledgement', id: 'heir-attorney-acknowledgement' },
}

/**
 * The trust consents once the heir and the heir's lawyer have signed both letters (Art 10.3). The files upload and
 * attach to the lease first, because the API needs their IDs; a retry reuses the ones already uploaded.
 */
export default function CompleteHeirTransferPanel({ transfer, onClose }: { transfer: HeirTransferResponse; onClose: () => void }) {
  const today = trustToday()
  const [transferredOn, setTransferredOn] = useState<CalendarDate | null>(today)
  const [notes, setNotes] = useState('')
  const [files, setFiles] = useState<Partial<Record<Letter, File>>>({})
  const [uploaded, setUploaded] = useState<Map<File, string>>(new Map())
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const upload = useUploadDocument()
  const attach = useAttachDocument()
  const complete = useCompleteHeirTransfer()
  const { success } = useToast()

  const diedOn = transfer.diedOn!
  const valid = !!transferredOn && transferredOn >= diedOn && transferredOn <= today && !!files.agreement && !!files.acknowledgement
  const pending = upload.isPending || attach.isPending || complete.isPending

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!valid) return
    setProblem(null)

    const ids: Partial<Record<Letter, string>> = {}
    for (const key of ['agreement', 'acknowledgement'] as Letter[]) {
      const file = files[key]!
      let documentId = uploaded.get(file)
      if (!documentId) {
        try {
          const result = await upload.mutateAsync({
            metadata: { title: `${LETTERS[key].title} ${transfer.heirFullName} ${file.name}`, sensitivity: 'Confidential', retention: 'LongTerm7yr' },
            file,
          })
          documentId = result.documentId
          await attach.mutateAsync({
            documentId,
            req: { entityType: 'GroundLease', entityId: transfer.groundLeaseId!, role: LETTERS[key].role },
          })
          setUploaded(map => new Map(map).set(file, documentId!))
        } catch (err) {
          setProblem(await getApiProblem(err, `${file.name} could not be uploaded. The transfer was not completed.`))
          return
        }
      }
      ids[key] = documentId
    }

    try {
      await complete.mutateAsync({
        heirTransferId: transfer.id!,
        groundLeaseId: transfer.groundLeaseId!,
        request: {
          transferredOn: transferredOn!,
          letterOfAgreementDocumentId: ids.agreement,
          attorneyAcknowledgementDocumentId: ids.acknowledgement,
          notes: notes.trim() || null,
        },
      })
      success('Transfer completed')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to complete the transfer.'))
    }
  }

  return (
    <SlideOver
      title="Complete Heir Transfer"
      subtitle={transfer.deceasedHomeownerFullName ?? undefined}
      onClose={onClose}
      isDirty={notes !== '' || transferredOn !== today || !!files.agreement || !!files.acknowledgement}
      footer={
        <>
          <SlideOverCancelButton />
          <button className="btn btn-primary" type="submit" form="complete-heir-transfer-form" disabled={pending || !valid}>
            {upload.isPending ? 'Uploading…' : pending ? 'Completing…' : 'Complete Transfer'}
          </button>
        </>
      }
    >
      <form id="complete-heir-transfer-form" onSubmit={handleSubmit}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>
          {transfer.heirFullName} ({transfer.relationship === 'HouseholdMember' ? 'Household member' : transfer.relationship}) becomes the homeowner,
          replacing {transfer.deceasedHomeownerFullName}, who died on {formatCalendarDate(diedOn)}.
        </p>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label" htmlFor="heir-transferred-on">Transfer Date</label>
            <DateInput id="heir-transferred-on" className="form-input" value={transferredOn} min={diedOn} max={today} onChange={setTransferredOn} required />
          </div>
          {(Object.keys(LETTERS) as Letter[]).map(key => (
            <div className="form-field full" key={key}>
              <label className="form-label" htmlFor={`${LETTERS[key].id}-file`}>{LETTERS[key].label}</label>
              <input
                id={`${LETTERS[key].id}-file`}
                type="file"
                className="form-input"
                accept="application/pdf,image/*"
                style={{ cursor: 'pointer' }}
                required
                onChange={e => setFiles(current => ({ ...current, [key]: e.target.files?.[0] }))}
              />
              <p className="form-hint">PDF or image.</p>
            </div>
          ))}
          <div className="form-field full">
            <label className="form-label" htmlFor="heir-complete-notes">Notes (optional)</label>
            <textarea id="heir-complete-notes" className="form-textarea" value={notes} onChange={e => setNotes(e.target.value)} />
          </div>
        </div>
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
