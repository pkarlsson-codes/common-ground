import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import NavIdLink from '../../components/NavIdLink'
import SlideOver from '../../components/SlideOver'
import { useHousehold, useStartApplication } from '../../hooks/households'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, getResponseStatus, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { formatPhoneDisplay } from '../../lib/phone'
import ApplicationPhaseBadge from '../applications/ApplicationPhaseBadge'

type Props = {
  householdId: string
  onClose: () => void
}

const OPEN_PHASES = ['Submitted', 'Approved']

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

function ReadinessItem({ label, done, status }: { label: string; done: boolean; status?: string }) {
  return (
    <div className={`checklist-item${done ? ' checklist-item--done' : ''}`}>
      <div className={`checklist-item__icon${done ? ' checklist-item__icon--done' : ' checklist-item__icon--pending'}`}>
        {done ? '✓' : '○'}
      </div>
      <span className="checklist-item__label">{label}</span>
      {done && status && <span className="checklist-item__status">{status}</span>}
    </div>
  )
}

export default function HouseholdDetailPanel({ householdId, onClose }: Props) {
  const { data: household, isLoading, isError, error, refetch } = useHousehold(householdId)
  const startApplication = useStartApplication()
  const { success } = useToast()
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)

  const status = getResponseStatus(error)
  const primary = household?.members?.find(m => m.isPrimary)
  const applications = household?.applications ?? []
  const openApplication = applications.find(a => OPEN_PHASES.includes(a.phase ?? ''))
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
          <div className="detail-grid">
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
            <div className="detail-field">
              <div className="detail-field__label">Registered</div>
              <div className="detail-field__value mono">{fmtDate(household.registeredAt)}</div>
            </div>
          </div>

          <div className="detail-section" style={{ marginTop: 16 }}>
            <div className="detail-section__title">Household Members</div>
            <div className="detail-grid" style={{ marginTop: 10 }}>
              {(household.members ?? []).map(member => (
                <div className="detail-field full-span" key={member.id}>
                  <div className="detail-field__label">{member.isPrimary ? 'Primary Contact' : 'Member'}</div>
                  <div className="detail-field__value">
                    {[member.name, member.email, formatPhoneDisplay(member.phone)].filter(Boolean).join(' · ')}
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="detail-section" style={{ marginTop: 16 }}>
            <div className="detail-section__title">Buyer Preparation Checklist</div>
            <div className="checklist">
              <ReadinessItem
                label="Housing Counseling"
                done={Boolean(household.isHousingCounselingCompleted)}
                status={`Completed ${fmtDate(household.housingCounselingCompletedDate)}`}
              />
              <ReadinessItem
                label="CLT Orientation"
                done={Boolean(household.isCltOrientationCompleted)}
                status={`Completed ${fmtDate(household.cltOrientationCompletedDate)}`}
              />
              <ReadinessItem
                label="Mortgage Prequalification"
                done={Boolean(prequalification)}
                status={prequalification
                  ? `${prequalification.lenderName} · ${fmtCurrency(prequalification.approvedAmount ?? 0)} · expires ${fmtDate(prequalification.expiryDate)}`
                  : undefined}
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
    </SlideOver>
  )
}
