import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import type { HomeResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import ConnectedRecords, { type ConnectedRecord } from '../../components/lifecycle/ConnectedRecords'
import LifecycleStepper from '../../components/lifecycle/LifecycleStepper'
import NextStepCard, { type NextStepAction } from '../../components/lifecycle/NextStepCard'
import { useConnectedGroundLeases } from '../../hooks/groundLeases'
import { useReleaseHomeForSale } from '../../hooks/homes'
import { resaleStep, useOpenResaleForLease } from '../../hooks/resales'
import { useToast } from '../../hooks/useToast'
import { homeSelectionPath, useHomeSelectionOverview } from '../../hooks/homeSelections'
import { isOpenPurchasePhase, purchaseWaitingOn, useConnectedPurchaseWorkflows, usePurchaseWorkflow } from '../../hooks/purchaseWorkflows'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { formatStepDate, homeStage, homeStages, lifecycleSteps } from '../../lib/lifecycle'

type Home = Omit<HomeResponse, 'registeredAt'> & { registeredAt: string }

interface Props {
  home: Home
  moreActions: NextStepAction[]
}

/** The home's lifecycle stepper and its next step. */
export function HomeLifecycle({ home, moreActions }: Props) {
  const navigate = useNavigate()
  const homeId = home.id!
  const stage = homeStage(home.occupancy, home.saleStage)
  const { data: selection } = useHomeSelectionOverview(homeId)
  const { data: purchases } = useConnectedPurchaseWorkflows({ homeId })
  const { data: leases } = useConnectedGroundLeases({ homeId })
  const openPurchaseId = purchases?.items?.find(p => isOpenPurchasePhase(p.phase))?.id ?? null
  const { data: openPurchase } = usePurchaseWorkflow(openPurchaseId)
  const releaseMutation = useReleaseHomeForSale()
  const { success } = useToast()
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)

  const earlierChoices = selection?.decisions?.length ?? 0
  const waitingCount = selection?.waitingList?.length
  const activeLease = leases?.items?.find(l => l.status === 'Active')
  const { data: openResale } = useOpenResaleForLease(activeLease?.id ?? '')

  const steps = lifecycleSteps(homeStages(stage), stage, {
    Development: formatStepDate(new Date(home.registeredAt)),
    Available: formatStepDate(home.availableAgainDate) ?? formatStepDate(new Date(home.registeredAt)),
    Purchasing: home.underContractDate ? `Since ${formatStepDate(home.underContractDate)}` : undefined,
    Leased: formatStepDate(home.leasedDate),
    Resale: home.resaleNoticeDate ? `Notice ${formatStepDate(home.resaleNoticeDate)}` : undefined,
    'Held by Trust': formatStepDate(home.heldSinceDate),
  })

  const handleRelease = async () => {
    setProblem(null)
    try {
      await releaseMutation.mutateAsync(homeId)
      success('Home released for sale')
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to release the home for sale.'))
    }
  }

  let title: string
  let primary: NextStepAction | undefined
  switch (stage) {
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
      title = activeLease && home.resaleNoticeDate
        ? `${activeLease.homeownerFullName ?? 'The homeowner'} gave notice to sell on ${formatStepDate(home.resaleNoticeDate)}.`
        : 'The home is being resold.'
      primary = openResale
        ? { label: 'Open Resale', onClick: () => navigate('/resales', { state: { openId: openResale.id } }) }
        : undefined
      break
    case 'Held by Trust':
      title = `The trust bought this home on ${formatStepDate(home.heldSinceDate)} and is holding it: ${home.holdReason}.`
      primary = { label: releaseMutation.isPending ? 'Releasing…' : 'Release for Sale', onClick: () => { void handleRelease() }, disabled: releaseMutation.isPending }
      break
    default:
      title = waitingCount === undefined
        ? 'Choose a household from the waiting list for this home.'
        : `Choose a household for this home from the ${waitingCount} on the waiting list.`
      primary = { label: 'Choose a Household', onClick: () => navigate(homeSelectionPath(homeId)) }
  }

  return (
    <>
      <LifecycleStepper steps={steps} label="Home lifecycle" />
      <NextStepCard
        title={title}
        body={stage === 'Available' && earlierChoices > 0
          ? `${earlierChoices} household${earlierChoices !== 1 ? 's were' : ' was'} chosen for this home before.`
          : stage === 'Resale' && openResale
            ? `The resale is at the ${resaleStep(openResale)} step.`
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
  const { data: selection, isLoading: selectionsLoading } = useHomeSelectionOverview(homeId)
  const { data: purchases, isLoading: purchasesLoading } = useConnectedPurchaseWorkflows({ homeId })
  const { data: leases, isLoading: leasesLoading } = useConnectedGroundLeases({ homeId })

  const records: ConnectedRecord[] = [
    ...(home.landParcelId
      ? [{ key: `parcel-${home.landParcelId}`, type: 'Land parcel', name: home.label ?? 'Land parcel', to: '/land-parcels', openId: home.landParcelId }]
      : []),
    ...(selection?.decisions ?? []).map(s => ({
      key: `selection-${s.id}`,
      type: 'Selection',
      name: `Chosen ${formatStepDate(s.selectedDate)}`,
      meta: `${s.selectionMethod === 'Lottery' ? 'By lottery' : 'By priority'} · ${s.applications?.length ?? 0} on the waiting list`,
      to: homeSelectionPath(homeId),
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
