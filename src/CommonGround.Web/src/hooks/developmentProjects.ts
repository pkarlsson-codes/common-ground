import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { DevelopmentProjectsApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import type { InitiateDevelopmentProjectRequest } from './types'

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

async function fetchJson<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(url, init)
  if (!res.ok) throw new Error(`${res.status}`)
  if (res.status === 204) return undefined as unknown as T
  return res.json()
}

export function useDevelopmentProjects(params: { pageNumber: number; pageSize?: number; filterSearch?: string }) {
  return useQuery({
    queryKey: ['development-projects', params.pageNumber, params.filterSearch],
    queryFn: () => api.getDevelopmentProjects({
      pageNumber: params.pageNumber,
      pageSize: params.pageSize ?? 20,
      filterSearch: params.filterSearch,
    }),
  })
}

export function useDevelopmentProject(id: string | null) {
  return useQuery<DevelopmentProjectDetail>({
    queryKey: ['development-project', id],
    queryFn: () => fetchJson(`/api/development-projects/${id}`),
    enabled: !!id,
  })
}

export function useInitiateDevelopmentProject() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (req: InitiateDevelopmentProjectRequest) =>
      fetchJson<{ id: string }>('/api/development-projects', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(req),
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['development-projects'] }),
  })
}

export function useActivateDevelopmentProject() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) =>
      fetchJson<void>(`/api/development-projects/${id}/activate`, { method: 'POST' }),
    onSuccess: (_d, id) => {
      qc.invalidateQueries({ queryKey: ['development-project', id] })
      qc.invalidateQueries({ queryKey: ['development-projects'] })
    },
  })
}

export function useCompleteDevelopmentProject() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, actualCompletionDate }: { id: string; actualCompletionDate: string }) =>
      fetchJson<void>(`/api/development-projects/${id}/complete`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ actualCompletionDate }),
      }),
    onSuccess: (_d, v) => {
      qc.invalidateQueries({ queryKey: ['development-project', v.id] })
      qc.invalidateQueries({ queryKey: ['development-projects'] })
    },
  })
}

export function useAbandonDevelopmentProject() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) =>
      fetchJson<void>(`/api/development-projects/${id}/abandon`, { method: 'POST' }),
    onSuccess: (_d, id) => {
      qc.invalidateQueries({ queryKey: ['development-project', id] })
      qc.invalidateQueries({ queryKey: ['development-projects'] })
    },
  })
}
