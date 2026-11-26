import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  HomeSelectionsApi,
  type DrawSelectionLotteryRequest,
  type HomeSelectionOverviewResponse,
  type PaginatedResponseOfHomeSelectionListItemResponse,
  type SelectHouseholdByPriorityRequest,
} from '../api'
import { apiConfig } from '../lib/apiConfig'

const api = new HomeSelectionsApi(apiConfig)

/** The selection priorities staff can cite, in the API's names, with display labels. */
export const SELECTION_PRIORITIES: { value: string; label: string }[] = [
  { value: 'LowerIncome', label: 'Lower income' },
  { value: 'HouseholdSizeFit', label: 'Household size fit' },
  { value: 'AccessibilityNeed', label: 'Accessibility need' },
  { value: 'WaitingListTenure', label: 'Waiting-list tenure' },
]

/** Why a household can't be chosen, in the API's names, with the words staff read. */
const CANNOT_BE_CHOSEN_REASONS: Record<string, string> = {
  OverIncomeLimit: 'Over income limit',
  NoPrimaryMember: 'No primary member',
  HousingCounselingIncomplete: 'Housing counseling not completed',
  CltOrientationIncomplete: 'CLT orientation not completed',
  NoPrequalification: 'No mortgage prequalification',
  PrequalificationExpired: 'Prequalification expired',
}

export const cannotBeChosenReasonLabel = (value: string) => CANNOT_BE_CHOSEN_REASONS[value] ?? value

/** The route of a home's selection page: its waiting list compared against it, and the households chosen for it. */
export const homeSelectionPath = (homeId: string) => `/homes/${homeId}/selection`

export const selectionPriorityLabel = (value: string) =>
  SELECTION_PRIORITIES.find(p => p.value === value)?.label ?? value

/** A home's waiting list compared against it today, and the households already chosen for it, newest first. */
export function useHomeSelectionOverview(homeId: string | null) {
  return useQuery<HomeSelectionOverviewResponse>({
    queryKey: ['home-selections', 'home', homeId],
    queryFn: () => api.getHomeSelectionOverview({ homeId: homeId! }),
    enabled: !!homeId,
  })
}

/** The households chosen for homes across every home, newest first. */
export function useHomeSelections(pageNumber: number) {
  return useQuery<PaginatedResponseOfHomeSelectionListItemResponse>({
    queryKey: ['home-selections', 'all', pageNumber],
    queryFn: () => api.getHomeSelections({ pageNumber, pageSize: 20 }),
  })
}

/**
 * Choosing a household changes the home's selections and sale stage, the chosen application, the waiting
 * list's pass-over counts, and starts a purchase workflow.
 */
const invalidateChoice = (qc: ReturnType<typeof useQueryClient>, homeId: string) => {
  qc.invalidateQueries({ queryKey: ['home-selections'] })
  qc.invalidateQueries({ queryKey: ['home', homeId] })
  qc.invalidateQueries({ queryKey: ['homes'] })
  qc.invalidateQueries({ queryKey: ['homeowner-application'] })
  qc.invalidateQueries({ queryKey: ['homeowner-applications'] })
  qc.invalidateQueries({ queryKey: ['purchase-workflows'] })
  qc.invalidateQueries({ queryKey: ['today'] })
}

export function useSelectHouseholdByPriority() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ homeId, request }: { homeId: string; request: SelectHouseholdByPriorityRequest }) =>
      api.selectHouseholdByPriority({ homeId, selectHouseholdByPriorityRequest: request }),
    onSuccess: (_data, variables) => invalidateChoice(qc, variables.homeId),
  })
}

export function useDrawSelectionLottery() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ homeId, request }: { homeId: string; request: DrawSelectionLotteryRequest }) =>
      api.drawSelectionLottery({ homeId, drawSelectionLotteryRequest: request }),
    onSuccess: (_data, variables) => invalidateChoice(qc, variables.homeId),
  })
}
