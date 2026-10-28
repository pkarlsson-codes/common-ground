import { useState } from 'react'
import NavIdLink from '../../components/NavIdLink'
import { findOpenApplication, useHousehold, useHouseholds } from '../../hooks/households'
import { formatPhoneDisplay } from '../../lib/phone'

const MAX_RESULTS = 5

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

type Props = {
  selectedId: string | null
  onSelect: (householdId: string | null) => void
}

export default function ExistingHouseholdPicker({ selectedId, onSelect }: Props) {
  const [search, setSearch] = useState('')
  const term = search.trim()
  const { data, isLoading } = useHouseholds({ pageNumber: 1, pageSize: MAX_RESULTS, search: term || undefined })
  const results = term ? data?.items ?? [] : []

  if (selectedId) {
    return <SelectedHousehold householdId={selectedId} onSelect={onSelect} />
  }

  return (
    <div className="detail-section">
      <div className="detail-section__title">Find Household</div>
      <input
        className="form-input"
        type="search"
        aria-label="Search households"
        placeholder="Search by member name or email"
        value={search}
        onChange={e => setSearch(e.target.value)}
        style={{ marginTop: 10 }}
        autoFocus
      />
      {term && isLoading && <div className="detail-field__label" style={{ marginTop: 10 }}>Searching…</div>}
      {term && !isLoading && results.length === 0 && (
        <div className="detail-field__label" style={{ marginTop: 10 }}>No households match "{term}".</div>
      )}
      {results.length > 0 && (
        <table className="registry-table" style={{ marginTop: 10 }}>
          <tbody>
            {results.map(h => (
              <tr key={h.id}>
                <td>{h.primaryContact?.name}</td>
                <td><span className="cell-mono">{h.primaryContact?.email}</span></td>
                <td>{h.householdSize} {h.householdSize === 1 ? 'person' : 'people'}</td>
                <td style={{ textAlign: 'right' }}>
                  <button
                    type="button"
                    className="btn btn-ghost btn-sm"
                    aria-label={`Select ${h.primaryContact?.name}`}
                    onClick={() => onSelect(h.id ?? null)}
                  >
                    Select
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}

function SelectedHousehold({ householdId, onSelect }: { householdId: string; onSelect: Props['onSelect'] }) {
  const { data: household, isLoading, isError } = useHousehold(householdId)
  const openApplication = findOpenApplication(household)

  return (
    <div className="detail-section">
      <div className="detail-section__title" style={{ display: 'flex', alignItems: 'center' }}>
        Selected Household
        <button
          type="button"
          className="btn btn-ghost btn-sm"
          style={{ marginLeft: 'auto' }}
          onClick={() => onSelect(null)}
        >
          Change
        </button>
      </div>
      {isLoading && <div className="skeleton-rows"><div className="skeleton-row" /></div>}
      {isError && <div className="form-error">Could not load this household.</div>}
      {household && (
        <>
          <div className="detail-grid" style={{ marginTop: 10 }}>
            {(household.members ?? []).map(member => (
              <div className="detail-field full-span" key={member.id}>
                <div className="detail-field__label">{member.isPrimary ? 'Primary Contact' : 'Member'}</div>
                <div className="detail-field__value">
                  {[member.name, member.email, formatPhoneDisplay(member.phone)].filter(Boolean).join(' · ')}
                </div>
              </div>
            ))}
            <div className="detail-field">
              <div className="detail-field__label">Household Size</div>
              <div className="detail-field__value mono">{household.householdSize ?? '—'}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Annual Income</div>
              <div className="detail-field__value mono">
                {typeof household.annualIncome === 'number' ? fmtCurrency(household.annualIncome) : '—'}
              </div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Accessibility Need</div>
              <div className="detail-field__value">{household.accessibilityRequired ? 'Yes' : 'No'}</div>
            </div>
          </div>
          {openApplication ? (
            <div style={{ marginTop: 12 }}>
              <div className="form-error" style={{ marginBottom: 8 }}>
                This household already has an open application.
              </div>
              <NavIdLink id={openApplication.id} to="/applications" label="Open Active Application" />
            </div>
          ) : (
            <div className="detail-field__label" style={{ marginTop: 12 }}>
              The application will capture this household's current size, income and accessibility.
              To change them first, edit the household on the Households page.
            </div>
          )}
        </>
      )}
    </div>
  )
}
