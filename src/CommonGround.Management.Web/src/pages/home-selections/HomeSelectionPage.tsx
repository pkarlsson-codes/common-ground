import { useState } from 'react'
import { useParams } from 'react-router-dom'
import type {
  HomeSelectionDecisionApplicationDto,
  HomeSelectionDecisionDto,
  HomeSelectionOverviewResponse,
  WaitingListComparisonEntryDto,
} from '../../api'
import NavIdLink from '../../components/NavIdLink'
import StatusBadge from '../../components/StatusBadge'
import UserName from '../../components/UserName'
import { useHome } from '../../hooks/homes'
import { cannotBeChosenReasonLabel, selectionPriorityLabel, useHomeSelectionOverview } from '../../hooks/homeSelections'
import { formatCalendarDate, numericDate, type CalendarDate } from '../../lib/calendarDate'
import { getResponseStatus } from '../../lib/getErrorMessage'
import { statusLabel } from '../../utils/statusLabel'
import DrawLotteryPanel from './DrawLotteryPanel'
import SelectByPriorityPanel from './SelectByPriorityPanel'

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

const fmtDate = (value?: CalendarDate | null) => (value ? formatCalendarDate(value, numericDate) : '—')

type ActionPanel =
  | { kind: 'select-by-priority'; entry: WaitingListComparisonEntryDto }
  | { kind: 'draw-lottery'; tied: string[] | null }
  | null

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

const homeDescription = (overview: HomeSelectionOverviewResponse) => {
  const size = overview.homeSquareFootage != null ? ` · ${Number(overview.homeSquareFootage).toLocaleString()} sq ft` : ''
  const baths = overview.homeBathrooms != null ? ` · ${Number(overview.homeBathrooms)} ba` : ''
  return `${overview.homeBedrooms} bd${baths}${size}${overview.homeIsAccessible ? ' · Accessible' : ' · Not accessible'}`
}

const incomeLimitDescription = (percentOfAmi?: number | null, effectiveFrom?: CalendarDate | null) => {
  if (percentOfAmi == null) return 'No income limit'
  const effective = effectiveFrom ? `, limits effective ${formatCalendarDate(effectiveFrom)}` : ''
  return `${percentOfAmi}% of AMI${effective}`
}

type Household = {
  annualIncome?: number | null
  householdSize?: number | null
  incomeLimit?: number | null
  requiresAccessibility?: boolean | null
  primaryContact?: { name?: string | null; email?: string | null } | null
}

function IncomeCell({ household }: { household: Household }) {
  const income = fmtCurrency(Number(household.annualIncome ?? 0))
  return (
    <>
      <span className="cell-mono">
        {household.incomeLimit == null ? income : `${income} of ${fmtCurrency(Number(household.incomeLimit))}`}
      </span>
      <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}>
        {household.householdSize} {household.householdSize === 1 ? 'person' : 'people'}
        {household.incomeLimit == null && ' · no limit'}
      </div>
    </>
  )
}

function ApplicantCell({ household, homeIsAccessible }: { household: Household; homeIsAccessible: boolean }) {
  return (
    <>
      <div>{household.primaryContact?.name ?? '—'}</div>
      {household.primaryContact?.email && (
        <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}>{household.primaryContact.email}</div>
      )}
      {household.requiresAccessibility && (
        <div>
          {homeIsAccessible
            ? <span className="badge badge-default">Needs accessible home</span>
            : <span className="badge badge-overincomelimit">Needs accessible home · home is not accessible</span>}
        </div>
      )}
    </>
  )
}

function Reasons({ reasons }: { reasons?: string[] | null }) {
  return <>{(reasons ?? []).map(reason => <div key={reason}>{cannotBeChosenReasonLabel(reason)}</div>)}</>
}

const notOfferedMessage = (saleStage: string) =>
  saleStage === 'UnderContract'
    ? 'A household has been chosen for this home. A new household can be chosen only if its purchase falls through.'
    : `This home is ${statusLabel(saleStage).toLowerCase()}, so a household can't be chosen for it now.`

