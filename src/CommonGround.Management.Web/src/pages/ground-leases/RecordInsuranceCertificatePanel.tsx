import { useState } from 'react'
import type { GroundLeaseResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useUploadDocument } from '../../hooks/documents'
import { useRecordInsuranceCertificate } from '../../hooks/groundLeases'
import { useHome } from '../../hooks/homes'
import { useToast } from '../../hooks/useToast'
import { addDays, parseCalendarDate, trustToday, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { checkCertificate, requirementsOn } from './insuranceCheck'

interface Props {
  lease: GroundLeaseResponse
  onClose: () => void
}

/** The same date next year, as a one-year certificate prints its period; Feb 29 becomes Feb 28. */
const oneYearOn = (start: CalendarDate): CalendarDate => {
  const monthDay = start.slice(4) === '-02-29' ? '-02-28' : start.slice(4)
  return parseCalendarDate(`${Number(start.slice(0, 4)) + 1}${monthDay}`)
}

const parseAmount = (value: string) => {
  const parsed = parseFloat(value)
  return Number.isNaN(parsed) ? null : parsed
}

/**
 * Records a certificate of the homeowner's insurance (Art 9.4), with the file, and shows as it is typed how the
 * certificate compares with the lease. One that falls short can still be recorded: it is what the homeowner sent.
 */
export default function RecordInsuranceCertificatePanel({ lease, onClose }: Props) {
  const today = trustToday()
  const { data: home } = useHome(lease.homeId ?? null)
  const [insurer, setInsurer] = useState('')
  const [policyNumber, setPolicyNumber] = useState('')
  const [policyStart, setPolicyStart] = useState<CalendarDate | null>(null)
  const [policyEnd, setPolicyEnd] = useState<CalendarDate | null>(null)
  const [policyEndTyped, setPolicyEndTyped] = useState(false)
  const [dwelling, setDwelling] = useState('')
  const [liability, setLiability] = useState('')
  const [trustNamed, setTrustNamed] = useState(false)
  const [receivedOn, setReceivedOn] = useState<CalendarDate | null>(today)
  const [notes, setNotes] = useState('')
  const [file, setFile] = useState<File | null>(null)
  const [dragOver, setDragOver] = useState(false)
  // A file uploaded on an attempt whose record then failed; reused on retry so Documents gets no duplicate.
  const [uploaded, setUploaded] = useState<{ file: File; documentId: string } | null>(null)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const upload = useUploadDocument()
  const record = useRecordInsuranceCertificate()
  const { success } = useToast()

  const dwellingCoverage = parseAmount(dwelling)
  const liabilityPerOccurrence = parseAmount(liability)
  const history = lease.insurance?.requirementsHistory ?? []
  const requirements = policyStart
    ? requirementsOn(history, policyStart)
    : {
        replacementValue: lease.insurance?.requiredReplacementValue ?? null,
        liabilityPerOccurrence: lease.insurance?.requiredLiabilityPerOccurrence ?? null,
      }
  const check =
    dwellingCoverage != null && liabilityPerOccurrence != null
      ? checkCertificate(requirements, { dwellingCoverage, liabilityPerOccurrence, trustNamedAdditionalInsured: trustNamed })
      : null

  const pending = upload.isPending || record.isPending
  const canSubmit =
    !!insurer.trim() && !!policyNumber.trim() && !!policyStart && !!policyEnd && policyEnd > policyStart &&
    dwellingCoverage != null && dwellingCoverage >= 0 && liabilityPerOccurrence != null && liabilityPerOccurrence >= 0 &&
    !!receivedOn && receivedOn <= today
  const isDirty = !!(insurer || policyNumber || policyStart || policyEnd || dwelling || liability || trustNamed || notes || file) ||
    receivedOn !== today

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!canSubmit) return
    setProblem(null)
    let documentId: string | null = uploaded && uploaded.file === file ? uploaded.documentId : null
    if (file && !documentId) {
      try {
        const result = await upload.mutateAsync({
          metadata: {
            title: `Insurance certificate ${insurer.trim()} ${policyNumber.trim()}`,
            sensitivity: 'Confidential',
            retention: 'LongTerm7yr',
            initialAttachment: { entityType: 'GroundLease', entityId: lease.id!, role: 'insurance-certificate' },
          },
          file,
        })
        documentId = result.documentId
        setUploaded({ file, documentId })
      } catch (err) {
        setProblem(await getApiProblem(err, 'The certificate file could not be uploaded. Nothing was recorded.'))
        return
      }
    }
    try {
      await record.mutateAsync({
        groundLeaseId: lease.id!,
        insurer: insurer.trim(),
        policyNumber: policyNumber.trim(),
        policyStart: policyStart!,
        policyEnd: policyEnd!,
        dwellingCoverage: dwellingCoverage!,
        liabilityPerOccurrence: liabilityPerOccurrence!,
        trustNamedAdditionalInsured: trustNamed,
        receivedOn: receivedOn!,
        notes: notes.trim() || null,
        documentId,
      })
      success('Insurance certificate recorded')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to record the insurance certificate.'))
    }
  }

  const required = (amount: number | null) => (amount == null ? 'Not set' : formatPrice(amount))

  return (
    <SlideOver
      title="Record Insurance Certificate"
      subtitle={[lease.homeownerFullName, home?.label].filter(Boolean).join(' · ') || 'Ground lease'}
      onClose={onClose}
      isDirty={isDirty}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="record-insurance-certificate-form"
            disabled={pending || !canSubmit}
          >
            {upload.isPending ? 'Uploading…' : record.isPending ? 'Recording…' : 'Record Certificate'}
          </button>
        </>
      }
    >
      <form id="record-insurance-certificate-form" onSubmit={handleSubmit}>
        <div className="form-hint" style={{ marginBottom: 6 }}>
          The lease requires{policyStart ? ' on the policy start' : ''}:
        </div>
        <div className="detail-grid" style={{ marginBottom: 14 }} aria-label="The lease requires">
          <div className="detail-field">
            <div className="detail-field__label">Replacement Value</div>
            <div className="detail-field__value mono">{required(requirements.replacementValue)}</div>
          </div>
          <div className="detail-field">
            <div className="detail-field__label">Liability per Occurrence</div>
            <div className="detail-field__value mono">{required(requirements.liabilityPerOccurrence)}</div>
          </div>
        </div>

        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="certificate-insurer">Insurer</label>
            <input id="certificate-insurer" className="form-input" value={insurer} onChange={e => setInsurer(e.target.value)} required />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="certificate-policy-number">Policy Number</label>
            <input id="certificate-policy-number" className="form-input" value={policyNumber} onChange={e => setPolicyNumber(e.target.value)} required />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="certificate-policy-start">Policy Start</label>
            <DateInput
              id="certificate-policy-start"
              className="form-input"
              value={policyStart}
              onChange={start => {
                setPolicyStart(start)
                // Most policies run a year; fill the end until the user sets one.
                if (!policyEndTyped) setPolicyEnd(oneYearOn(start))
              }}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="certificate-policy-end">Policy End</label>
            <DateInput
              id="certificate-policy-end"
              className="form-input"
              value={policyEnd}
              min={policyStart ? addDays(policyStart, 1) : undefined}
              onChange={end => {
                setPolicyEnd(end)
                setPolicyEndTyped(true)
              }}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="certificate-dwelling">Dwelling Coverage ($)</label>
            <input
              id="certificate-dwelling"
              className="form-input"
              type="number"
              min="0"
              step="0.01"
              value={dwelling}
              onChange={e => setDwelling(e.target.value)}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="certificate-liability">Liability per Occurrence ($)</label>
            <input
              id="certificate-liability"
              className="form-input"
              type="number"
              min="0"
              step="0.01"
              value={liability}
              onChange={e => setLiability(e.target.value)}
              required
            />
          </div>
          <div className="form-field full" style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <input type="checkbox" id="certificate-trust-named" checked={trustNamed} onChange={e => setTrustNamed(e.target.checked)} />
            <label htmlFor="certificate-trust-named" className="form-label" style={{ marginBottom: 0 }}>
              Trust named as additional insured
            </label>
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="certificate-received-on">Received On</label>
            <DateInput id="certificate-received-on" className="form-input" value={receivedOn} max={today} onChange={setReceivedOn} required />
          </div>
          <div className="form-field full">
            <label className="form-label" htmlFor="certificate-notes">Notes (optional)</label>
            <textarea
              id="certificate-notes"
              className="form-textarea"
              value={notes}
              onChange={e => setNotes(e.target.value)}
              placeholder="Flood cover, agent contact"
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
              const dropped = e.dataTransfer.files[0]
              if (dropped) setFile(dropped)
            }}
          >
            <label className="form-label" htmlFor="certificate-file">Certificate File (optional)</label>
            <input
              id="certificate-file"
              type="file"
              className="form-input"
              accept="application/pdf,image/*"
              style={{ cursor: 'pointer' }}
              onChange={e => setFile(e.target.files?.[0] ?? null)}
            />
            <p className="form-hint">
              {file ? `${file.name} goes to Documents, attached to this lease.` : 'PDF or image. Or drop the file here.'}
            </p>
          </div>
        </div>

        {check && (
          <div role="status" aria-label="Check against the lease" style={{ marginTop: 14, fontSize: 13 }}>
            {check.shortfalls.length === 0 ? (
              <div style={{ color: 'var(--success)' }}>Meets the lease</div>
            ) : (
              <ul style={{ margin: 0, paddingLeft: 18, color: 'var(--warning)' }}>
                {check.shortfalls.map(s => <li key={s}>{s}</li>)}
              </ul>
            )}
            {check.notChecked.map(s => (
              <div key={s} style={{ color: 'var(--text-secondary)', marginTop: 4 }}>{s}</div>
            ))}
            {check.shortfalls.length > 0 && (
              <p className="form-hint">You can still record it; it shows on Today as short of the lease.</p>
            )}
          </div>
        )}

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
