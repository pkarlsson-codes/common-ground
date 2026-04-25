import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { BuildingsApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import type { RecordBuildingAssessmentRequest, RegisterBuildingRequest } from './types'

const api = new BuildingsApi(apiConfig)

const dateStr = (d: Date | null | undefined) =>
  d instanceof Date ? d.toISOString().substring(0, 10) : undefined

export function useBuildings(page: number, landParcelId?: string, status?: string) {
  return useQuery({
    queryKey: ['buildings', page, landParcelId, status],
    queryFn: () => api.getBuildings({ page, pageSize: 20, landParcelId, status }),
  })
}

export function useBuilding(id: string | null) {
  return useQuery({
    queryKey: ['building', id],
    queryFn: async () => {
      const b = await api.getBuilding({ buildingId: id! })
      return {
        ...b,
        lastAssessedDate: dateStr(b.lastAssessedDate),
        registeredAt: b.registeredAt?.toISOString() ?? '',
      }
    },
    enabled: !!id,
  })
}

export function useRegisterBuilding() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (req: RegisterBuildingRequest) =>
      api.registerBuilding({ registerBuildingRequest: req }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['buildings'] }),
  })
}

export function useRecordAssessment() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ buildingId, req }: { buildingId: string; req: RecordBuildingAssessmentRequest }) =>
      api.recordBuildingAssessment({
        buildingId,
        recordBuildingAssessmentRequest: {
          ...req,
          assessmentDate: new Date(req.assessmentDate),
        },
      }),
    onSuccess: (_d, v) => {
      qc.invalidateQueries({ queryKey: ['building', v.buildingId] })
      qc.invalidateQueries({ queryKey: ['buildings'] })
    },
  })
}

export function useUpdateBuildingStatus() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, newStatus }: { id: string; newStatus: string }) =>
      api.updateBuildingStatus({
        buildingId: id,
        updateBuildingStatusRequest: { newStatus },
      }),
    onMutate: async ({ id, newStatus }) => {
      await qc.cancelQueries({ queryKey: ['building', id] })
      await qc.cancelQueries({ queryKey: ['buildings'] })

      const previousDetail = qc.getQueryData(['building', id])
      const previousLists = qc.getQueriesData({ queryKey: ['buildings'] })

      qc.setQueryData(['building', id], (current: any) =>
        current ? { ...current, status: newStatus } : current,
      )

      previousLists.forEach(([queryKey]) => {
        qc.setQueryData(queryKey, (current: any) => {
          if (!current?.items) return current

          return {
            ...current,
            items: current.items.map((item: any) =>
              item.id === id ? { ...item, status: newStatus } : item,
            ),
          }
        })
      })

      return { previousDetail, previousLists, id }
    },
    onError: (_error, _vars, context) => {
      if (!context) return

      qc.setQueryData(['building', context.id], context.previousDetail)
      context.previousLists.forEach(([queryKey, data]) => {
        qc.setQueryData(queryKey, data)
      })
    },
    onSettled: (_d, _e, v) => {
      qc.invalidateQueries({ queryKey: ['building', v.id] })
      qc.invalidateQueries({ queryKey: ['buildings'] })
    },
  })
}
