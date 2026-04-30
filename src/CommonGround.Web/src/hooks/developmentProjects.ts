import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { InitiateDevelopmentProjectRequest } from './types'

interface DevelopmentProjectListItem {
  id: string
  landParcelId: string
  name: string
  status: string
  projectedEndDate?: string
}

interface DevelopmentProjectDetail extends DevelopmentProjectListItem {
  description?: string
  projectedStartDate?: string
  actualCompletionDate?: string
  budget?: number
  homeIds?: string[]
}

interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  totalPages: number
  page: number
  pageSize: number
}

async function fetchJson<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(url, init)
  if (!res.ok) throw new Error(`${res.status}`)
  if (res.status === 204) return undefined as unknown as T
  return res.json()
}

export function useDevelopmentProjects(params: { page: number; pageSize?: number; landParcelId?: string; status?: string }) {
  const qs = new URLSearchParams()
  qs.set('page', String(params.page))
  qs.set('pageSize', String(params.pageSize ?? 20))
  if (params.landParcelId) qs.set('landParcelId', params.landParcelId)
  if (params.status) qs.set('status', params.status)

  return useQuery<PaginatedResponse<DevelopmentProjectListItem>>({
    queryKey: ['development-projects', params.page, params.landParcelId, params.status],
    queryFn: () => fetchJson(`/api/development-projects?${qs}`),
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
