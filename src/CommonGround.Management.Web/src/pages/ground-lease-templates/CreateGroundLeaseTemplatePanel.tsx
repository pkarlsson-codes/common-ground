import { useState } from 'react'
import DebtCapField from '../ground-leases/DebtCapField'
import OptionalAmountField from '../ground-leases/OptionalAmountField'
import SlideOver from '../../components/SlideOver'
import { useCreateGroundLeaseTemplate } from '../../hooks/groundLeaseTemplates'
import {
  FORMULA_KIND_LABELS,
  FORMULA_KINDS,
  INDEX_TYPES,
  type CreateGroundLeaseTemplateForm,
} from '../../hooks/types'
import { useToast } from '../../hooks/useToast'
import LeaseFeeFields from '../ground-leases/LeaseFeeFields'
import { getErrorMessage } from '../../lib/getErrorMessage'

const emptyCreate = (): CreateGroundLeaseTemplateForm => ({
  templateName: '',
  termYears: 99,
  optionWindowDays: 60,
  landUseFee: 0,
  repairReserveFee: 0,
  lateInterestRate: 0,
  interestWaiverDays: 30,
  covenantTerms: '',
  formulaKind: 'AppraisalBased',
  appreciationShare: 0.25,
  indexType: 'CPI',
  baselineIndexValue: 100,
  annualPercentage: 0.01,
  improvementsCap: null,
  hasImprovementsCap: false,
  maxMortgageDebtPercent: null,
  insuranceLiabilityPerOccurrence: null,
})

interface Props {
  onClose: () => void
}

export default function CreateGroundLeaseTemplatePanel({ onClose }: Props) {
  const [form, setForm] = useState<CreateGroundLeaseTemplateForm>(emptyCreate)
  const [error, setError] = useState<string | null>(null)
  const mutation = useCreateGroundLeaseTemplate()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    try {
      await mutation.mutateAsync(form)
      success('Ground lease template created')
      onClose()
    } catch (err) {
      setError(await getErrorMessage(err, 'Failed to create ground lease template.'))
    }
  }

  return (
    <SlideOver
      title="New Template"
      subtitle="Create a ground lease template"
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="create-ground-lease-template-form"
            disabled={mutation.isPending}
          >
            {mutation.isPending ? 'Creating…' : 'Create Template'}
          </button>
        </>
      }
    >
      <form id="create-ground-lease-template-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label">Template Name</label>
            <input
              className="form-input"
              type="text"
              value={form.templateName}
              onChange={e => setForm(f => ({ ...f, templateName: e.target.value }))}
              required
            />
          </div>

          <div className="form-field">
            <label className="form-label">Term (years)</label>
            <input
              className="form-input"
              type="number"
              min="1"
              max="999"
              value={form.termYears}
              onChange={e => setForm(f => ({ ...f, termYears: parseInt(e.target.value) || 99 }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Option Window (days)</label>
            <input
              className="form-input"
              type="number"
              min="1"
              max="999"
              value={form.optionWindowDays}
              onChange={e => setForm(f => ({ ...f, optionWindowDays: parseInt(e.target.value) || 60 }))}
              required
            />
          </div>
          <LeaseFeeFields
            fees={form}
            onChange={fees => setForm(f => ({ ...f, ...fees }))}
          />

          <div className="form-field full">
            <label className="form-label">Covenant Terms</label>
            <textarea
              className="form-textarea"
              value={form.covenantTerms}
              onChange={e => setForm(f => ({ ...f, covenantTerms: e.target.value }))}
              placeholder="Describe covenant restrictions…"
              required
            />
          </div>

          <div className="form-field full">
            <label className="form-label">Resale Formula</label>
            <select
              className="form-select"
              value={form.formulaKind}
              onChange={e => setForm(f => ({ ...f, formulaKind: e.target.value }))}
            >
              {FORMULA_KINDS.map(k => (
                <option key={k} value={k}>{FORMULA_KIND_LABELS[k]}</option>
              ))}
            </select>
          </div>

          {form.formulaKind === 'AppraisalBased' && (
            <div className="form-field">
              <label className="form-label">Appreciation Share</label>
              <input
                className="form-input"
                type="number"
                min="0"
                max="1"
                step="0.01"
                value={form.appreciationShare ?? ''}
                onChange={e => setForm(f => ({ ...f, appreciationShare: parseFloat(e.target.value) || 0 }))}
                required
              />
              <div className="form-help">Decimal fraction, e.g. 0.25 = 25%</div>
            </div>
          )}

          {form.formulaKind === 'IndexBased' && (
            <>
              <div className="form-field">
                <label className="form-label">Index Type</label>
                <select
                  className="form-select"
                  value={form.indexType ?? 'CPI'}
                  onChange={e => setForm(f => ({ ...f, indexType: e.target.value }))}
                >
                  {INDEX_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
                </select>
              </div>
              <div className="form-field">
                <label className="form-label">Baseline Index Value</label>
                <input
                  className="form-input"
                  type="number"
                  min="0"
                  step="0.01"
                  value={form.baselineIndexValue ?? ''}
                  onChange={e => setForm(f => ({ ...f, baselineIndexValue: parseFloat(e.target.value) || 0 }))}
                  required
                />
              </div>
            </>
          )}

          {form.formulaKind === 'FixedPercentage' && (
            <div className="form-field">
              <label className="form-label">Annual Percentage</label>
              <input
                className="form-input"
                type="number"
                min="0"
                max="1"
                step="0.001"
                value={form.annualPercentage ?? ''}
                onChange={e => setForm(f => ({ ...f, annualPercentage: parseFloat(e.target.value) || 0 }))}
                required
              />
              <div className="form-help">Decimal fraction, e.g. 0.03 = 3%</div>
            </div>
          )}

          <div className="form-field full" style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <input
              type="checkbox"
              id="template-has-cap"
              checked={form.hasImprovementsCap}
              onChange={e => setForm(f => ({ ...f, hasImprovementsCap: e.target.checked, improvementsCap: e.target.checked ? (f.improvementsCap ?? 25000) : null }))}
            />
            <label htmlFor="template-has-cap" className="form-label" style={{ marginBottom: 0 }}>
              Set improvements cap
            </label>
          </div>

          {form.hasImprovementsCap && (
            <div className="form-field">
              <label className="form-label">Improvements Cap ($)</label>
              <input
                className="form-input"
                type="number"
                min="0"
                step="1"
                value={form.improvementsCap ?? ''}
                onChange={e => setForm(f => ({ ...f, improvementsCap: parseFloat(e.target.value) || 0 }))}
                required
              />
            </div>
          )}
          <DebtCapField
            value={form.maxMortgageDebtPercent}
            onChange={maxMortgageDebtPercent => setForm(f => ({ ...f, maxMortgageDebtPercent }))}
          />
          <OptionalAmountField
            id="template-liability"
            label="Liability per Occurrence ($)"
            value={form.insuranceLiabilityPerOccurrence}
            onChange={insuranceLiabilityPerOccurrence => setForm(f => ({ ...f, insuranceLiabilityPerOccurrence }))}
            hintId="template-liability-hint"
          />
          <div className="form-field full">
            <p id="template-liability-hint" className="form-hint" style={{ margin: 0 }}>
              Liability insurance the homeowner must carry, with the trust named as additional insured (Art 9.4).
            </p>
          </div>
        </div>

        {error && (
          <div className="form-error">{error}</div>
        )}
      </form>
    </SlideOver>
  )
}
