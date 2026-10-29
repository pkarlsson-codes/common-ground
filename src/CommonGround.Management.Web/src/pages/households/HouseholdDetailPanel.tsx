import { useState } from 'react'
import type { HouseholdResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import NavIdLink from '../../components/NavIdLink'
import SlideOver from '../../components/SlideOver'
import { findOpenApplication, useHousehold, useStartApplication, useUpdateHouseholdCircumstances } from '../../hooks/households'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, getResponseStatus, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { formatPhoneDisplay } from '../../lib/phone'
import ApplicationPhaseBadge from '../applications/ApplicationPhaseBadge'
import HouseholdMembersSection from './HouseholdMembersSection'
import RecordCltOrientationPanel from './RecordCltOrientationPanel'
import RecordHousingCounselingPanel from './RecordHousingCounselingPanel'
import RecordMortgagePrequalificationPanel from './RecordMortgagePrequalificationPanel'

type Props = {
  householdId: string
  onClose: () => void
}

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

const fmtDate = (value?: string | null) => {
  if (!value) return '—'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleDateString()
}

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 8 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

type ReadinessPanel = 'housing-counseling' | 'clt-orientation' | 'mortgage-prequalification'

function ReadinessItem({ label, done, status, actionLabel, onAction }: {
  label: string
  done: boolean
  status?: string
  actionLabel?: string
  onAction: () => void
}) {
  return (
    <div className={`checklist-item${done ? ' checklist-item--done' : ''}`}>
      <div className={`checklist-item__icon${done ? ' checklist-item__icon--done' : ' checklist-item__icon--pending'}`}>
        {done ? '✓' : '○'}
      </div>
      <span className="checklist-item__label">{label}</span>
      {done && status && <span className="checklist-item__status">{status}</span>}
      {actionLabel && (
        <button className="btn btn-secondary btn-sm" type="button" aria-label={`${actionLabel} ${label}`} onClick={onAction}>
          {actionLabel}
        </button>
      )}
    </div>
  )
}

function CircumstancesSection({ householdId, household }: { householdId: string; household: HouseholdResponse }) {
  const [editing, setEditing] = useState(false)
  const [form, setForm] = useState({ annualIncome: 0, requiresAccessibility: false })
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const update = useUpdateHouseholdCircumstances(householdId)
  const { success } = useToast()

  const startEditing = () => {
    setForm({
      annualIncome: household.annualIncome ?? 0,
      requiresAccessibility: Boolean(household.accessibilityRequired),
    })
    setProblem(null)
    setEditing(true)
  }

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)
    try {
      await update.mutateAsync(form)
      success('Circumstances updated')
      setEditing(false)
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to update circumstances.'))
    }
  }

  return (
    <>
      <div className="detail-grid">
        <div className="detail-field">
          <div className="detail-field__label">Household Size</div>
          <div className="detail-field__value mono">{household.householdSize ?? '—'}</div>
        </div>
        <div className="detail-field">
          <div className="detail-field__label">Registered</div>
          <div className="detail-field__value mono">{fmtDate(household.registeredAt)}</div>
        </div>
        {!editing && (
          <>
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
          </>
        )}
      </div>

      {editing ? (
        <form aria-label="Edit Circumstances" onSubmit={e => { void handleSave(e) }} style={{ marginTop: 10 }}>
          <div className="form-grid">
            <div className="form-field">
              <label className="form-label">Annual Income ($)</label>
              <input
                className="form-input"
                type="number"
                min="0"
                step="1"
                value={form.annualIncome}
                onChange={e => setForm(f => ({ ...f, annualIncome: parseFloat(e.target.value) || 0 }))}
                required
              />
            </div>
            <div className="form-field full">
              <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <input
                  type="checkbox"
                  checked={form.requiresAccessibility}
                  onChange={e => setForm(f => ({ ...f, requiresAccessibility: e.target.checked }))}
                />
                Accessibility accommodations required
              </label>
            </div>
          </div>
          <div className="detail-field__label" style={{ marginTop: 8 }}>
            Applications already submitted keep the circumstances declared at submission.
          </div>
          <ApiProblemAlert problem={problem} />
          <div style={{ display: 'flex', gap: 8, marginTop: 8 }}>
            <button className="btn btn-primary btn-sm" type="submit" disabled={update.isPending}>
              {update.isPending ? 'Saving…' : 'Save Circumstances'}
            </button>
            <button className="btn btn-ghost btn-sm" type="button" onClick={() => setEditing(false)}>Cancel</button>
          </div>
        </form>
      ) : (
        <button className="btn btn-ghost btn-sm" type="button" style={{ marginTop: 10 }} onClick={startEditing}>
          Edit Circumstances
        </button>
      )}
    </>
  )
}

