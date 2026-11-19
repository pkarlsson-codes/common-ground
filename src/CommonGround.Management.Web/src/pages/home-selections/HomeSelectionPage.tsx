import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { formatStepDate } from '../../lib/lifecycle'
import type { HomeSelectionCandidateDto, HomeSelectionResponse } from '../../api'
import NavIdLink from '../../components/NavIdLink'
import StatusBadge from '../../components/StatusBadge'
import { useHome } from '../../hooks/homes'
import { selectionPriorityLabel, useHomeSelection } from '../../hooks/homeSelections'
import { formatCalendarDate, formatInstant, numericDate, type CalendarDate } from '../../lib/calendarDate'
import { getResponseStatus } from '../../lib/getErrorMessage'
import CancelSelectionPanel from './CancelSelectionPanel'
import DrawLotteryPanel from './DrawLotteryPanel'
import SelectByPriorityPanel from './SelectByPriorityPanel'

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

const fmtDateTime = (value?: Date | null) => (value ? formatInstant(value) : '—')

const fmtDate = (value?: CalendarDate | null) => (value ? formatCalendarDate(value, numericDate) : '—')

type ActionPanel =
  | { kind: 'select-by-priority'; candidate: HomeSelectionCandidateDto }
  | { kind: 'draw-lottery' }
  | { kind: 'cancel' }
  | null

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

const homeDescription = (selection: HomeSelectionResponse) => {
  if (selection.homeBedrooms == null) return 'Home'
  const size = selection.homeSquareFootage != null ? ` · ${Number(selection.homeSquareFootage).toLocaleString()} sq ft` : ''
  const baths = selection.homeBathrooms != null ? ` · ${Number(selection.homeBathrooms)} ba` : ''
  return `${selection.homeBedrooms} bd${baths}${size}${selection.homeIsAccessible ? ' · Accessible' : ' · Not accessible'}`
}

const incomeLimitDescription = (selection: HomeSelectionResponse) => {
  if (selection.incomeLimitPercentOfAmi == null) return 'No income limit'
  const effective = selection.incomeLimitsEffectiveFrom
    ? `, limits effective ${formatCalendarDate(selection.incomeLimitsEffectiveFrom)}`
    : ''
  return `${selection.incomeLimitPercentOfAmi}% of AMI${effective}`
}

function IncomeCell({ candidate }: { candidate: HomeSelectionCandidateDto }) {
  const income = fmtCurrency(Number(candidate.annualIncome ?? 0))
  return (
    <>
      <span className="cell-mono">
        {candidate.incomeLimit == null ? income : `${income} of ${fmtCurrency(Number(candidate.incomeLimit))}`}
      </span>
      <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}>
        {candidate.householdSize} {candidate.householdSize === 1 ? 'person' : 'people'}
        {candidate.incomeLimit == null && ' · no limit'}
      </div>
    </>
  )
}

