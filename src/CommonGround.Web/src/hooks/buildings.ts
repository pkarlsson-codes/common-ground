import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { BuildingsApi, Configuration } from '../api'
import type { RecordBuildingAssessmentRequest, RegisterBuildingRequest } from './types'

// Empty basePath so requests use relative URLs through Vite's /api proxy
const api = new BuildingsApi(new Configuration({ basePath: '' }))

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
    onSuccess: (_d, v) => {
      qc.invalidateQueries({ queryKey: ['building', v.id] })
      qc.invalidateQueries({ queryKey: ['buildings'] })
    },
  })
}
