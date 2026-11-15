import { useState } from 'react'
import type { ImprovementRequestResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useAttachDocument, useUploadDocument } from '../../hooks/documents'
import { useRecordImprovementCompleted } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

interface Props {
  request: ImprovementRequestResponse
  onClose: () => void
}

/**
 * Records finished work with its receipts and the credit that counts at resale. Receipts upload first; a retry
 * after a failed record reuses them. They attach to the request once it is completed.
 */
export default function RecordImprovementCompletedPanel({ request, onClose }: Props) {
  const today = trustToday()
  const agreed = request.preApprovedCredit ?? 0
  const [completedOn, setCompletedOn] = useState<CalendarDate | null>(today)
  const [actualCost, setActualCost] = useState('')
  const [creditText, setCreditText] = useState<string | null>(null)
  const [notes, setNotes] = useState('')
  const [files, setFiles] = useState<File[]>([])
  const [dragOver, setDragOver] = useState(false)
  const [uploaded, setUploaded] = useState<Map<File, string>>(new Map())
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const upload = useUploadDocument()
  const attach = useAttachDocument()
  const record = useRecordImprovementCompleted()
  const { success, warning } = useToast()

  const cost = parseFloat(actualCost)
  // Until staff type a credit, it follows the cost: the lesser of what the work cost and what was agreed.
  const defaultCredit = Number.isNaN(cost) ? '' : String(Math.min(cost, agreed))
  const creditValue = creditText ?? defaultCredit
  const credit = parseFloat(creditValue)
  const approvedOn = request.approvedOn ?? undefined
  const canSubmit = !!completedOn && completedOn <= today && (!approvedOn || completedOn >= approvedOn)
    && cost > 0 && files.length > 0 && !Number.isNaN(credit) && credit >= 0 && credit <= agreed
  const pending = upload.isPending || record.isPending || attach.isPending
  const addFiles = (added: FileList | null) => {
    if (added && added.length > 0) setFiles(current => [...current, ...Array.from(added)])
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!canSubmit) return
    setProblem(null)
    const documentIds: string[] = []
    for (const file of files) {
      let documentId = uploaded.get(file)
      if (!documentId) {
        try {
          const result = await upload.mutateAsync({
            metadata: {
              title: `Improvement receipt ${(request.description ?? '').slice(0, 60)} ${file.name}`,
              sensitivity: 'Internal',
              retention: 'LongTerm7yr',
            },
            file,
          })
          documentId = result.documentId
          setUploaded(map => new Map(map).set(file, documentId!))
        } catch (err) {
          setProblem(await getApiProblem(err, `${file.name} could not be uploaded. Nothing was recorded.`))
          return
        }
      }
      documentIds.push(documentId)
    }

    try {
      await record.mutateAsync({
        request,
        completedOn: completedOn!,
        actualCost: cost,
        receiptDocumentIds: documentIds,
        confirmedCredit: credit,
        notes,
      })
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to record the completed work.'))
      return
    }

    let unattached = 0
    for (const documentId of documentIds) {
      try {
        await attach.mutateAsync({
          documentId,
          req: { entityType: 'ImprovementRequest', entityId: request.id!, role: 'improvement-receipt' },
        })
      } catch {
        unattached++
      }
    }
    success('Improvement recorded')
    if (unattached > 0) {
      warning(`${unattached} receipt ${unattached === 1 ? 'file is' : 'files are'} in Documents but not attached to the request.`)
    }
    onClose()
  }

  return (
    <SlideOver
      title="Record Improvement Completed"
      subtitle={request.description}
      onClose={onClose}
      isDirty={actualCost !== '' || files.length > 0 || notes !== '' || creditText !== null || completedOn !== today}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="record-improvement-completed-form"
            disabled={pending || !canSubmit}
          >
            {upload.isPending ? 'Uploading…' : pending ? 'Recording…' : 'Record Completed'}
          </button>
        </>
      }
    >
      <form id="record-improvement-completed-form" onSubmit={handleSubmit}>
        <div className="detail-grid" style={{ marginBottom: 14 }}>
          <div className="detail-field">
            <div className="detail-field__label">Estimated Cost</div>
            <div className="detail-field__value mono">{formatPrice(request.estimatedCost ?? 0)}</div>
          </div>
          <div className="detail-field">
            <div className="detail-field__label">Pre-Approved Credit</div>
            <div className="detail-field__value mono">{formatPrice(agreed)}</div>
          </div>
        </div>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="improvement-completed-on">Completed On</label>
            <DateInput
              id="improvement-completed-on"
              className="form-input"
              value={completedOn}
              min={request.approvedOn}
              max={today}
              onChange={setCompletedOn}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="improvement-actual-cost">Actual Cost ($)</label>
            <input
              id="improvement-actual-cost"
              className="form-input"
              type="number"
              min="0.01"
              step="0.01"
              value={actualCost}
              onChange={e => setActualCost(e.target.value)}
              required
            />
          </div>
          <div
            className="form-field full"
            style={{
              border: `1px dashed ${dragOver ? 'var(--amber)' : 'var(--border-bright)'}`,
              borderRadius: 8,
              padding: 12,
              background: dragOver ? 'var(--amber-dim)' : undefined,
            }}
            onDragOver={e => { e.preventDefault(); setDragOver(true) }}
            onDragLeave={() => setDragOver(false)}
            onDrop={e => {
              e.preventDefault()
              setDragOver(false)
              addFiles(e.dataTransfer.files)
            }}
          >
            <label className="form-label" htmlFor="improvement-receipts">Receipts</label>
            <input
              id="improvement-receipts"
              type="file"
              multiple
              className="form-input"
              accept="application/pdf,image/*"
              style={{ cursor: 'pointer' }}
              value=""
              onChange={e => addFiles(e.target.files)}
            />
            {files.length === 0 ? (
              <p className="form-hint">PDF or image, at least one. Or drop the files here.</p>
            ) : (
              <>
                <ul style={{ margin: '8px 0 0', paddingLeft: 18, fontSize: 13 }} aria-label="Receipts to attach">
                  {files.map((file, i) => (
                    <li key={`${file.name}-${i}`}>
                      {file.name}{' '}
                      <button
                        type="button"
                        className="btn btn-ghost btn-sm"
                        aria-label={`Remove ${file.name}`}
                        onClick={() => setFiles(current => current.filter(f => f !== file))}
                      >
                        Remove
                      </button>
                    </li>
                  ))}
                </ul>
                <p className="form-hint">These go to Documents, attached to the request.</p>
              </>
            )}
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="improvement-confirmed-credit">Confirmed Credit ($)</label>
            <input
              id="improvement-confirmed-credit"
              className="form-input"
              type="number"
              min="0"
              max={agreed}
              step="0.01"
              value={creditValue}
              onChange={e => setCreditText(e.target.value)}
              required
            />
            <p className="form-hint">The credit can't be more than the {formatPrice(agreed)} agreed before the work.</p>
          </div>
          <div className="form-field full">
            <label className="form-label" htmlFor="improvement-completion-notes">Notes (optional)</label>
            <textarea
              id="improvement-completion-notes"
              className="form-textarea"
              value={notes}
              onChange={e => setNotes(e.target.value)}
            />
          </div>
        </div>
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
