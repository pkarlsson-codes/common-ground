import { useState } from 'react'
import type { ResaleResponse } from '../../api'
import ConnectedRecords, { type ConnectedRecord } from '../../components/lifecycle/ConnectedRecords'
import LifecycleStepper from '../../components/lifecycle/LifecycleStepper'
import NextStepCard, { type NextStepAction } from '../../components/lifecycle/NextStepCard'
import SlideOver from '../../components/SlideOver'
import { useGroundLease } from '../../hooks/groundLeases'
import { NOTICE_CHANNEL_LABELS, optionWindowLeft, useResale } from '../../hooks/resales'
import { formatPrice } from '../../lib/currency'
import { daysBetween, formatInstantDate, trustToday } from '../../lib/calendarDate'
import { formatStepDate, lifecycleSteps } from '../../lib/lifecycle'
import ExercisePurchaseOptionPanel from './ExercisePurchaseOptionPanel'
import PayoffStatementPanel from './PayoffStatementPanel'
import RecordBuybackClosedPanel from './RecordBuybackClosedPanel'
import RecordResaleAppraisalPanel from './RecordResaleAppraisalPanel'
import WithdrawResalePanel from './WithdrawResalePanel'

type ActionPanel = 'appraisal' | 'exercise' | 'closing' | 'withdraw' | 'payoff' | null

const STEPS = ['Notice', 'Appraisal', 'Decision', 'Closing'] as const

interface Props {
  resaleId: string
  onClose: () => void
}

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

function Field({ label, children, full }: { label: string; children: React.ReactNode; full?: boolean }) {
  return (
    <div className={`detail-field${full ? ' full-span' : ''}`}>
      <div className="detail-field__label">{label}</div>
      <div className="detail-field__value">{children}</div>
    </div>
  )
}

/** The step the tracker highlights: the one the resale is waiting on, or none once the buyback closed. */
function currentStep(resale: ResaleResponse) {
  if (resale.optionExercisedDate) return 'Closing'
  if (resale.appraisedValue != null) return 'Decision'
  return 'Appraisal'
}

/** What the resale is waiting on, in one sentence, and the button that moves it on. */
function nextStep(resale: ResaleResponse, open: (panel: ActionPanel) => void): { title: string; primary?: NextStepAction } {
  switch (resale.status) {
    case 'Withdrawn':
      return { title: `The homeowner withdrew the notice to sell on ${formatStepDate(resale.withdrawnDate)}.` }
    case 'Closed':
      return { title: `The trust bought this home on ${formatStepDate(resale.closingDate)}.` }
    default:
  }

  if (resale.optionExercisedDate) {
    return {
      title: 'Record the closing when the trust has paid the Purchase Option Price.',
      primary: { label: 'Record Closing', onClick: () => open('closing') },
    }
  }

  if (resale.appraisedValue != null) {
    const passed = daysBetween(trustToday(), resale.optionWindowEndsOn!) < 0
    return {
      title: passed
        ? 'The option window has passed. Decide whether the trust buys this home.'
        : `Decide whether the trust buys this home by ${formatStepDate(resale.optionWindowEndsOn)}.`,
      primary: { label: 'Buy the Home', onClick: () => open('exercise') },
    }
  }

  return { title: 'Record the appraisal.', primary: { label: 'Record Appraisal', onClick: () => open('appraisal') } }
}

