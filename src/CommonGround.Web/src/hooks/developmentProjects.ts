import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { DevelopmentProjectsApi } from '../api'
import type { InitiateDevelopmentProjectRequest } from '../api'
import { apiConfig } from '../lib/apiConfig'

const api = new DevelopmentProjectsApi(apiConfig)

interface DevelopmentProjectDetail {
  id: string
  landParcelId: string
  description?: string
  status: string
  projectedStartDate?: string
  projectedEndDate?: string
  actualCompletionDate?: string
  budget?: number
  homeIds?: string[]
}

export function useDevelopmentProjects(params: { pageNumber: number; pageSize?: number; search?: string; landParcelId?: string }) {
  return useQuery({
    queryKey: ['development-projects', params.pageNumber, params.search, params.landParcelId],
    queryFn: () => api.getDevelopmentProjects({
      pageNumber: params.pageNumber,
      pageSize: params.pageSize ?? 20,
      search: params.search,
      landParcelId: params.landParcelId,
    }),
  })
}

export function useDevelopmentProject(id: string | null) {
  return useQuery<DevelopmentProjectDetail>({
    queryKey: ['development-project', id],
    queryFn: async () => {
      if (!id) return undefined as unknown as DevelopmentProjectDetail
      // Gateway doesn't expose a single-item GET; fall back to listing and pick the item
      const res = await api.getDevelopmentProjects({ pageNumber: 1, pageSize: 50 })
      const item = (res.items ?? []).find((p: any) => p.id === id)
      return (item ?? (undefined as unknown)) as DevelopmentProjectDetail
    },
    enabled: !!id,
  })
}

export function useInitiateDevelopmentProject() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (req: InitiateDevelopmentProjectRequest) =>
      api.initiateDevelopmentProject({ initiateDevelopmentProjectRequest: req }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['development-projects'] }),
  })
}