export default function HouseholdDetailPanel({ householdId, onClose }: Props) {
  const { data: household, isLoading, isError, error, refetch } = useHousehold(householdId)
  const startApplication = useStartApplication()
  const { success } = useToast()
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const [readinessPanel, setReadinessPanel] = useState<ReadinessPanel | null>(null)

  const status = getResponseStatus(error)
  const primary = household?.members?.find(m => m.isPrimary)
  const applications = household?.applications ?? []
  const openApplication = findOpenApplication(household)
  const prequalification = household?.mortgagePrequalification

  const handleStartApplication = async () => {
    setProblem(null)
    try {
      await startApplication.mutateAsync(householdId)
      success('Application submitted')
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to start application.'))
    }
  }

  return (
    <SlideOver
      title={primary?.name ?? 'Household'}
      subtitle={primary?.email ?? formatPhoneDisplay(primary?.phone)}
      onClose={onClose}
      footer={<button className="btn btn-ghost" onClick={onClose}>Close</button>}
    >
      {isLoading && <SkeletonRows />}

      {isError && (
        <div className="error-state">
          <div className="error-state__title">Could not load household</div>
          <div className="error-state__sub">
            {status === 404
              ? 'This household no longer exists or is inaccessible.'
              : 'Try again or close and reopen this panel.'}
          </div>
          <div style={{ marginTop: 10 }}>
            <button className="btn btn-ghost btn-sm" type="button" onClick={() => { void refetch() }}>Retry</button>
          </div>
        </div>
      )}

      {!isLoading && !isError && household && (
        <>
          <CircumstancesSection householdId={householdId} household={household} />

          <HouseholdMembersSection householdId={householdId} members={household.members ?? []} />

          <div className="detail-section" style={{ marginTop: 16 }}>
            <div className="detail-section__title">Buyer Preparation Checklist</div>
            <div className="checklist">
              <ReadinessItem
                label="Housing Counseling"
                done={Boolean(household.isHousingCounselingCompleted)}
                status={`Completed ${fmtDate(household.housingCounselingCompletedDate)}`}
                actionLabel={household.isHousingCounselingCompleted ? undefined : 'Record'}
                onAction={() => setReadinessPanel('housing-counseling')}
              />
              <ReadinessItem
                label="CLT Orientation"
                done={Boolean(household.isCltOrientationCompleted)}
                status={`Completed ${fmtDate(household.cltOrientationCompletedDate)}`}
                actionLabel={household.isCltOrientationCompleted ? undefined : 'Record'}
                onAction={() => setReadinessPanel('clt-orientation')}
              />
              <ReadinessItem
                label="Mortgage Prequalification"
                done={Boolean(prequalification)}
                status={prequalification
                  ? `${prequalification.lenderName} · ${fmtCurrency(prequalification.approvedAmount ?? 0)} · expires ${fmtDate(prequalification.expiryDate)}`
                  : undefined}
                actionLabel={prequalification ? 'Update' : 'Record'}
                onAction={() => setReadinessPanel('mortgage-prequalification')}
              />
            </div>
          </div>

          <div className="detail-section" style={{ marginTop: 16 }}>
            <div className="detail-section__title">Applications</div>
            {applications.length === 0 && (
              <div className="detail-field__value">No applications yet.</div>
            )}
            {applications.length > 0 && (
              <table className="registry-table" style={{ marginTop: 10 }}>
                <thead>
                  <tr>
                    <th>Phase</th>
                    <th>Submitted</th>
                    <th />
                  </tr>
                </thead>
                <tbody>
                  {applications.map(application => (
                    <tr key={application.id}>
                      <td><ApplicationPhaseBadge phase={application.phase} /></td>
                      <td><span className="cell-mono">{fmtDate(application.submittedAt)}</span></td>
                      <td style={{ textAlign: 'right' }}>
                        <NavIdLink id={application.id} to="/applications" label="Open" />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}

            <div style={{ marginTop: 12 }}>
              <ApiProblemAlert problem={problem} />
              {openApplication ? (
                <NavIdLink id={openApplication.id} to="/applications" label="Open Active Application" />
              ) : (
                <button
                  className="btn btn-primary btn-sm"
                  type="button"
                  onClick={() => { void handleStartApplication() }}
                  disabled={startApplication.isPending}
                >
                  {startApplication.isPending ? 'Submitting…' : 'Start Application'}
                </button>
              )}
            </div>
          </div>
        </>
      )}

      {readinessPanel === 'housing-counseling' && (
        <RecordHousingCounselingPanel householdId={householdId} onClose={() => setReadinessPanel(null)} />
      )}
      {readinessPanel === 'clt-orientation' && (
        <RecordCltOrientationPanel householdId={householdId} onClose={() => setReadinessPanel(null)} />
      )}
      {readinessPanel === 'mortgage-prequalification' && (
        <RecordMortgagePrequalificationPanel householdId={householdId} onClose={() => setReadinessPanel(null)} />
      )}
    </SlideOver>
  )
}
