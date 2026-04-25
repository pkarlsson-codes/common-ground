import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { LandParcelsApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import type { RegisterLandParcelRequest } from './types'

const api = new LandParcelsApi(apiConfig)

const dateStr = (d: Date | null | undefined) =>
  d instanceof Date ? d.toISOString().substring(0, 10) : undefined

export function useLandParcels(page: number, status?: string, search?: string) {
  return useQuery({
    queryKey: ['land-parcels', page, status, search],
    queryFn: async () => {
      const result = await api.getLandParcels({ page, pageSize: 20, status, search })
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
    onMutate: async ({ id, newStatus }) => {
      await qc.cancelQueries({ queryKey: ['land-parcel', id] })
      await qc.cancelQueries({ queryKey: ['land-parcels'] })

      const previousDetail = qc.getQueryData(['land-parcel', id])
      const previousLists = qc.getQueriesData({ queryKey: ['land-parcels'] })

      qc.setQueryData(['land-parcel', id], (current: any) =>
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

      qc.setQueryData(['land-parcel', context.id], context.previousDetail)
      context.previousLists.forEach(([queryKey, data]) => {
        qc.setQueryData(queryKey, data)
      })
    },
    onSettled: (_d, _e, v) => {
      qc.invalidateQueries({ queryKey: ['land-parcel', v.id] })
      qc.invalidateQueries({ queryKey: ['land-parcels'] })
    },
  })
}
