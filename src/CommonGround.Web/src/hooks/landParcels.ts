import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { LandParcelsApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import type { RegisterLandParcelRequest } from './types'

const api = new LandParcelsApi(apiConfig)

const dateStr = (d: Date | null | undefined) =>
  d instanceof Date ? d.toISOString().substring(0, 10) : undefined

export function useLandParcels(page: number, status?: string) {
  return useQuery({
    queryKey: ['land-parcels', page, status],
    queryFn: async () => {
      const result = await api.getLandParcels({ page, pageSize: 20, status })
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

export function useUpdateLandParcelStatus() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, newStatus }: { id: string; newStatus: string }) =>
      api.updateLandParcelStatus({
        landParcelId: id,
        updateLandParcelStatusRequest: { newStatus },
      }),
    onSuccess: (_d, v) => {
      qc.invalidateQueries({ queryKey: ['land-parcel', v.id] })
      qc.invalidateQueries({ queryKey: ['land-parcels'] })
    },
  })
}
