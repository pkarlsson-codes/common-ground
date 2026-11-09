import type { ReactNode } from 'react'
import { useNavigate } from 'react-router-dom'
import type { HouseholdResponse } from '../../api'
import ConnectedRecords, { type ConnectedRecord } from '../../components/lifecycle/ConnectedRecords'
import LifecycleStepper from '../../components/lifecycle/LifecycleStepper'
import NextStepCard, { type NextStepAction } from '../../components/lifecycle/NextStepCard'
import { useConnectedGroundLeases } from '../../hooks/groundLeases'
import { isOpenPurchasePhase, purchaseWaitingOn, useConnectedPurchaseWorkflows, usePurchaseWorkflow } from '../../hooks/purchaseWorkflows'
import { formatStepDate, HOUSEHOLD_STAGES, lifecycleSteps, missingReadiness, READINESS_LABELS, type ReadinessItem } from '../../lib/lifecycle'

interface Props {
  household: HouseholdResponse
  onRecordReadiness: (item: ReadinessItem) => void
  onStartApplication: () => void
  startingApplication: boolean
  moreActions: NextStepAction[]
  children?: ReactNode
}

/** The household's lifecycle stepper and its next step. */
export function HouseholdLifecycle({ household, onRecordReadiness, onStartApplication, startingApplication, moreActions, children }: Props) {
  const navigate = useNavigate()
  const journey = household.journey
  const stage = journey?.stage ?? 'Intake'
  const { data: purchases } = useConnectedPurchaseWorkflows({ householdId: household.id ?? undefined })
  const openPurchaseItem = purchases?.items?.find(p => isOpenPurchasePhase(p.phase))
  const { data: openPurchase } = usePurchaseWorkflow(openPurchaseItem?.id ?? null)
  const { data: leases } = useConnectedGroundLeases({ homeId: stage === 'Homeowner' ? journey?.selectedHomeId ?? undefined : undefined })
  const lease = leases?.items?.find(l => l.status === 'Active') ?? leases?.items?.[0]
  const homeLabel = purchases?.items?.find(p => p.homeId === journey?.selectedHomeId)?.homeLabel ?? 'their home'

  const steps = lifecycleSteps(HOUSEHOLD_STAGES, stage, {
    Intake: formatStepDate(household.registeredAt),
    Ready: formatStepDate(journey?.readyDate),
    Approved: formatStepDate(journey?.approvedDate),
    Waiting: journey?.approvedDate ? `Since ${formatStepDate(journey.approvedDate)}` : undefined,
    Selected: formatStepDate(journey?.selectedDate),
    Purchasing: journey?.selectedDate ? `Since ${formatStepDate(journey.selectedDate)}` : undefined,
    Homeowner: formatStepDate(journey?.purchasedDate),
  })

  let title: string
  let primary: NextStepAction | undefined
  switch (stage) {
    case 'Ready':
      if (journey?.currentApplicationId) {
        const applicationId = journey.currentApplicationId
        title = 'An application is awaiting a decision.'
        primary = { label: 'Open Application', onClick: () => navigate('/applications', { state: { openId: applicationId } }) }
      } else {
        title = 'Readiness is complete, so the household can apply.'
        primary = { label: startingApplication ? 'Starting…' : 'Start Application', onClick: onStartApplication, disabled: startingApplication }
      }
      break
    case 'Waiting':
      title = `On the waiting list since ${formatStepDate(journey?.approvedDate)}.`
      primary = { label: 'Open Waiting List', onClick: () => navigate('/waiting-list') }
      break
    case 'Purchasing':
      title = openPurchase
        ? `Buying ${openPurchaseItem?.homeLabel ?? 'a home'}: waiting on ${purchaseWaitingOn(openPurchase)}.`
        : `Selected for ${homeLabel}; the purchase is starting.`
      primary = openPurchaseItem
        ? { label: 'Open Purchase', onClick: () => navigate('/purchase-workflows', { state: { openId: openPurchaseItem.id } }) }
        : undefined
      break
    case 'Homeowner':
      title = `Owns ${homeLabel} since ${formatStepDate(journey?.purchasedDate)}.`
      primary = lease ? { label: 'Open Ground Lease', onClick: () => navigate('/ground-leases', { state: { openId: lease.id } }) } : undefined
      break
    default: {
      const missing = missingReadiness(household)
      title = missing.length
        ? `Still to record: ${missing.map(m => READINESS_LABELS[m]).join(', ')}.`
        : 'Readiness is being completed.'
      primary = missing.length
        ? { label: `Record ${READINESS_LABELS[missing[0]]}`, onClick: () => onRecordReadiness(missing[0]) }
        : undefined
    }
  }

  return (
    <>
      <LifecycleStepper steps={steps} label="Household lifecycle" />
      <NextStepCard title={title} primary={primary} moreActions={moreActions}>
        {children}
      </NextStepCard>
    </>
  )
}

/** Links to the household's applications, the homes it was selected for, its purchases and its ground lease. */
export function HouseholdConnectedRecords({ household }: { household: HouseholdResponse }) {
  const householdId = household.id!
  const journey = household.journey
  const { data: purchases, isLoading } = useConnectedPurchaseWorkflows({ householdId })
  const { data: leases } = useConnectedGroundLeases({ homeId: journey?.stage === 'Homeowner' ? journey.selectedHomeId ?? undefined : undefined })

  const homes = new Map<string, string>()
  for (const p of purchases?.items ?? []) {
    if (p.homeId && !homes.has(p.homeId)) homes.set(p.homeId, p.homeLabel ?? 'Home')
  }

  const records: ConnectedRecord[] = [
    ...(household.applications ?? []).map(a => ({
      key: `application-${a.id}`,
      type: 'Application',
      name: `Submitted ${formatStepDate(a.submittedAt)}`,
      status: a.phase ?? undefined,
      to: '/applications',
      openId: a.id!,
    })),
    ...[...homes].map(([homeId, label]) => ({
      key: `home-${homeId}`,
      type: 'Home',
      name: label,
      to: '/homes',
      openId: homeId,
    })),
    ...(purchases?.items ?? []).map(p => ({
      key: `purchase-${p.id}`,
      type: 'Purchase',
      name: p.homeLabel ?? 'Purchase',
      status: p.phase ?? undefined,
      to: '/purchase-workflows',
      openId: p.id!,
    })),
    ...(leases?.items ?? []).map(l => ({
      key: `lease-${l.id}`,
      type: 'Ground lease',
      name: l.homeownerFullName ?? 'Ground lease',
      meta: `From ${formatStepDate(l.leaseStartDate)}`,
      status: l.status ?? undefined,
      to: '/ground-leases',
      openId: l.id!,
    })),
  ]

  return <ConnectedRecords records={records} isLoading={isLoading} />
}