function WaitingList({
  overview,
  onSelect,
  onDraw,
}: {
  overview: HomeSelectionOverviewResponse
  onSelect: (entry: WaitingListComparisonEntryDto) => void
  onDraw: (tied: string[] | null) => void
}) {
  const [tied, setTied] = useState<string[]>([])
  const entries = overview.waitingList ?? []
  const canChoose = overview.isOffered && overview.hasIncomeLimitsInEffect && entries.length > 0

  const toggleTied = (applicationId: string, checked: boolean) =>
    setTied(t => (checked ? [...t, applicationId] : t.filter(id => id !== applicationId)))

  return (
    <div className="detail-section" style={{ marginBottom: 16 }}>
      <div className="detail-section__title">Waiting List Today</div>
      {!overview.isOffered && (
        <div className="detail-field__value" style={{ marginBottom: 12 }}>{notOfferedMessage(overview.saleStage ?? '')}</div>
      )}
      {overview.isOffered && !overview.hasIncomeLimitsInEffect && (
        <div className="form-warning" role="status" style={{ marginBottom: 12 }}>
          No {overview.incomeLimitPercentOfAmi}% of AMI income limits are in effect. Enter them under Income Limits before choosing a household.
        </div>
      )}
      {canChoose && (
        <div className="filter-bar" style={{ alignItems: 'center' }}>
          <button
            type="button"
            className="btn btn-secondary btn-sm"
            disabled={tied.length === 1}
            onClick={() => onDraw(tied.length >= 2 ? tied : null)}
          >
            {tied.length >= 2 ? `Draw Lottery Between ${tied.length} Tied` : 'Draw Lottery From All'}
          </button>
          <span style={{ fontSize: 13, color: 'var(--text-secondary)' }}>
            Compare the households, then choose one by priority or draw a lottery. Mark two or more as tied to draw only
            between them. The whole waiting list is recorded with the choice.
          </span>
        </div>
      )}

      {entries.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state__title">Nobody is on the waiting list</div>
          <div className="empty-state__sub">Approved applications join the waiting list.</div>
        </div>
      ) : (
        <div className="registry-table-wrap">
          <table className="registry-table">
            <thead>
              <tr>
                {canChoose && <th>Tied</th>}
                <th>Applicant</th>
                <th title="Income and household size as on the application, which follows the household">Income (Application)</th>
                <th>Prequalified</th>
                <th>Waiting Since</th>
                <th>Can Be Chosen</th>
                {canChoose && <th />}
              </tr>
            </thead>
            <tbody>
              {entries.map(entry => {
                const applicationId = entry.applicationId!
                return (
                  <tr key={applicationId}>
                    {canChoose && (
                      <td>
                        <input
                          type="checkbox"
                          aria-label={`Mark ${entry.primaryContact?.name ?? applicationId} as tied`}
                          checked={tied.includes(applicationId)}
                          onChange={e => toggleTied(applicationId, e.target.checked)}
                        />
                      </td>
                    )}
                    <td><ApplicantCell household={entry} homeIsAccessible={!!overview.homeIsAccessible} /></td>
                    <td><IncomeCell household={entry} /></td>
                    <td>
                      {entry.prequalifiedAmount != null
                        ? <span className="cell-mono">{fmtCurrency(Number(entry.prequalifiedAmount))}</span>
                        : 'None'}
                    </td>
                    <td><span className="cell-mono">{fmtDate(entry.waitingSince)}</span></td>
                    <td>{entry.canBeChosen ? 'Yes' : <Reasons reasons={entry.reasons} />}</td>
                    {canChoose && (
                      <td style={{ textAlign: 'right' }}>
                        <button
                          type="button"
                          className="btn btn-secondary btn-sm"
                          disabled={!entry.canBeChosen}
                          onClick={() => onSelect(entry)}
                        >
                          Choose
                        </button>
                      </td>
                    )}
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}

function DecisionSummary({ decision }: { decision: HomeSelectionDecisionDto }) {
  const isLottery = decision.selectionMethod === 'Lottery'
  const leftOut = (decision.lotteryTiedApplicationIds ?? []).length - (decision.lotteryPoolApplicationIds ?? []).length
  return (
    <div className="detail-grid">
      <div className="detail-field">
        <div className="detail-field__label">Method</div>
        <div className="detail-field__value">{isLottery ? 'Lottery' : 'Priority'}</div>
      </div>
      {isLottery ? (
        <>
          <div className="detail-field">
            <div className="detail-field__label">Drawn From</div>
            <div className="detail-field__value mono">
              {decision.lotteryPoolApplicationIds?.length ?? 0} household{decision.lotteryPoolApplicationIds?.length !== 1 ? 's' : ''}
              {decision.lotteryTiedApplicationIds ? ' found tied' : ' on the waiting list'}
              {leftOut > 0 && ` · ${leftOut} tied left out`}
            </div>
          </div>
          <div className="detail-field">
            <div className="detail-field__label">Seed</div>
            <div className="detail-field__value mono">{decision.lotterySeed ?? '—'}</div>
          </div>
        </>
      ) : (
        <>
          <div className="detail-field">
            <div className="detail-field__label">Priorities</div>
            <div className="detail-field__value">
              {(decision.priorities ?? []).map(selectionPriorityLabel).join(', ') || '—'}
            </div>
          </div>
          <div className="detail-field full-span">
            <div className="detail-field__label">Reason</div>
            <div className="detail-field__value">{decision.selectionReason ?? '—'}</div>
          </div>
        </>
      )}
      <div className="detail-field">
        <div className="detail-field__label">Chosen On</div>
        <div className="detail-field__value mono">{fmtDate(decision.selectedDate)}</div>
      </div>
      <div className="detail-field">
        <div className="detail-field__label">Chosen By</div>
        <div className="detail-field__value"><UserName id={decision.selectedBy} /></div>
      </div>
      <div className="detail-field">
        <div className="detail-field__label">Purchase Price</div>
        <div className="detail-field__value mono">{fmtCurrency(Number(decision.purchasePrice ?? 0))}</div>
      </div>
      <div className="detail-field">
        <div className="detail-field__label">Income Limit</div>
        <div className="detail-field__value">{incomeLimitDescription(decision.incomeLimitPercentOfAmi, decision.incomeLimitsEffectiveFrom)}</div>
      </div>
    </div>
  )
}

function Decision({ decision, homeIsAccessible }: { decision: HomeSelectionDecisionDto; homeIsAccessible: boolean }) {
  const chosen = decision.applications?.find(a => a.outcome === 'Chosen')
  const applications: HomeSelectionDecisionApplicationDto[] = decision.applications ?? []
  return (
    <div className="detail-section" style={{ marginBottom: 16 }}>
      <div className="detail-section__title">
        {chosen?.primaryContact?.name ?? 'Household'} chosen {fmtDate(decision.selectedDate)}
      </div>
      <DecisionSummary decision={decision} />
      <div className="registry-table-wrap" style={{ marginTop: 12 }}>
        <table className="registry-table">
          <thead>
            <tr>
              <th>Applicant</th>
              <th title="Income and household size as on the application, which follows the household">Income (Application)</th>
              <th>Waiting Since</th>
              <th>Outcome</th>
            </tr>
          </thead>
          <tbody>
            {applications.map(a => (
              <tr key={a.applicationId} className={a.outcome === 'Chosen' ? 'row--selected' : ''}>
                <td><ApplicantCell household={a} homeIsAccessible={homeIsAccessible} /></td>
                <td><IncomeCell household={a} /></td>
                <td><span className="cell-mono">{fmtDate(a.waitingSince)}</span></td>
                <td>
                  <StatusBadge status={a.outcome ?? ''} />
                  {a.outcome === 'CouldNotBeChosen' && (
                    <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}><Reasons reasons={a.reasons} /></div>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default function HomeSelectionPage() {
  const { homeId = '' } = useParams()
  const { data: overview, isLoading, isError, error, refetch } = useHomeSelectionOverview(homeId)
  const { data: home } = useHome(homeId)
  // Undefined while loading, null when the home has no assessment on record.
  const assessedValue = home ? (home.assessedValue ? Number(home.assessedValue) : null) : undefined
  const [actionPanel, setActionPanel] = useState<ActionPanel>(null)

  const status = getResponseStatus(error)
  const waitingCount = overview?.waitingList?.length ?? 0

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Home Selection</div>
          <h1 className="page-header__title">{overview ? overview.homeLabel || homeDescription(overview) : '…'}</h1>
          {overview && (
            <div className="page-header__count">
              {homeDescription(overview)} · {incomeLimitDescription(overview.incomeLimitPercentOfAmi, overview.incomeLimitsEffectiveFrom)} · {waitingCount} on the waiting list
            </div>
          )}
        </div>
        {overview && (
          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <StatusBadge status={overview.isOffered ? 'Available' : overview.saleStage ?? ''} />
            <NavIdLink id={homeId} to="/homes" label="Open Home" />
          </div>
        )}
      </div>

      {isLoading && <SkeletonRows />}

      {isError && (
        <div className="error-state">
          <div className="error-state__title">Could not load the selection</div>
          <div className="error-state__sub">
            {status === 404 ? 'This home does not exist.' : 'Try again or reopen it from the home.'}
          </div>
          <div style={{ marginTop: 10 }}>
            <button className="btn btn-ghost btn-sm" type="button" onClick={() => { void refetch() }}>Retry</button>
          </div>
        </div>
      )}

      {overview && (
        <>
          <WaitingList
            overview={overview}
            onSelect={entry => setActionPanel({ kind: 'select-by-priority', entry })}
            onDraw={tied => setActionPanel({ kind: 'draw-lottery', tied })}
          />
          {(overview.decisions ?? []).map(decision => (
            <Decision key={decision.id} decision={decision} homeIsAccessible={!!overview.homeIsAccessible} />
          ))}
        </>
      )}

      {actionPanel?.kind === 'select-by-priority' && (
        <SelectByPriorityPanel
          homeId={homeId}
          entry={actionPanel.entry}
          assessedValue={assessedValue}
          onClose={() => setActionPanel(null)}
        />
      )}

      {actionPanel?.kind === 'draw-lottery' && (
        <DrawLotteryPanel
          homeId={homeId}
          tiedApplicationIds={actionPanel.tied}
          waitingCount={waitingCount}
          assessedValue={assessedValue}
          onClose={() => setActionPanel(null)}
        />
      )}
    </div>
  )
}
