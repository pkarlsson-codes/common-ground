import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { LandParcelsApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import type { RegisterLandParcelRequest } from './types'

const api = new LandParcelsApi(apiConfig)

const dateStr = (d: Date | null | undefined) =>
  d instanceof Date ? d.toISOString().substring(0, 10) : undefined

export function useLandParcels(pageNumber: number, includeDecommissioned: boolean, search?: string) {
  return useQuery({
    queryKey: ['land-parcels', pageNumber, includeDecommissioned, search],
    queryFn: async () => {
      const result = await api.getLandParcels({ pageNumber, pageSize: 20, search, includeDecommissioned })
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