function Outcome({ selection }: { selection: HomeSelectionResponse }) {
  if (selection.status === 'Cancelled') {
    return (
      <div className="detail-grid">
        <div className="detail-field full-span">
          <div className="detail-field__label">Cancellation Reason</div>
          <div className="detail-field__value">{selection.cancellationReason ?? '—'}</div>
        </div>
        <div className="detail-field">
          <div className="detail-field__label">Cancelled By</div>
          <div className="detail-field__value">{selection.closedBy ?? '—'}</div>
        </div>
        <div className="detail-field">
          <div className="detail-field__label">Cancelled At</div>
          <div className="detail-field__value mono">{fmtDateTime(selection.closedAt)}</div>
        </div>
      </div>
    )
  }

  const isLottery = selection.selectionMethod === 'Lottery'
  return (
    <div className="detail-grid">
      <div className="detail-field">
        <div className="detail-field__label">Method</div>
        <div className="detail-field__value">{selection.selectionMethod ?? '—'}</div>
      </div>
      {isLottery ? (
        <>
          <div className="detail-field">
            <div className="detail-field__label">Seed</div>
            <div className="detail-field__value mono">{selection.lotterySeed ?? '—'}</div>
          </div>
          <div className="detail-field">
            <div className="detail-field__label">Drawn From</div>
            <div className="detail-field__value mono">{selection.lotteryPoolApplicationIds?.length ?? 0} candidates</div>
          </div>
          <div className="detail-field">
            <div className="detail-field__label">Left Out of Draw</div>
            <div className="detail-field__value mono">{selection.lotteryIneligibleApplicationIds?.length ?? 0} candidates</div>
          </div>
        </>
      ) : (
        <>
          <div className="detail-field">
            <div className="detail-field__label">Priorities</div>
            <div className="detail-field__value">
              {(selection.priorities ?? []).map(selectionPriorityLabel).join(', ') || '—'}
            </div>
          </div>
          <div className="detail-field full-span">
            <div className="detail-field__label">Reason</div>
            <div className="detail-field__value">{selection.selectionReason ?? '—'}</div>
          </div>
        </>
      )}
      <div className="detail-field">
        <div className="detail-field__label">Selected By</div>
        <div className="detail-field__value">{selection.closedBy ?? '—'}</div>
      </div>
      <div className="detail-field">
        <div className="detail-field__label">Selected At</div>
        <div className="detail-field__value mono">{fmtDateTime(selection.closedAt)}</div>
      </div>
      {selection.status === 'FellThrough' && (
        <div className="detail-field full-span">
          <div className="detail-field__label">Purchase fell through</div>
          <div className="detail-field__value">
            {formatStepDate(selection.fellThroughDate)}{selection.fellThroughReason ? ` · ${selection.fellThroughReason}` : ''}
          </div>
        </div>
      )}
    </div>
  )
}

function CandidateOutcome({ selection, candidate }: { selection: HomeSelectionResponse; candidate: HomeSelectionCandidateDto }) {
  const applicationId = candidate.applicationId!
  if (selection.selectedApplicationId === applicationId) {
    return <span className="badge badge-closed">Selected</span>
  }
  if (candidate.isOverIncomeLimit) {
    return <span className="badge badge-overincomelimit">Over income limit</span>
  }
  if (selection.lotteryIneligibleApplicationIds?.includes(applicationId)) {
    return <span className="badge badge-default">Left out of draw</span>
  }
  return null
}