/** One resale: where it stands, what it waits on, and the notice, appraisal, price and closing. */
export default function ResaleDetailPanel({ resaleId, onClose }: Props) {
  const { data: resale, isLoading } = useResale(resaleId)
  const { data: lease } = useGroundLease(resale?.groundLeaseId ?? null)
  const [actionPanel, setActionPanel] = useState<ActionPanel>(null)

  if (isLoading || !resale) {
    return (
      <SlideOver title="…" onClose={onClose}>
        <SkeletonRows />
      </SlideOver>
    )
  }

  const closed = resale.status === 'Closed'
  const open = resale.status === 'Open'
  const current = currentStep(resale)
  const steps = lifecycleSteps(STEPS, current, {
    Notice: formatStepDate(resale.noticeDate),
    Appraisal: formatStepDate(resale.appraisalDate),
    Decision: formatStepDate(resale.optionExercisedDate),
    Closing: formatStepDate(resale.closingDate),
  }).map(step => (closed ? { ...step, state: 'done' as const, date: step.date } : step))
  const next = nextStep(resale, setActionPanel)
  const canWithdraw = open && !resale.optionExercisedDate

  const records: ConnectedRecord[] = [
    {
      key: `lease-${resale.groundLeaseId}`,
      type: 'Ground lease',
      name: resale.homeownerFullName ?? 'Ground lease',
      status: lease?.status ?? undefined,
      to: '/ground-leases',
      openId: resale.groundLeaseId!,
    },
    {
      key: `home-${resale.homeId}`,
      type: 'Home',
      name: resale.homeLabel ?? 'Home',
      to: '/homes',
      openId: resale.homeId!,
    },
  ]

  return (
    <>
      <SlideOver
        title={resale.homeLabel ?? 'Resale'}
        subtitle={resale.homeownerFullName ?? undefined}
        onClose={onClose}
      >
        <LifecycleStepper steps={steps} label="Resale progress" />
        <NextStepCard
          title={next.title}
          primary={next.primary}
          moreActions={[
            ...(open ? [{ label: 'Payoff Statement', onClick: () => setActionPanel('payoff') }] : []),
            ...(canWithdraw ? [{ label: 'Withdraw', onClick: () => setActionPanel('withdraw') }] : []),
          ]}
        />

        <div className="detail-section">
          <div className="detail-section__title">Notice</div>
          <div className="detail-grid">
            <Field label="Notice Date"><span className="mono">{resale.noticeDate}</span></Field>
            <Field label="Notice Came By">{NOTICE_CHANNEL_LABELS[resale.noticeChannel ?? ''] ?? resale.noticeChannel}</Field>
            <Field label="Option Window End"><span className="mono">{resale.optionWindowEndsOn}</span></Field>
            {open && !resale.optionExercisedDate && resale.appraisedValue == null && (
              <Field label="Option Window">{optionWindowLeft(resale.optionWindowEndsOn!)}</Field>
            )}
            {open && resale.appraisedValue != null && !resale.optionExercisedDate && (
              <Field label="Option Window">{optionWindowLeft(resale.optionWindowEndsOn!)}</Field>
            )}
            {resale.notes && <Field label="Notes" full>{resale.notes}</Field>}
          </div>
        </div>

        {resale.appraisedValue != null && (
          <>
            <div className="detail-section">
              <div className="detail-section__title">Appraisal</div>
              <div className="detail-grid">
                <Field label="Appraised Value"><span className="mono">{formatPrice(resale.appraisedValue)}</span></Field>
                <Field label="Appraisal Date"><span className="mono">{resale.appraisalDate}</span></Field>
                <Field label="Appraiser">{resale.appraiserName}</Field>
                {resale.approvedImprovements != null && (
                  <Field label="Approved Improvements"><span className="mono">{formatPrice(resale.approvedImprovements)}</span></Field>
                )}
                {resale.currentIndexValue != null && (
                  <Field label="Current Index Value"><span className="mono">{resale.currentIndexValue}</span></Field>
                )}
              </div>
              {open && !resale.optionExercisedDate && (
                <button type="button" className="btn btn-ghost btn-sm" style={{ marginTop: 8 }} onClick={() => setActionPanel('appraisal')}>
                  Correct Appraisal
                </button>
              )}
            </div>
            <div className="detail-section">
              <div className="detail-section__title">Price</div>
              <div className="detail-grid">
                <Field label="Formula Price"><span className="mono">{formatPrice(resale.formulaPrice!)}</span></Field>
                <Field label="Appraised Value"><span className="mono">{formatPrice(resale.appraisedValue)}</span></Field>
                <Field label="Purchase Option Price" full><span className="mono">{formatPrice(resale.purchaseOptionPrice!)}</span></Field>
              </div>
            </div>
          </>
        )}

        {resale.optionExercisedDate && (
          <div className="detail-section">
            <div className="detail-section__title">Decision</div>
            <div className="detail-grid">
              <Field label="Exercised Date"><span className="mono">{resale.optionExercisedDate}</span></Field>
              {resale.optionExercisedNotes && <Field label="Notes" full>{resale.optionExercisedNotes}</Field>}
            </div>
          </div>
        )}

        {closed && (
          <div className="detail-section">
            <div className="detail-section__title">Closing</div>
            <div className="detail-grid">
              <Field label="Closing Date"><span className="mono">{resale.closingDate}</span></Field>
              <Field label="Purchase Price Paid"><span className="mono">{formatPrice(resale.pricePaid!)}</span></Field>
              {resale.feesPaidFromProceeds != null && resale.feesPaidFromProceeds > 0 && (
                <Field label="Fees Paid from Proceeds"><span className="mono">{formatPrice(resale.feesPaidFromProceeds)}</span></Field>
              )}
              <Field label="Home">{resale.offeredForSale ? 'Offered to the waiting list' : `Held by the trust: ${resale.holdReason}`}</Field>
              {resale.closingNotes && <Field label="Closing Notes" full>{resale.closingNotes}</Field>}
            </div>
          </div>
        )}

        {resale.status === 'Withdrawn' && (
          <div className="detail-section">
            <div className="detail-section__title">Withdrawal</div>
            <div className="detail-grid">
              <Field label="Withdrawn Date"><span className="mono">{resale.withdrawnDate}</span></Field>
              <Field label="Reason" full>{resale.withdrawnReason}</Field>
            </div>
          </div>
        )}

        {(resale.payoffStatements?.length ?? 0) > 0 && (
          <div className="detail-section">
            <div className="detail-section__title">Payoff Statements</div>
            <table className="registry-table" aria-label="Payoff statements">
              <thead>
                <tr><th>Printed</th><th>By</th><th>Good Through</th><th>Total Owed</th></tr>
              </thead>
              <tbody>
                {[...resale.payoffStatements!].reverse().map(p => (
                  <tr key={`${p.printedAt}`}>
                    <td className="mono">{formatInstantDate(p.printedAt!)}</td>
                    <td>{p.printedBy}</td>
                    <td className="mono">{p.goodThrough}</td>
                    <td className="mono">{formatPrice(p.totalOwed ?? 0)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        <ConnectedRecords records={records} />
      </SlideOver>

      {actionPanel === 'appraisal' && (
        <RecordResaleAppraisalPanel resale={resale} lease={lease} onClose={() => setActionPanel(null)} />
      )}
      {actionPanel === 'exercise' && <ExercisePurchaseOptionPanel resale={resale} onClose={() => setActionPanel(null)} />}
      {actionPanel === 'closing' && <RecordBuybackClosedPanel resale={resale} onClose={() => setActionPanel(null)} />}
      {actionPanel === 'payoff' && <PayoffStatementPanel resale={resale} onClose={() => setActionPanel(null)} />}
      {actionPanel === 'withdraw' && <WithdrawResalePanel resale={resale} onClose={() => { setActionPanel(null); onClose() }} />}
    </>
  )
}
