import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { HomesApi, type RegisterHomeRequest } from '../api'
import { apiConfig } from '../lib/apiConfig'
import type { RecordHomeAssessmentRequest } from './types'
import type { PaginatedResponseOfHomeAssessmentItemResponse } from '../api'

const api = new HomesApi(apiConfig)

export function useHomes(params: { pageNumber: number; pageSize?: number; landParcelId?: string; occupancy?: string; saleStage?: string; enabled?: boolean }) {
  return useQuery({
    queryKey: ['homes', params.pageNumber, params.pageSize, params.landParcelId, params.occupancy, params.saleStage],
    queryFn: () => api.getHomes({
      pageNumber: params.pageNumber,
      pageSize: params.pageSize ?? 20,
      landParcelIds: params.landParcelId ? [params.landParcelId] : undefined,
      occupancy: params.occupancy ? [params.occupancy] : undefined,
      saleStages: params.saleStage ? [params.saleStage] : undefined,
    }),
    enabled: params.enabled ?? true,
  })
}

export function useHome(id: string | null) {
  return useQuery({
    queryKey: ['home', id],
    queryFn: async () => {
      const b = await api.getHome({ homeId: id! })
      return {
        ...b,
        registeredAt: b.registeredAt?.toISOString() ?? '',
      }
    },
    enabled: !!id,
  })
}

export function useHomeAssessments(
  homeId: string | null,
  pageNumber: number = 1,
  pageSize: number = 20,
) {
  return useQuery<PaginatedResponseOfHomeAssessmentItemResponse>({
    queryKey: ['home-assessments', homeId, pageNumber, pageSize],
    queryFn: () => api.getHomeAssessments({ homeId: homeId!, pageNumber, pageSize }),
    enabled: !!homeId,
  })
}

export function useRegisterHome() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (req: RegisterHomeRequest) =>
      api.registerHome({ registerHomeRequest: req }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['homes'] }),
  })
}

export function useRecordHomeAssessment() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ homeId, req }: { homeId: string; req: RecordHomeAssessmentRequest }) =>
      api.recordHomeAssessment({
        homeId,
        recordHomeAssessmentRequest: {
          assessedValue: req.assessedValue,
          assessmentDate: req.assessmentDate,
          appraiserName: req.appraiserName,
          conditionNotes: req.conditionNotes,
        },
      }),
    onSuccess: (_d, v) => {
      qc.invalidateQueries({ queryKey: ['home', v.homeId] })
      qc.invalidateQueries({ queryKey: ['home-assessments', v.homeId] })
      qc.invalidateQueries({ queryKey: ['homes'] })
    },
  })
}

export function useSetHomeIncomeLimit() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ homeId, percentOfAmi }: { homeId: string; percentOfAmi: number | null }) =>
      api.setHomeIncomeLimit({ homeId, setHomeIncomeLimitRequest: { percentOfAmi } }),
    onSuccess: (_d, v) => {
      qc.invalidateQueries({ queryKey: ['home', v.homeId] })
      qc.invalidateQueries({ queryKey: ['homes'] })
    },
  })
}

export function useReleaseHomeForSale() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (homeId: string) => api.releaseHomeForSale({ homeId }),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['homes'] })
      void qc.invalidateQueries({ queryKey: ['home'] })
      void qc.invalidateQueries({ queryKey: ['today'] })
    },
  })
}
