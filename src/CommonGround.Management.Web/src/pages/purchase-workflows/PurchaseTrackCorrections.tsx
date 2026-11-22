import { useState, type FormEvent } from 'react'
import type { PurchaseCorrectionChangeDto, PurchaseCorrectionDto, PurchaseWorkflowResponse } from '../../api'
import { DateInput } from '../../components/DateInput'
import {
  useCorrectAppraisal,
  useCorrectEligibility,
  useCorrectFinancing,
  type CorrectAppraisalForm,
  type CorrectEligibilityForm,
  type CorrectFinancingForm,
} from '../../hooks/purchaseWorkflows'
import { formatCalendarDate, formatInstantDate, parseCalendarDate, trustToday } from '../../lib/calendarDate'
import { formatCurrency } from '../../lib/currency'
import { getErrorMessage } from '../../lib/getErrorMessage'
import { EligibilityIncomeCheck } from './EligibilityIncomeCheck'
import { useUserName } from '../../hooks/useUserName'

interface CorrectionFormProps {
  detail: PurchaseWorkflowResponse
  showToast: (msg: string, kind: 'success' | 'error') => void
  /** Called after the correction is saved. */
  onDone: () => void
}

const REASON_HINT = 'Why the record is corrected. Kept with the old and new values.'

function ReasonField({ id, value, onChange }: { id: string; value: string; onChange: (v: string) => void }) {
  return (
    <div className="form-field full">
      <label className="form-label" htmlFor={id}>Reason</label>
      <input id={id} className="form-input" value={value} onChange={e => onChange(e.target.value)} required />
      <p className="form-hint">{REASON_HINT}</p>
    </div>
  )
}

/** Runs a correction and reports the outcome in a toast; closes the form only when it was saved. */
function useCorrectionSubmit<T>(
  mutateAsync: (args: { workflowId: string; form: T }) => Promise<unknown>,
  { detail, showToast, onDone }: CorrectionFormProps,
) {
  return async (e: FormEvent, form: T) => {
    e.preventDefault()
    try {
      await mutateAsync({ workflowId: detail.id!, form })
      showToast('Correction saved', 'success')
      onDone()
    } catch (error) {
      showToast(await getErrorMessage(error, 'Correction failed'), 'error')
    }
  }
}

