import { GroundLeaseTemplatesApi } from '../../api'
import EntityCombobox from '../../components/EntityCombobox'
import { DateInput } from '../../components/DateInput'
import { apiConfig } from '../../lib/apiConfig'
import { formatCurrency } from '../../lib/currency'
import type { PurchaseTerms } from './purchaseTerms'
import { purchasePriceWarnings } from './purchasePriceCheck'

const groundLeaseTemplatesApi = new GroundLeaseTemplatesApi(apiConfig)

type Props = {
  value: PurchaseTerms
  onChange: (update: (terms: PurchaseTerms) => PurchaseTerms) => void
  /** The home's latest assessed value, or null when it has none. Undefined while it loads. */
  assessedValue?: number | null
  /**
   * The selected household's prequalified amount, null when it has none. Omit when the household
   * isn't known yet, as in a lottery draw.
   */
  prequalifiedAmount?: number | null
}

export default function PurchaseTermsFields({ value, onChange, assessedValue, prequalifiedAmount }: Props) {
  const warnings = purchasePriceWarnings({ price: value.purchasePrice, prequalifiedAmount })

  return (
    <>
      <div className="form-field full">
        <label className="form-label">Ground Lease Template</label>
        <EntityCombobox
          value={value.groundLeaseTemplateId || null}
          onChange={id => onChange(t => ({ ...t, groundLeaseTemplateId: id ?? '' }))}
          fetchOptions={async q => {
            const result = await groundLeaseTemplatesApi.getGroundLeaseTemplates({
              search: q,
              status: 'Active',
              pageSize: 10,
            })
            return (result.items ?? []).map(t => ({
              id: t.id!,
              label: t.templateName ?? t.id!,
            }))
          }}
          placeholder="Search templates…"
        />
      </div>
      <div className="form-field">
        <label className="form-label">Purchase Price ($)</label>
        <input
          className="form-input"
          type="number"
          min="1"
          step="1"
          value={value.purchasePrice || ''}
          onChange={e => onChange(t => ({ ...t, purchasePrice: parseFloat(e.target.value) || 0 }))}
          required
        />
        <p className="form-hint">
          Assessed value: {assessedValue === undefined ? 'loading…' : assessedValue === null ? 'none on record' : formatCurrency(assessedValue)}
          {prequalifiedAmount !== undefined && (
            <> · Prequalified amount: {prequalifiedAmount === null ? 'none' : formatCurrency(prequalifiedAmount)}</>
          )}
        </p>
        {warnings.map(w => <div key={w} className="form-warning" role="status">{w}</div>)}
      </div>
      <div className="form-field">
        <label className="form-label">Selected Date</label>
        <DateInput
          className="form-input"
          value={value.selectedDate}
          onChange={v => onChange(t => ({ ...t, selectedDate: v }))}
          required
        />
      </div>
    </>
  )
}
