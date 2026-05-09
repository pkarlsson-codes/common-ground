import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { HomesApi, type RegisterHomeRequest } from '../api'
import { apiConfig } from '../lib/apiConfig'
import type { RecordHomeAssessmentRequest } from './types'
import type { PaginatedResponseOfHomeAssessmentItemResponse } from '../api'

const api = new HomesApi(apiConfig)

const dateStr = (d: Date | null | undefined) =>
  d instanceof Date ? d.toISOString().substring(0, 10) : undefined

export function useHomes(params: { pageNumber: number; pageSize?: number; landParcelId?: string; status?: string }) {
  return useQuery({
    queryKey: ['homes', params.pageNumber, params.landParcelId, params.status],
    queryFn: () => api.getHomes({
      pageNumber: params.pageNumber,
      pageSize: params.pageSize ?? 20,
      landParcelId: params.landParcelId,
      status: params.status,
    }),
  })
}

export function useHome(id: string | null) {
  return useQuery({
    queryKey: ['home', id],
    queryFn: async () => {
      const b = await api.getHome({ homeId: id! })
      return {
        ...b,
        lastAssessedDate: dateStr(b.lastAssessedDate),
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
          assessmentDate: new Date(req.assessmentDate),
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
