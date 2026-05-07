import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { LandParcelsApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import type { RegisterLandParcelRequest } from './types'

const api = new LandParcelsApi(apiConfig)

const dateStr = (d: Date | null | undefined) =>
  d instanceof Date ? d.toISOString().substring(0, 10) : undefined

export function useLandParcels(page: number, isDecommissioned?: boolean, search?: string) {
  return useQuery({
    queryKey: ['land-parcels', page, isDecommissioned, search],
    queryFn: async () => {
      const params: Parameters<typeof api.getLandParcels>[0] = { page, pageSize: 20, search }
      if (isDecommissioned !== undefined) {
        (params as Record<string, unknown>)['isDecommissioned'] = isDecommissioned
      }
      const result = await api.getLandParcels(params)
      return {
        ...result,
        items: result.items?.map(p => ({
          ...p,
          registeredAt: p.registeredAt?.toISOString() ?? '',
        })),
      }
    },
  })
}

export function useLandParcel(id: string | null) {
  return useQuery({
    queryKey: ['land-parcel', id],
    queryFn: async () => {
      const p = await api.getLandParcel({ landParcelId: id! })
      return {
        ...p,
        acquisitionDate: dateStr(p.acquisitionDate) ?? '',
        registeredAt: p.registeredAt?.toISOString() ?? '',
      }
    },
    enabled: !!id,
  })
}

export function useRegisterLandParcel() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (req: RegisterLandParcelRequest) =>
      api.registerLandParcel({
        registerLandParcelRequest: {
          ...req,
          acquisitionDate: new Date(req.acquisitionDate),
        },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['land-parcels'] }),
  })
}

export function useDecommissionParcel() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) =>
      api.decommissionLandParcel({ landParcelId: id, body: {} }),
    onSuccess: (_d, id) => {
      qc.invalidateQueries({ queryKey: ['land-parcel', id] })
      qc.invalidateQueries({ queryKey: ['land-parcels'] })
    },
  })
}

export function useRecommissionParcel() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) =>
      api.recommissionLandParcel({ landParcelId: id, body: {} }),
    onSuccess: (_d, id) => {
      qc.invalidateQueries({ queryKey: ['land-parcel', id] })
      qc.invalidateQueries({ queryKey: ['land-parcels'] })
    },
  })
}
