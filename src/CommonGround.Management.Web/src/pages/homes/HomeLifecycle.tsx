import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import type { HomeResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import ConnectedRecords, { type ConnectedRecord } from '../../components/lifecycle/ConnectedRecords'
import LifecycleStepper from '../../components/lifecycle/LifecycleStepper'
import NextStepCard, { type NextStepAction } from '../../components/lifecycle/NextStepCard'
import { useConnectedGroundLeases } from '../../hooks/groundLeases'
import { homeSelectionPath, useHomeSelectionsForHome, useStartHomeSelection } from '../../hooks/homeSelections'
import { isOpenPurchasePhase, purchaseWaitingOn, useConnectedPurchaseWorkflows, usePurchaseWorkflow } from '../../hooks/purchaseWorkflows'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { formatStepDate, HOME_STAGES, homeStage, lifecycleSteps } from '../../lib/lifecycle'

type Home = Omit<HomeResponse, 'registeredAt'> & { registeredAt: string }

interface Props {
  home: Home
  moreActions: NextStepAction[]
}

/** The home's lifecycle stepper and its next step. */
export function HomeLifecycle({ home, moreActions }: Props) {
  const navigate = useNavigate()
  const homeId = home.id!
  const stage = homeStage(home.status)
  const { data: selections } = useHomeSelectionsForHome(homeId)
  const { data: purchases } = useConnectedPurchaseWorkflows({ homeId })
  const { data: leases } = useConnectedGroundLeases({ homeId })
  const openPurchaseId = purchases?.items?.find(p => isOpenPurchasePhase(p.phase))?.id ?? null
  const { data: openPurchase } = usePurchaseWorkflow(openPurchaseId)
  const startMutation = useStartHomeSelection()
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)

  const openSelection = selections?.find(s => s.status === 'Open')
  const endedCount = (selections ?? []).filter(s => s.status === 'Cancelled' || s.status === 'FellThrough').length
  const activeLease = leases?.items?.find(l => l.status === 'Active')

  const steps = lifecycleSteps(HOME_STAGES, stage, {
    Development: formatStepDate(new Date(home.registeredAt)),
    Available: formatStepDate(home.availableAgainDate) ?? formatStepDate(new Date(home.registeredAt)),
    Selection: formatStepDate(home.selectionOpenedDate),
    Purchasing: home.underContractDate ? `Since ${formatStepDate(home.underContractDate)}` : undefined,
    Leased: formatStepDate(home.leasedDate),
  })

  const handleStart = async () => {
    setProblem(null)
    try {
      const { selectionId } = await startMutation.mutateAsync(homeId)
      navigate(homeSelectionPath(selectionId!))
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to start the selection.'))
    }
  }

  let title: string
  let primary: NextStepAction | undefined
  switch (stage) {
    case 'Selection':
      title = openSelection
        ? `A selection is choosing a household from ${openSelection.candidateCount} candidate${openSelection.candidateCount !== 1 ? 's' : ''}.`
        : 'A selection is choosing a household for this home.'
      primary = openSelection ? { label: 'Open Selection', onClick: () => navigate(homeSelectionPath(openSelection.id!)) } : undefined
      break
    case 'Purchasing':
      title = openPurchase
        ? `The purchase by ${openPurchase.buyerFullName} is waiting on ${purchaseWaitingOn(openPurchase)}.`
        : 'The home is under contract.'
      primary = openPurchaseId
        ? { label: 'Open Purchase', onClick: () => navigate('/purchase-workflows', { state: { openId: openPurchaseId } }) }
        : undefined
      break
    case 'Leased':
      title = activeLease
        ? `Leased to ${activeLease.homeownerFullName ?? 'its homeowner'} since ${formatStepDate(home.leasedDate ?? activeLease.leaseStartDate)}.`
        : 'The home is leased.'
      primary = activeLease
        ? { label: 'Open Ground Lease', onClick: () => navigate('/ground-leases', { state: { openId: activeLease.id } }) }
        : undefined
      break
    case 'Resale':
      title = 'The home is being resold.'
      break
    default:
      title = 'Choose a household from the waiting list for this home.'
      primary = { label: startMutation.isPending ? 'Starting…' : 'Start Selection', onClick: () => { void handleStart() }, disabled: startMutation.isPending }
  }

  return (
    <>
      <LifecycleStepper steps={steps} label="Home lifecycle" />
      <NextStepCard
        title={title}
        body={stage === 'Available' && endedCount > 0
          ? `${endedCount} earlier selection${endedCount !== 1 ? 's' : ''} ended without a sale.`
          : undefined}
        primary={primary}
        moreActions={moreActions}
      >
        <ApiProblemAlert problem={problem} />
      </NextStepCard>
    </>
  )
}

/** Links to the home's land parcel, selections, purchases, buying households and ground leases. */
export function HomeConnectedRecords({ home }: { home: Home }) {
  const homeId = home.id!
  const { data: selections, isLoading: selectionsLoading } = useHomeSelectionsForHome(homeId)
  const { data: purchases, isLoading: purchasesLoading } = useConnectedPurchaseWorkflows({ homeId })
  const { data: leases, isLoading: leasesLoading } = useConnectedGroundLeases({ homeId })

  const records: ConnectedRecord[] = [
    ...(home.landParcelId
      ? [{ key: `parcel-${home.landParcelId}`, type: 'Land parcel', name: home.label ?? 'Land parcel', to: '/land-parcels', openId: home.landParcelId }]
      : []),
    ...(selections ?? []).map(s => ({
      key: `selection-${s.id}`,
      type: 'Selection',
      name: `Started ${formatStepDate(s.startedAt)}`,
      meta: `${s.candidateCount} candidate${s.candidateCount !== 1 ? 's' : ''}`,
      status: s.status ?? undefined,
      to: homeSelectionPath(s.id!),
    })),
    ...(purchases?.items ?? []).map(p => ({
      key: `purchase-${p.id}`,
      type: 'Purchase',
      name: p.buyerFullName ?? 'Purchase',
      status: p.phase ?? undefined,
      to: '/purchase-workflows',
      openId: p.id!,
    })),
    ...[...new Map((purchases?.items ?? [])
      .filter(p => p.householdId)
      .map(p => [p.householdId!, p.buyerFullName ?? 'Household'])).entries()]
      .map(([householdId, name]) => ({
        key: `household-${householdId}`,
        type: 'Household',
        name,
        to: '/households',
        openId: householdId,
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

  return <ConnectedRecords records={records} isLoading={selectionsLoading || purchasesLoading || leasesLoading} />
}