export default function HomeSelectionPage() {
  const { selectionId = '' } = useParams()
  const { data: selection, isLoading, isError, error, refetch } = useHomeSelection(selectionId)
  const { data: home } = useHome(selection?.homeId ?? null)
  // Undefined while loading, null when the home has no assessment on record.
  const assessedValue = home ? (home.assessedValue ? Number(home.assessedValue) : null) : undefined
  const [tied, setTied] = useState<string[]>([])
  const [actionPanel, setActionPanel] = useState<ActionPanel>(null)

  const status = getResponseStatus(error)
  const isOpen = selection?.status === 'Open'
  const candidates = selection?.candidates ?? []

  const toggleTied = (applicationId: string, checked: boolean) =>
    setTied(t => (checked ? [...t, applicationId] : t.filter(id => id !== applicationId)))

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Home Selection</div>
          <h1 className="page-header__title">{selection ? selection.homeLabel || homeDescription(selection) : '…'}</h1>
          {selection && (
            <div className="page-header__count">
              {selection.homeLabel && `${homeDescription(selection)} · `}Started {fmtDateTime(selection.startedAt)} by {selection.startedBy} · {incomeLimitDescription(selection)} · {candidates.length} candidate{candidates.length !== 1 ? 's' : ''}
            </div>
          )}
        </div>
        {selection && (
          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <StatusBadge status={selection.status ?? ''} />
            <NavIdLink id={selection.homeId} to="/homes" label="Open Home" />
          </div>
        )}
      </div>

      {isLoading && <SkeletonRows />}

      {isError && (
        <div className="error-state">
          <div className="error-state__title">Could not load the selection</div>
          <div className="error-state__sub">
            {status === 404 ? 'This selection does not exist.' : 'Try again or reopen it from the home.'}
          </div>
          <div style={{ marginTop: 10 }}>
            <button className="btn btn-ghost btn-sm" type="button" onClick={() => { void refetch() }}>Retry</button>
          </div>
        </div>
      )}

      {selection && (
        <>
          {isOpen ? (
            <div className="filter-bar" style={{ alignItems: 'center' }}>
              <button
                type="button"
                className="btn btn-secondary btn-sm"
                disabled={tied.length === 1}
                onClick={() => setActionPanel({ kind: 'draw-lottery' })}
              >
                {tied.length >= 2 ? `Draw Lottery Between ${tied.length} Tied` : 'Draw Lottery From All'}
              </button>
              <button type="button" className="btn btn-ghost btn-sm" onClick={() => setActionPanel({ kind: 'cancel' })}>
                Cancel Selection
              </button>
              <span style={{ fontSize: 13, color: 'var(--text-secondary)' }}>
                Compare the candidates’ facts, then select one by priority or draw a lottery. Mark two or more as tied
                to draw only between them.
              </span>
            </div>
          ) : (
            <div className="detail-section" style={{ marginBottom: 16 }}>
              <div className="detail-section__title">Outcome</div>
              <Outcome selection={selection} />
            </div>
          )}

          <div className="registry-table-wrap">
            <table className="registry-table">
              <thead>
                <tr>
                  {isOpen && <th>Tied</th>}
                  <th>Applicant</th>
                  <th>Income</th>
                  <th>Prequalified</th>
                  <th>Waiting Since</th>
                  <th>Status</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {candidates.map(candidate => {
                  const applicationId = candidate.applicationId!
                  return (
                    <tr
                      key={applicationId}
                      className={selection.selectedApplicationId === applicationId ? 'row--selected' : ''}
                    >
                      {isOpen && (
                        <td>
                          <input
                            type="checkbox"
                            aria-label={`Mark ${candidate.primaryContact?.name ?? applicationId} as tied`}
                            checked={tied.includes(applicationId)}
                            onChange={e => toggleTied(applicationId, e.target.checked)}
                          />
                        </td>
                      )}
                      <td>
                        <div>{candidate.primaryContact?.name ?? '—'}</div>
                        {candidate.primaryContact?.email && (
                          <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}>{candidate.primaryContact.email}</div>
                        )}
                        {candidate.requiresAccessibility && (
                          <div>
                            {selection.homeIsAccessible === false
                              ? <span className="badge badge-overincomelimit">Needs accessible home · home is not accessible</span>
                              : <span className="badge badge-default">Needs accessible home</span>}
                          </div>
                        )}
                      </td>
                      <td><IncomeCell candidate={candidate} /></td>
                      <td>
                        {candidate.prequalifiedAmount != null
                          ? <span className="cell-mono">{fmtCurrency(Number(candidate.prequalifiedAmount))}</span>
                          : 'None'}
                      </td>
                      <td><span className="cell-mono">{fmtDate(candidate.waitingSince)}</span></td>
                      <td>
                        {candidate.canBeSelected
                          ? 'Can be selected'
                          : (candidate.notSelectableReasons ?? []).map(reason => <div key={reason}>{reason}</div>)}
                      </td>
                      <td style={{ textAlign: 'right' }}>
                        {isOpen ? (
                          <button
                            type="button"
                            className="btn btn-secondary btn-sm"
                            disabled={!candidate.canBeSelected || candidate.isOverIncomeLimit}
                            onClick={() => setActionPanel({ kind: 'select-by-priority', candidate })}
                          >
                            Select
                          </button>
                        ) : (
                          <CandidateOutcome selection={selection} candidate={candidate} />
                        )}
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        </>
      )}

      {actionPanel?.kind === 'select-by-priority' && (
        <SelectByPriorityPanel
          selectionId={selectionId}
          candidate={actionPanel.candidate}
          assessedValue={assessedValue}
          onClose={() => setActionPanel(null)}
        />
      )}

      {actionPanel?.kind === 'draw-lottery' && (
        <DrawLotteryPanel
          selectionId={selectionId}
          tiedApplicationIds={tied.length >= 2 ? tied : null}
          candidateCount={candidates.length}
          assessedValue={assessedValue}
          onClose={() => setActionPanel(null)}
        />
      )}

      {actionPanel?.kind === 'cancel' && (
        <CancelSelectionPanel selectionId={selectionId} onClose={() => setActionPanel(null)} />
      )}
    </div>
  )
}
