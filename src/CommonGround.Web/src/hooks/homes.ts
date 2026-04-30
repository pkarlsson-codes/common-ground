import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { HomesApi, type RegisterHomeRequest } from '../api'
import { apiConfig } from '../lib/apiConfig'
import type { RecordHomeAssessmentRequest } from './types'
import type { PaginatedResponseOfBuildingAssessmentItemResponse } from '../api'

const api = new HomesApi(apiConfig)

const dateStr = (d: Date | null | undefined) =>
  d instanceof Date ? d.toISOString().substring(0, 10) : undefined

export function useHomes(params: { page: number; pageSize?: number; landParcelId?: string; status?: string }) {
  return useQuery({
    queryKey: ['homes', params.page, params.landParcelId, params.status],
    queryFn: () => api.getHomes({
      page: params.page,
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
  page: number = 1,
  pageSize: number = 20,
) {
  return useQuery<PaginatedResponseOfBuildingAssessmentItemResponse>({
    queryKey: ['home-assessments', homeId, page, pageSize],
    queryFn: () => api.getHomeAssessments({ homeId: homeId!, page, pageSize }),
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
        recordBuildingAssessmentRequest: {
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
