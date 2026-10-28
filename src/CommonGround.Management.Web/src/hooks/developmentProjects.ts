import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { DevelopmentProjectsApi } from '../api'
import type { InitiateDevelopmentProjectRequest } from '../api'
import { apiConfig } from '../lib/apiConfig'

const api = new DevelopmentProjectsApi(apiConfig)

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
  return useQuery({
    queryKey: ['development-project', id],
    queryFn: () => api.getDevelopmentProject({ projectId: id! }),
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