export function CorrectEligibilityPanel(props: CorrectionFormProps) {
  const { detail } = props
  const recorded = detail.eligibilityDetails
  const mutation = useCorrectEligibility()
  const submit = useCorrectionSubmit(mutation.mutateAsync, props)
  const [form, setForm] = useState<CorrectEligibilityForm>(() => ({
    verifiedDate: recorded?.verifiedDate ?? trustToday(),
    verifiedAnnualIncome: String(recorded?.verifiedAnnualIncome ?? ''),
    verifiedHouseholdSize: String(recorded?.verifiedHouseholdSize ?? ''),
    notes: recorded?.notes ?? '',
    incomeFiles: [],
    reason: '',
  }))
  const recordedCount = recorded?.incomeDocumentIds?.length ?? 0

  return (
    <form onSubmit={e => submit(e, form)} aria-label="Correct Eligibility">
      <div className="form-grid" style={{ marginBottom: 10 }}>
        <div className="form-field">
          <label className="form-label">Verified Date</label>
          <DateInput className="form-input" value={form.verifiedDate} onChange={v => setForm(f => ({ ...f, verifiedDate: v }))} required />
        </div>
        <div className="form-field">
          <label className="form-label" htmlFor="correct-elig-income">Verified Annual Income ($)</label>
          <input
            id="correct-elig-income" className="form-input" type="number" min="0" step="0.01" required
            value={form.verifiedAnnualIncome}
            onChange={e => setForm(f => ({ ...f, verifiedAnnualIncome: e.target.value }))}
          />
        </div>
        <div className="form-field">
          <label className="form-label" htmlFor="correct-elig-size">Verified Household Size</label>
          <input
            id="correct-elig-size" className="form-input" type="number" min="1" step="1" required
            value={form.verifiedHouseholdSize}
            onChange={e => setForm(f => ({ ...f, verifiedHouseholdSize: e.target.value }))}
          />
        </div>
        <div className="form-field full">
          <EligibilityIncomeCheck detail={detail} form={form} />
        </div>
        <div className="form-field full">
          <label className="form-label" htmlFor="correct-elig-files">Income Documents</label>
          <input
            id="correct-elig-files" type="file" multiple accept="application/pdf,image/*" className="form-input"
            style={{ cursor: 'pointer' }}
            onChange={e => setForm(f => ({ ...f, incomeFiles: Array.from(e.target.files ?? []) }))}
          />
          <p className="form-hint">
            {form.incomeFiles.length > 0
              ? `${form.incomeFiles.map(f => f.name).join(', ')} replace the ${recordedCount} recorded ${recordedCount === 1 ? 'file' : 'files'}. The old files stay in Documents.`
              : `Leave empty to keep the ${recordedCount} recorded ${recordedCount === 1 ? 'file' : 'files'}.`}
          </p>
        </div>
        <div className="form-field full">
          <label className="form-label">Notes (optional)</label>
          <input className="form-input" value={form.notes} onChange={e => setForm(f => ({ ...f, notes: e.target.value }))} />
        </div>
        <ReasonField id="correct-elig-reason" value={form.reason} onChange={reason => setForm(f => ({ ...f, reason }))} />
      </div>
      <button type="submit" className="btn btn-primary btn-sm" disabled={mutation.isPending}>
        {mutation.isPending ? 'Saving…' : 'Save Correction'}
      </button>
    </form>
  )
}

export function CorrectAppraisalPanel(props: CorrectionFormProps) {
  const recorded = props.detail.appraisalDetails
  const mutation = useCorrectAppraisal()
  const submit = useCorrectionSubmit(mutation.mutateAsync, props)
  const [form, setForm] = useState<CorrectAppraisalForm>(() => ({
    appraisedValue: String(recorded?.appraisedValue ?? ''),
    appraisalDate: recorded?.appraisalDate ?? trustToday(),
    appraiserName: recorded?.appraiserName ?? '',
    notes: recorded?.notes ?? '',
    reason: '',
  }))

  return (
    <form onSubmit={e => submit(e, form)} aria-label="Correct Appraisal">
      <div className="form-grid" style={{ marginBottom: 10 }}>
        <div className="form-field">
          <label className="form-label" htmlFor="correct-appraised-value">Appraised Value ($)</label>
          <input
            id="correct-appraised-value" className="form-input" type="number" min="1" step="1" required
            value={form.appraisedValue}
            onChange={e => setForm(f => ({ ...f, appraisedValue: e.target.value }))}
          />
        </div>
        <div className="form-field">
          <label className="form-label">Appraisal Date</label>
          <DateInput className="form-input" value={form.appraisalDate} onChange={v => setForm(f => ({ ...f, appraisalDate: v }))} required />
        </div>
        <div className="form-field">
          <label className="form-label">Appraiser Name</label>
          <input className="form-input" required value={form.appraiserName} onChange={e => setForm(f => ({ ...f, appraiserName: e.target.value }))} />
        </div>
        <div className="form-field full">
          <label className="form-label">Notes (optional)</label>
          <input className="form-input" value={form.notes} onChange={e => setForm(f => ({ ...f, notes: e.target.value }))} />
        </div>
        <ReasonField id="correct-appraisal-reason" value={form.reason} onChange={reason => setForm(f => ({ ...f, reason }))} />
      </div>
      <button type="submit" className="btn btn-primary btn-sm" disabled={mutation.isPending}>
        {mutation.isPending ? 'Saving…' : 'Save Correction'}
      </button>
    </form>
  )
}

