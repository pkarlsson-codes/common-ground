import { useEffect, useState } from 'react'
import type { GroundLeaseResponse, ResaleResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import { usePreviewResalePrice, useRecordResaleAppraisal, type RecordResaleAppraisalForm } from '../../hooks/resales'
import { useToast } from '../../hooks/useToast'
import { trustToday } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

const emptyForm = (): RecordResaleAppraisalForm => ({
  appraisedValue: 0,
  appraisalDate: trustToday(),
  appraiserName: '',
  approvedImprovements: 0,
  currentIndexValue: 0,
})

interface Props {
  resale: ResaleResponse
  lease: GroundLeaseResponse | undefined
  onClose: () => void
}

/** Records the appraisal the lease requires after a notice to sell, previewing the prices it sets. */
export default function RecordResaleAppraisalPanel({ resale, lease, onClose }: Props) {
  const [form, setForm] = useState<RecordResaleAppraisalForm>(emptyForm)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useRecordResaleAppraisal()
  const { success } = useToast()

  const formulaKind = lease?.formulaKind ?? ''
  const creditsImprovements = lease?.improvementsCap != null
  const needsIndex = formulaKind === 'IndexBased'
  const leaseFiguresMissing = !!lease && lease.originalAppraisedValue == null

  // The preview follows what the user types, once they stop.
  const [figures, setFigures] = useState(form)
  useEffect(() => {
    const timeout = setTimeout(() => setFigures(form), 300)
    return () => clearTimeout(timeout)
  }, [form])

  const previewReady = !leaseFiguresMissing && !!lease && figures.appraisedValue > 0 && (!needsIndex || figures.currentIndexValue > 0)
  const preview = usePreviewResalePrice(
    resale.id!,
    {
      appraisedValue: figures.appraisedValue,
      appraisalDate: figures.appraisalDate,
      approvedImprovements: creditsImprovements ? figures.approvedImprovements : undefined,
      currentIndexValue: needsIndex ? figures.currentIndexValue : undefined,
    },
    previewReady,
  )

  const valid = !leaseFiguresMissing
    && form.appraisedValue > 0
    && form.appraiserName.trim() !== ''
    && (!needsIndex || form.currentIndexValue > 0)
    && (!creditsImprovements || form.approvedImprovements >= 0)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)
    try {
      await mutation.mutateAsync({ resaleId: resale.id!, form, formulaKind, creditsImprovements })
      success('Appraisal recorded')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to record the appraisal.'))
    }
  }

  return (
    <SlideOver
      title="Record Appraisal"
      subtitle={resale.homeLabel ?? undefined}
      onClose={onClose}
      footer={
        <>
          <button type="button" className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button type="submit" form="record-resale-appraisal-form" className="btn btn-primary" disabled={mutation.isPending || !valid}>
            {mutation.isPending ? 'Recording…' : 'Record Appraisal'}
          </button>
        </>
      }
    >
      <form id="record-resale-appraisal-form" onSubmit={handleSubmit}>
        {leaseFiguresMissing && (
          <div className="form-error" role="alert">
            The lease has no Base Price or Original Appraised Value, so the formula price can't be worked out.
          </div>
        )}
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="resale-appraised-value">Appraised Value ($)</label>
            <input
              id="resale-appraised-value"
              className="form-input"
              type="number"
              min="0"
              step="1"
              value={form.appraisedValue || ''}
              onChange={e => setForm(f => ({ ...f, appraisedValue: parseFloat(e.target.value) || 0 }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="resale-appraisal-date">Appraisal Date</label>
            <DateInput
              id="resale-appraisal-date"
              className="form-input"
              value={form.appraisalDate}
              min={resale.noticeDate}
              max={trustToday()}
              onChange={v => setForm(f => ({ ...f, appraisalDate: v }))}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label" htmlFor="resale-appraiser">Appraiser</label>
            <input
              id="resale-appraiser"
              className="form-input"
              type="text"
              value={form.appraiserName}
              onChange={e => setForm(f => ({ ...f, appraiserName: e.target.value }))}
              required
            />
          </div>
          {creditsImprovements && (
            <div className="form-field">
              <label className="form-label" htmlFor="resale-improvements">Approved Improvements ($)</label>
              <input
                id="resale-improvements"
                className="form-input"
                type="number"
                min="0"
                step="1"
                value={form.approvedImprovements}
                onChange={e => setForm(f => ({ ...f, approvedImprovements: parseFloat(e.target.value) || 0 }))}
              />
            </div>
          )}
          {needsIndex && (
            <div className="form-field">
              <label className="form-label" htmlFor="resale-index">Current Index Value</label>
              <input
                id="resale-index"
                className="form-input"
                type="number"
                min="0"
                step="any"
                value={form.currentIndexValue || ''}
                onChange={e => setForm(f => ({ ...f, currentIndexValue: parseFloat(e.target.value) || 0 }))}
                required
              />
            </div>
          )}
        </div>

        <div className="detail-section" aria-live="polite">
          <div className="detail-section__title">Price</div>
          <div className="detail-grid">
            <div className="detail-field">
              <div className="detail-field__label">Formula Price</div>
              <div className="detail-field__value mono">{preview.data ? formatPrice(preview.data.formulaPrice!) : '—'}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Appraised Value</div>
              <div className="detail-field__value mono">{form.appraisedValue > 0 ? formatPrice(form.appraisedValue) : '—'}</div>
            </div>
            <div className="detail-field full-span">
              <div className="detail-field__label">Purchase Option Price</div>
              <div className="detail-field__value mono">{preview.data ? formatPrice(preview.data.purchaseOptionPrice!) : '—'}</div>
            </div>
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
