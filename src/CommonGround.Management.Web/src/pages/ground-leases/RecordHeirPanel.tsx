import { useState } from 'react'
import type { HeirTransferResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useAttachDocument, useUploadDocument } from '../../hooks/documents'
import { useHeirIncomeLimit, useRecordHeir } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { yearOf, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

const RELATIONSHIPS = [
  { value: 'Spouse', label: 'Spouse' },
  { value: 'Child', label: 'Child' },
  { value: 'HouseholdMember', label: 'Household Member' },
  { value: 'Other', label: 'Other' },
]

interface Props {
  transfer: HeirTransferResponse
  onClose: () => void
}

/**
 * Records who is taking the home and how they qualify (Art 10.3). A household member needs a year in the home
 * before the death; anyone else must be under the home's income limit, shown here, with income documents. The
 * documents upload and attach to the lease first, because the API needs their IDs; a retry reuses them.
 */
export default function RecordHeirPanel({ transfer, onClose }: Props) {
  const changing = !!transfer.heirFullName
  const [fullName, setFullName] = useState(transfer.heirFullName ?? '')
  const [email, setEmail] = useState(transfer.heirEmail ?? '')
  const [phone, setPhone] = useState(transfer.heirPhone ?? '')
  const [relationship, setRelationship] = useState(transfer.relationship ?? 'Spouse')
  const [movedInOn, setMovedInOn] = useState<CalendarDate | null>(transfer.heirMovedInOn ?? null)
  const [income, setIncome] = useState(transfer.heirVerifiedAnnualIncome?.toString() ?? '')
  const [householdSize, setHouseholdSize] = useState(transfer.heirVerifiedHouseholdSize?.toString() ?? '')
  const [files, setFiles] = useState<File[]>([])
  // Files uploaded and attached on an attempt whose record then failed, by file; reused on retry.
  const [uploaded, setUploaded] = useState<Map<File, string>>(new Map())
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const upload = useUploadDocument()
  const attach = useAttachDocument()
  const record = useRecordHeir()
  const { success } = useToast()
  const isOther = relationship === 'Other'
  const { data: limit } = useHeirIncomeLimit(transfer.id ?? null, isOther)

  const diedOn = transfer.diedOn!
  const underOneYear = !!movedInOn && `${yearOf(movedInOn) + 1}${movedInOn.slice(4)}` > diedOn
  const size = parseInt(householdSize, 10)
  const annualIncome = parseFloat(income)
  const sizeValid = size >= 1 && size <= 8
  const limitForSize = limit && sizeValid ? limit.limits?.[size - 1] : undefined
  const noLimit = !!limit && (limit.limits ?? []).length === 0
  const over = limitForSize != null && annualIncome > limitForSize
  const hasIncomeDocs = files.length > 0

  const baseValid = fullName.trim() !== '' && email.trim() !== ''
  const relationshipValid =
    relationship === 'HouseholdMember' ? !!movedInOn && movedInOn <= diedOn
    : isOther ? annualIncome >= 0 && income !== '' && sizeValid && hasIncomeDocs
    : true
  const valid = baseValid && relationshipValid
  const pending = upload.isPending || attach.isPending || record.isPending

  const addFiles = (added: FileList | null) => {
    if (added && added.length > 0) setFiles(current => [...current, ...Array.from(added)])
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!valid) return
    setProblem(null)

    const documentIds: string[] = []
    if (isOther) {
      for (const file of files) {
        let documentId = uploaded.get(file)
        if (!documentId) {
          try {
            const result = await upload.mutateAsync({
              metadata: { title: `Heir income ${fullName.trim()} ${file.name}`, sensitivity: 'Confidential', retention: 'LongTerm7yr' },
              file,
            })
            documentId = result.documentId
            await attach.mutateAsync({
              documentId,
              req: { entityType: 'GroundLease', entityId: transfer.groundLeaseId!, role: 'heir-income-document' },
            })
            setUploaded(map => new Map(map).set(file, documentId!))
          } catch (err) {
            setProblem(await getApiProblem(err, `${file.name} could not be uploaded. The heir was not recorded.`))
            return
          }
        }
        documentIds.push(documentId)
      }
    }

    try {
      await record.mutateAsync({
        heirTransferId: transfer.id!,
        groundLeaseId: transfer.groundLeaseId!,
        request: {
          fullName: fullName.trim(),
          email: email.trim(),
          phone: phone.trim() || null,
          relationship,
          movedInOn: relationship === 'HouseholdMember' ? movedInOn : null,
          verifiedAnnualIncome: isOther ? annualIncome : null,
          verifiedHouseholdSize: isOther ? size : null,
          incomeDocumentIds: isOther ? documentIds : null,
        },
      })
      success('Heir recorded')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to record the heir.'))
    }
  }

  return (
    <SlideOver
      title={changing ? 'Change Heir' : 'Record Heir'}
      subtitle={transfer.deceasedHomeownerFullName ?? undefined}
      onClose={onClose}
      isDirty={fullName !== (transfer.heirFullName ?? '') || email !== (transfer.heirEmail ?? '') || files.length > 0}
      footer={
        <>
          <SlideOverCancelButton />
          <button className="btn btn-primary" type="submit" form="record-heir-form" disabled={pending || !valid}>
            {upload.isPending ? 'Uploading…' : pending ? 'Recording…' : 'Record Heir'}
          </button>
        </>
      }
    >
      <form id="record-heir-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="heir-full-name">Full Name</label>
            <input id="heir-full-name" className="form-input" value={fullName} onChange={e => setFullName(e.target.value)} required />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="heir-email">Email</label>
            <input id="heir-email" className="form-input" type="email" value={email} onChange={e => setEmail(e.target.value)} required />
            <p className="form-hint">The lease's notices go here.</p>
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="heir-phone">Phone (optional)</label>
            <input id="heir-phone" className="form-input" type="tel" value={phone} onChange={e => setPhone(e.target.value)} />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="heir-relationship">Relationship</label>
            <select id="heir-relationship" className="form-select" value={relationship} onChange={e => setRelationship(e.target.value)}>
              {RELATIONSHIPS.map(r => <option key={r.value} value={r.value}>{r.label}</option>)}
            </select>
          </div>

          {relationship === 'HouseholdMember' && (
            <div className="form-field full">
              <label className="form-label" htmlFor="heir-moved-in-on">Lived In Home Since</label>
              <DateInput id="heir-moved-in-on" className="form-input" value={movedInOn} max={diedOn} onChange={setMovedInOn} required />
              <p className="form-hint">Art 10.3 needs a year in the home before the death.</p>
              {underOneYear && <p className="form-hint" role="status" style={{ color: 'var(--danger)' }}>Under one year</p>}
            </div>
          )}

          {isOther && (
            <>
              <div className="form-field">
                <label className="form-label" htmlFor="heir-income">Verified Annual Income ($)</label>
                <input id="heir-income" className="form-input" type="number" min="0" step="0.01" value={income} onChange={e => setIncome(e.target.value)} required />
              </div>
              <div className="form-field">
                <label className="form-label" htmlFor="heir-household-size">Household Size</label>
                <input id="heir-household-size" className="form-input" type="number" min="1" max="8" step="1" value={householdSize} onChange={e => setHouseholdSize(e.target.value)} required />
              </div>
              {noLimit && <p className="form-hint full" role="status">No income limit on this home.</p>}
              {limitForSize != null && (
                <p className="form-hint full" role="status">
                  Limit for {size} {size === 1 ? 'person' : 'people'}: <span className="cell-mono">{formatPrice(limitForSize)}</span>
                  {over && <span style={{ color: 'var(--danger)' }}> · Over income limit</span>}
                </p>
              )}
              <div className="form-field full">
                <label className="form-label" htmlFor="heir-income-files">Income Documents</label>
                <input
                  id="heir-income-files"
                  type="file"
                  multiple
                  className="form-input"
                  accept="application/pdf,image/*"
                  style={{ cursor: 'pointer' }}
                  value=""
                  onChange={e => addFiles(e.target.files)}
                />
                {files.length === 0 ? (
                  <p className="form-hint">Pay stubs, tax returns. At least one. PDF or image.</p>
                ) : (
                  <ul style={{ margin: '8px 0 0', paddingLeft: 18, fontSize: 13 }} aria-label="Income documents to attach">
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
                )}
              </div>
            </>
          )}
        </div>
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