export function CorrectFinancingPanel(props: CorrectionFormProps) {
  const recorded = props.detail.financingDetails
  const mutation = useCorrectFinancing()
  const submit = useCorrectionSubmit(mutation.mutateAsync, props)
  const [form, setForm] = useState<CorrectFinancingForm>(() => ({
    lenderName: recorded?.lenderName ?? '',
    committedAmount: String(recorded?.committedAmount ?? ''),
    preapprovalLetterDate: recorded?.preapprovalLetterDate ?? trustToday(),
    reason: '',
  }))

  return (
    <form onSubmit={e => submit(e, form)} aria-label="Correct Financing">
      <div className="form-grid" style={{ marginBottom: 10 }}>
        <div className="form-field">
          <label className="form-label">Lender Name</label>
          <input className="form-input" required value={form.lenderName} onChange={e => setForm(f => ({ ...f, lenderName: e.target.value }))} />
        </div>
        <div className="form-field">
          <label className="form-label" htmlFor="correct-committed-amount">Committed Amount ($)</label>
          <input
            id="correct-committed-amount" className="form-input" type="number" min="0" step="1" required
            value={form.committedAmount}
            onChange={e => setForm(f => ({ ...f, committedAmount: e.target.value }))}
          />
        </div>
        <div className="form-field">
          <label className="form-label">Pre-Approval Letter Date</label>
          <DateInput className="form-input" value={form.preapprovalLetterDate} onChange={v => setForm(f => ({ ...f, preapprovalLetterDate: v }))} required />
        </div>
        <ReasonField id="correct-financing-reason" value={form.reason} onChange={reason => setForm(f => ({ ...f, reason }))} />
      </div>
      <button type="submit" className="btn btn-primary btn-sm" disabled={mutation.isPending}>
        {mutation.isPending ? 'Saving…' : 'Save Correction'}
      </button>
    </form>
  )
}

const humanizeField = (field: string) => {
  const words = field.replace(/([a-z])([A-Z])/g, '$1 $2').toLowerCase()
  return words.charAt(0).toUpperCase() + words.slice(1)
}

const countFiles = (ids: string | null | undefined) => (ids ? ids.split(',').filter(Boolean).length : 0)

const formatChangeValue = (kind: string | undefined, value: string | null | undefined) => {
  if (value == null || value === '') return 'none'
  switch (kind) {
    case 'Money': return formatCurrency(Number(value))
    case 'Date': return formatCalendarDate(parseCalendarDate(value))
    case 'Documents': { const n = countFiles(value); return `${n} ${n === 1 ? 'file' : 'files'}` }
    default: return value
  }
}

const describeChange = (c: PurchaseCorrectionChangeDto) =>
  `${humanizeField(c.field ?? '')}: ${formatChangeValue(c.kind, c.was)} → ${formatChangeValue(c.kind, c.now)}`

/** Every correction to a recorded track, newest first as the API sends them. */
export function CorrectionsList({ corrections }: { corrections: PurchaseCorrectionDto[] }) {
  const nameOf = useUserName()
  if (corrections.length === 0) return null

  return (
    <div style={{ marginTop: 8 }} data-testid="purchase-corrections">
      <div className="detail-section__title">Corrections</div>
      <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
        {corrections.map((c, i) => (
          <li key={i} style={{ marginBottom: 10, fontSize: 13 }}>
            <div>
              <strong>{c.track}</strong>
              <span style={{ color: 'var(--text-secondary)' }}>
                {' · '}{c.correctedAt ? formatInstantDate(c.correctedAt) : ''}{' · '}{nameOf(c.correctedBy)}
              </span>
            </div>
            <div>{c.reason}</div>
            {(c.changes ?? []).map((ch, j) => (
              <div key={j} style={{ color: 'var(--text-secondary)' }}>{describeChange(ch)}</div>
            ))}
          </li>
        ))}
      </ul>
    </div>
  )
}
