import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  HomeSelectionsApi,
  type CancelHomeSelectionRequest,
  type DrawSelectionLotteryRequest,
  type HomeSelectionResponse,
  type PaginatedResponseOfHomeSelectionListItemResponse,
  type HomeSelectionSummaryResponse,
  type SelectHouseholdByPriorityRequest,
  type StartHomeSelectionResponse,
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

/** The route of a home selection's page. */
export const homeSelectionPath = (selectionId: string) => `/homes/selections/${selectionId}`

export const selectionPriorityLabel = (value: string) =>
  SELECTION_PRIORITIES.find(p => p.value === value)?.label ?? value

/** A home's selections, newest first. At most one is open or completed; the rest are cancelled. */
export function useHomeSelectionsForHome(homeId: string | null) {
  return useQuery<HomeSelectionSummaryResponse[]>({
    queryKey: ['home-selections', 'home', homeId],
    queryFn: () => api.getHomeSelectionsForHome({ homeId: homeId! }),
    enabled: !!homeId,
  })
}

/** Home selections across every home, newest first, optionally only those with <paramref name="status"/>. */
export function useHomeSelections(pageNumber: number, status?: string) {
  return useQuery<PaginatedResponseOfHomeSelectionListItemResponse>({
    queryKey: ['home-selections', 'all', pageNumber, status],
    queryFn: () => api.getHomeSelections({ pageNumber, pageSize: 20, status }),
  })
}

export function useHomeSelection(selectionId: string | null) {
  return useQuery<HomeSelectionResponse>({
    queryKey: ['home-selection', selectionId],
    queryFn: () => api.getHomeSelection({ selectionId: selectionId! }),
    enabled: !!selectionId,
  })
}

/**
 * Completing or cancelling a selection changes the home's selections, the chosen
 * application, the waiting list's pass-over counts, and starts a purchase workflow.
 */
const invalidateSelection = (qc: ReturnType<typeof useQueryClient>, selectionId: string) => {
  qc.invalidateQueries({ queryKey: ['home-selection', selectionId] })
  qc.invalidateQueries({ queryKey: ['home-selections'] })
  qc.invalidateQueries({ queryKey: ['homeowner-application'] })
  qc.invalidateQueries({ queryKey: ['homeowner-applications'] })
  qc.invalidateQueries({ queryKey: ['purchase-workflows'] })
}

export function useStartHomeSelection() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (homeId: string) =>
      await api.startHomeSelection({ homeId }) as StartHomeSelectionResponse,
    onSuccess: (_data, homeId) => {
      qc.invalidateQueries({ queryKey: ['home-selections', 'home', homeId] })
    },
  })
}

export function useSelectHouseholdByPriority() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ selectionId, request }: { selectionId: string; request: SelectHouseholdByPriorityRequest }) =>
      api.selectHouseholdByPriority({ selectionId, selectHouseholdByPriorityRequest: request }),
    onSuccess: (_data, variables) => invalidateSelection(qc, variables.selectionId),
  })
}

export function useDrawSelectionLottery() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ selectionId, request }: { selectionId: string; request: DrawSelectionLotteryRequest }) =>
      api.drawSelectionLottery({ selectionId, drawSelectionLotteryRequest: request }),
    onSuccess: (_data, variables) => invalidateSelection(qc, variables.selectionId),
  })
}

export function useCancelHomeSelection() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ selectionId, request }: { selectionId: string; request: CancelHomeSelectionRequest }) =>
      api.cancelHomeSelection({ selectionId, cancelHomeSelectionRequest: request }),
    onSuccess: (_data, variables) => invalidateSelection(qc, variables.selectionId),
  })
}
