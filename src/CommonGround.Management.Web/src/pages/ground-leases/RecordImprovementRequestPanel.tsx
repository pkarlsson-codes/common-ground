import { useState } from 'react'
import type { GroundLeaseResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useAttachDocument, useUploadDocument } from '../../hooks/documents'
import { useReceiveImprovementRequest } from '../../hooks/groundLeases'
import { useHome } from '../../hooks/homes'
import { useToast } from '../../hooks/useToast'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

interface Props {
  lease: GroundLeaseResponse
  onClose: () => void
  /** Called with the new request's ID once it is recorded. */
  onRecorded: (improvementRequestId: string) => void
}

/**
 * Records a homeowner's request to alter or add to the home (Art 7.3), with the plans or quotes they sent. The
 * plans upload first and attach to the request once it exists; a retry after a failed record reuses them.
 */
export default function RecordImprovementRequestPanel({ lease, onClose, onRecorded }: Props) {
  const today = trustToday()
  const { data: home } = useHome(lease.homeId ?? null)
  const [receivedOn, setReceivedOn] = useState<CalendarDate | null>(today)
  const [description, setDescription] = useState('')
  const [estimatedCost, setEstimatedCost] = useState('')
  const [files, setFiles] = useState<File[]>([])
  const [dragOver, setDragOver] = useState(false)
  // Plans uploaded on an attempt whose record then failed, by file; reused on retry so Documents gets no duplicate.
  const [uploaded, setUploaded] = useState<Map<File, string>>(new Map())
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const upload = useUploadDocument()
  const attach = useAttachDocument()
  const record = useReceiveImprovementRequest()
  const { success, warning } = useToast()

  const cost = parseFloat(estimatedCost)
  const canSubmit = !!receivedOn && receivedOn <= today && description.trim() !== '' && cost > 0
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
              title: `Improvement plans ${description.trim().slice(0, 60)} ${file.name}`,
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

    let requestId: string
    try {
      const result = await record.mutateAsync({
        groundLeaseId: lease.id!,
        receivedOn: receivedOn!,
        description,
        estimatedCost: cost,
      })
      requestId = result.improvementRequestId!
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to record the request.'))
      return
    }

    let unattached = 0
    for (const documentId of documentIds) {
      try {
        await attach.mutateAsync({
          documentId,
          req: { entityType: 'ImprovementRequest', entityId: requestId, role: 'improvement-plans' },
        })
      } catch {
        unattached++
      }
    }
    success('Improvement request recorded')
    if (unattached > 0) {
      warning(`${unattached} plans ${unattached === 1 ? 'file is' : 'files are'} in Documents but not attached to the request.`)
    }
    onRecorded(requestId)
  }

  return (
    <SlideOver
      title="Record Improvement Request"
      subtitle={[lease.homeownerFullName, home?.label].filter(Boolean).join(' · ') || 'Ground lease'}
      onClose={onClose}
      isDirty={description !== '' || estimatedCost !== '' || files.length > 0 || receivedOn !== today}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="record-improvement-request-form"
            disabled={pending || !canSubmit}
          >
            {upload.isPending ? 'Uploading…' : pending ? 'Recording…' : 'Record Request'}
          </button>
        </>
      }
    >
      <form id="record-improvement-request-form" onSubmit={handleSubmit}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>
          The lease asks the trust to reply in writing within two weeks of receiving the request.
        </p>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="improvement-received-on">Received On</label>
            <DateInput id="improvement-received-on" className="form-input" value={receivedOn} max={today} onChange={setReceivedOn} required />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="improvement-estimated-cost">Estimated Cost ($)</label>
            <input
              id="improvement-estimated-cost"
              className="form-input"
              type="number"
              min="0.01"
              step="0.01"
              value={estimatedCost}
              onChange={e => setEstimatedCost(e.target.value)}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label" htmlFor="improvement-description">Description</label>
            <textarea
              id="improvement-description"
              className="form-textarea"
              value={description}
              onChange={e => setDescription(e.target.value)}
              placeholder="Replace the roof with standing-seam metal."
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
            <label className="form-label" htmlFor="improvement-plans">Plans or Quotes (optional)</label>
            <input
              id="improvement-plans"
              type="file"
              multiple
              className="form-input"
              accept="application/pdf,image/*"
              style={{ cursor: 'pointer' }}
              value=""
              onChange={e => addFiles(e.target.files)}
            />
            {files.length === 0 ? (
              <p className="form-hint">PDF or image, several allowed. Or drop the files here.</p>
            ) : (
              <>
                <ul style={{ margin: '8px 0 0', paddingLeft: 18, fontSize: 13 }} aria-label="Plans to attach">
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
        </div>
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
